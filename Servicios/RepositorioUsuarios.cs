using Microsoft.Data.SqlClient;
using NSIE.Models;
using Dapper;
using System.Data;
using System.Data.SqlTypes;

namespace NSIE.Servicios
{
    // ============================
    // INTERFAZ DEL REPOSITORIO DE USUARIOS
    // ============================
    public interface IRepositorioUsuarios
    {
        // Usuarios
        Task<IEnumerable<UserViewModel>> ObtenerListadeUsuarios();
        Task<UserViewModel> ObtenerUsuarioPorId(int id);
        Task<bool> ActualizarUsuario(UserViewModel usuario);
        Task<int> RegistraUsuario(UserViewModel nuevoUsuario);
        Task<bool> EliminarUsuario(int id);
        Task<UserViewModel> BuscarPorCorreo(string email);

        // Roles y Mercados
        Task<IEnumerable<Rol>> ObtenerTodosLosRoles();
        Task<IEnumerable<Mercado>> ObtenerTodosLosMercados();
        Task<bool> ActualizarRolUsuario(RolesUsuarioViewModel rolUsuario);
        Task<bool> RegistraRolUsuario(RolesUsuarioViewModel rolUsuario);

        // Notificaciones
        Task<List<Notificacion>> GetNotificationsByUserIdAsync(int userId);
        Task<int> GetUnreadNotificationsCountAsync(int userId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId);
        Task<IEnumerable<Notificacion>> GetAllNotificationsAsync(int userId);
        Task<bool> GenerateNotificationsScriptAsync();
        Task<Notificacion> ObtenerNotificacionPorId(int id);
        Task<bool> GuardarNotificacionScriptAsync(Notificacion model);
        Task<bool> DeleteNotificationAsync(int notificationId);

        // Créditos
        Task<IEnumerable<Credito>> ObtenerCreditos();
        Task<Credito> ObtenerCreditoPorId(int creditoId);

        // Encuestas
        Task InsertarEncuesta(Encuesta encuesta);

        // Métodos de prueba/otros
        Task<UsuarioApp> BuscarUsuarioPorEmail(string emailNomarlizado);
        Task<int> CrearUsuario(UsuarioApp usuario);
    }

    // ============================
    // IMPLEMENTACIÓN DEL REPOSITORIO DE USUARIOS
    // ============================
    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        private readonly string connectionString;
        private readonly ILogger<RepositorioUsuarios> _logger;

        private const string SelectUsuarioDetallado = @"
            SELECT
                u.[IdUsuario],
                u.[Correo],
                u.[ClaveHash] AS [Clave],
                u.[Nombre],
                u.[UnidadAdscripcion] AS [Unidad_de_Adscripcion],
                u.[Cargo],
                CAST(CASE WHEN ISNULL(s.[Activa], 0) = 1 THEN 1 ELSE 0 END AS bit) AS [SesionActiva],
                ISNULL(s.[UltimaActividad], u.[FechaActualizacion]) AS [UltimaActualizacion],
                u.[RFC],
                u.[Vigente],
                u.[ClaveEmpleado],
                ISNULL(s.[FechaInicio], u.[FechaAlta]) AS [HoraInicioSesion],
                ISNULL(ur.[RolId], 0) AS [Rol],
                ISNULL(ur.[MercadoId], 0) AS [Mercado_ID],
                CAST(ISNULL(ur.[Vigente], 0) AS bit) AS [RolUsuario_Vigente],
                CAST(ISNULL(ur.[QuienRegistro], 0) AS nvarchar(50)) AS [RolUsuario_QuienRegistro],
                ISNULL(ur.[FechaModificacion], u.[FechaActualizacion]) AS [RolUsuario_FechaMod],
                ur.[Comentarios] AS [RolUsuario_Comentarios],
                ISNULL(r.[RolId], 0) AS [Rol_ID],
                r.[RolNombre] AS [Rol_Nombre],
                r.[RolClave] AS [Rol_Clave],
                CAST(ISNULL(r.[RolVigente], 0) AS bit) AS [Rol_Vigente],
                ISNULL(r.[RolFechaMod], u.[FechaActualizacion]) AS [Rol_FechaMod],
                r.[RolComentario] AS [Rol_Comentario],
                ISNULL(m.[MercadoId], 0) AS [Mercado_ID_M],
                m.[MercadoNombre] AS [Mercado_Nombre],
                CAST(ISNULL(m.[MercadoVigente], 0) AS bit) AS [Mercado_Vigente],
                ISNULL(m.[MercadoFechaMod], u.[FechaActualizacion]) AS [Mercado_FechaMod],
                m.[MercadoComentario] AS [Mercado_Comentario]
            FROM [dgmesnie].[Usuario] u
            OUTER APPLY
            (
                SELECT TOP (1)
                    ur0.[RolId],
                    ur0.[MercadoId],
                    ur0.[Vigente],
                    ur0.[QuienRegistro],
                    ur0.[FechaModificacion],
                    ur0.[Comentarios]
                FROM [dgmesnie].[UsuarioRol] ur0
                WHERE ur0.[IdUsuario] = u.[IdUsuario]
                ORDER BY ur0.[Vigente] DESC, ur0.[FechaModificacion] DESC, ur0.[UsuarioRolId] DESC
            ) ur
            LEFT JOIN [dgmesnie].[Rol] r ON r.[RolId] = ur.[RolId]
            LEFT JOIN [dgmesnie].[Mercado] m ON m.[MercadoId] = ur.[MercadoId]
            OUTER APPLY
            (
                SELECT TOP (1)
                    s0.[Activa],
                    s0.[UltimaActividad],
                    s0.[FechaInicio]
                FROM [dgmesnie].[Sesion] s0
                WHERE s0.[IdUsuario] = u.[IdUsuario]
                ORDER BY s0.[Activa] DESC, s0.[UltimaActividad] DESC
            ) s";

        public RepositorioUsuarios(IConfiguration configuration, ILogger<RepositorioUsuarios> logger)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        // ============================
        // USUARIOS
        // ============================

        // Obtiene la lista de usuarios
        public async Task<IEnumerable<UserViewModel>> ObtenerListadeUsuarios()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var query = SelectUsuarioDetallado + " ORDER BY u.[IdUsuario] DESC";
                    var usuarios = await connection.QueryAsync<UserViewModel>(query);
                    return usuarios;
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError($"Error de SQL: {ex.Message}");
                throw;
            }
        }

        // Obtiene un usuario por su ID
        public async Task<UserViewModel> ObtenerUsuarioPorId(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var user = await connection.QuerySingleOrDefaultAsync<UserViewModel>(
                    SelectUsuarioDetallado + " WHERE u.[IdUsuario] = @IdUsuario",
                    new { IdUsuario = id }
                );
                return user;
            }
        }

        // Actualiza los datos de un usuario
        public async Task<bool> ActualizarUsuario(UserViewModel usuario)
        {
            var sql = @"UPDATE [dgmesnie].[Usuario]
                        SET [Nombre] = @Nombre,
                            [Correo] = @Correo,
                            [RFC] = @RFC,
                            [Cargo] = @Cargo,
                            [UnidadAdscripcion] = @Unidad_de_Adscripcion,
                            [ClaveEmpleado] = @ClaveEmpleado,
                            [Vigente] = @Vigente,
                            [FechaActualizacion] = SYSUTCDATETIME()
                        WHERE [IdUsuario] = @IdUsuario;";
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var result = await connection.ExecuteAsync(sql, usuario);
                return result > 0;
            }
        }

        // Registra un nuevo usuario y retorna su ID
        public async Task<int> RegistraUsuario(UserViewModel nuevoUsuario)
        {
            var sql = @"INSERT INTO [dgmesnie].[Usuario]
                        ([Nombre], [Correo], [RFC], [Cargo], [UnidadAdscripcion], [ClaveEmpleado], [Vigente], [ClaveHash], [FechaAlta], [FechaActualizacion])
                        VALUES (@Nombre, @Correo, @RFC, @Cargo, @Unidad_de_Adscripcion, @ClaveEmpleado, @Vigente, @Clave, SYSUTCDATETIME(), SYSUTCDATETIME());
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var newUserId = await connection.QuerySingleAsync<int>(sql, nuevoUsuario);
                return newUserId;
            }
        }

        // Elimina un usuario y sus roles asociados (transaccional)
        public async Task<bool> EliminarUsuario(int usuarioId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Elimina roles asociados
                        var sqlRolesUsuario = "DELETE FROM [dgmesnie].[UsuarioRol] WHERE [IdUsuario] = @IdUsuario;";
                        await connection.ExecuteAsync(sqlRolesUsuario, new { IdUsuario = usuarioId }, transaction);

                        // Elimina recuperación de contraseña
                        var sqlRecuperarContraseña = "DELETE FROM [dgmesnie].[RecuperacionContrasena] WHERE [IdUsuario] = @IdUsuario;";
                        await connection.ExecuteAsync(sqlRecuperarContraseña, new { IdUsuario = usuarioId }, transaction);

                        var sqlNotificaciones = "DELETE FROM [dgmesnie].[Notificacion] WHERE [IdUsuario] = @IdUsuario;";
                        await connection.ExecuteAsync(sqlNotificaciones, new { IdUsuario = usuarioId }, transaction);

                        var sqlActividad = "DELETE FROM [dgmesnie].[ActividadLog] WHERE [IdUsuario] = @IdUsuario;";
                        await connection.ExecuteAsync(sqlActividad, new { IdUsuario = usuarioId }, transaction);

                        var sqlAccesos = "DELETE FROM [dgmesnie].[Acceso] WHERE [IdUsuario] = @IdUsuario;";
                        await connection.ExecuteAsync(sqlAccesos, new { IdUsuario = usuarioId }, transaction);

                        var sqlSesiones = "DELETE FROM [dgmesnie].[Sesion] WHERE [IdUsuario] = @IdUsuario;";
                        await connection.ExecuteAsync(sqlSesiones, new { IdUsuario = usuarioId }, transaction);

                        // Elimina al usuario
                        var sqlUsuario = "DELETE FROM [dgmesnie].[Usuario] WHERE [IdUsuario] = @IdUsuario;";
                        await connection.ExecuteAsync(sqlUsuario, new { IdUsuario = usuarioId }, transaction);

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        try { transaction.Rollback(); } catch { }
                        return false;
                    }
                }
            }
        }

        // ============================
        // ROLES Y MERCADOS
        // ============================

        // Obtiene todos los roles
        public async Task<IEnumerable<Rol>> ObtenerTodosLosRoles()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var roles = await connection.QueryAsync<Rol>("SELECT [RolId] AS [Rol_ID], [RolNombre] AS [Rol_Nombre] FROM [dgmesnie].[Rol] WHERE [RolVigente] = 1 ORDER BY [RolNombre]");
                return roles;
            }
        }

        // Obtiene todos los mercados
        public async Task<IEnumerable<Mercado>> ObtenerTodosLosMercados()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var mercados = await connection.QueryAsync<Mercado>("SELECT [MercadoId] AS [Mercado_ID], [MercadoNombre] AS [Mercado_Nombre] FROM [dgmesnie].[Mercado] WHERE [MercadoVigente] = 1 ORDER BY [MercadoNombre]");
                return mercados;
            }
        }

        // Actualiza el rol de un usuario
        public async Task<bool> ActualizarRolUsuario(RolesUsuarioViewModel rolUsuario)
        {
            var sql = @"
                                                UPDATE [dgmesnie].[UsuarioRol]
                                                SET [Vigente] = 0,
                                                        [FechaModificacion] = SYSUTCDATETIME()
                                                WHERE [IdUsuario] = @IdUsuario
                                                    AND [Vigente] = 1;

                                                INSERT INTO [dgmesnie].[UsuarioRol]
                                                ([IdUsuario], [RolId], [MercadoId], [Vigente], [QuienRegistro], [FechaModificacion], [Comentarios])
                                                VALUES (@IdUsuario, @Rol_ID, NULLIF(@Mercado_ID, 0), @RolUsuario_Vigente, NULLIF(@RolUsuario_QuienRegistro, 0),
                                                                ISNULL(@RolUsuario_FechaMod, SYSUTCDATETIME()), @RolUsuario_Comentarios);";

            var fechaRol = rolUsuario.RolUsuario_FechaMod >= (DateTime)SqlDateTime.MinValue
                ? rolUsuario.RolUsuario_FechaMod
                : (DateTime?)null;

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var result = await connection.ExecuteAsync(sql, new
                {
                    rolUsuario.IdUsuario,
                    rolUsuario.Rol_ID,
                    rolUsuario.Mercado_ID,
                    RolUsuario_Vigente = rolUsuario.RolUsuario_Vigente == 0 ? 1 : rolUsuario.RolUsuario_Vigente,
                    rolUsuario.RolUsuario_QuienRegistro,
                    RolUsuario_FechaMod = fechaRol,
                    rolUsuario.RolUsuario_Comentarios
                });
                return result > 0;
            }
        }

        // Registra el rol de un usuario
        public async Task<bool> RegistraRolUsuario(RolesUsuarioViewModel rolUsuario)
        {
            var sql = @"INSERT INTO [dgmesnie].[UsuarioRol] ([IdUsuario], [RolId], [MercadoId], [Vigente], [QuienRegistro], [FechaModificacion], [Comentarios]) 
                        VALUES (@IdUsuario, @Rol_ID, NULLIF(@Mercado_ID, 0), @RolUsuario_Vigente, NULLIF(@RolUsuario_QuienRegistro, 0), ISNULL(@RolUsuario_FechaMod, SYSUTCDATETIME()), @RolUsuario_Comentarios);";

            var fechaRol = rolUsuario.RolUsuario_FechaMod >= (DateTime)SqlDateTime.MinValue
                ? rolUsuario.RolUsuario_FechaMod
                : (DateTime?)null;

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var affectedRows = await connection.ExecuteAsync(sql, new
                {
                    rolUsuario.IdUsuario,
                    rolUsuario.Rol_ID,
                    rolUsuario.Mercado_ID,
                    RolUsuario_Vigente = rolUsuario.RolUsuario_Vigente == 0 ? 1 : rolUsuario.RolUsuario_Vigente,
                    rolUsuario.RolUsuario_QuienRegistro,
                    RolUsuario_FechaMod = fechaRol,
                    rolUsuario.RolUsuario_Comentarios
                });
                return affectedRows > 0;
            }
        }

        // ============================
        // NOTIFICACIONES
        // ============================

        // Obtiene las notificaciones no vistas (máx 4) de un usuario
        public async Task<List<Notificacion>> GetNotificationsByUserIdAsync(int userId)
        {
            var notificaciones = new List<Notificacion>();
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT TOP 4
                        [IdNotificacion] AS [ID],
                        [Titulo] AS [Titulo_Notificacion], 
                        [Mensaje], 
                        [FechaNotificacion] AS [Fecha_Notificacion], 
                        [Link],
                        [IdUsuario] AS [ID_Usuario]
                    FROM [dgmesnie].[Notificacion]
                    WHERE [IdUsuario] = @UserId AND [Visto] = 0 AND [Activo] = 1
                    ORDER BY [FechaNotificacion] DESC";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var notificacion = new Notificacion
                        {
                            ID = reader.GetInt32(reader.GetOrdinal("ID")),
                            Titulo_Notificacion = reader.GetString(reader.GetOrdinal("Titulo_Notificacion")),
                            Mensaje = reader.GetString(reader.GetOrdinal("Mensaje")),
                            Fecha_Notificacion = reader.GetDateTime(reader.GetOrdinal("Fecha_Notificacion")),
                            Link = reader.GetString(reader.GetOrdinal("Link")),
                            ID_Usuario = reader.GetInt32(reader.GetOrdinal("ID_Usuario"))
                        };
                        notificaciones.Add(notificacion);
                    }
                }
            }
            return notificaciones;
        }

        // Cuenta de notificaciones no leídas
        public async Task<int> GetUnreadNotificationsCountAsync(int userId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT COUNT(*) FROM [dgmesnie].[Notificacion] WHERE [IdUsuario] = @UserId AND [Visto] = 0 AND [Activo] = 1";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);
                return (int)await command.ExecuteScalarAsync();
            }
        }

        // Marca una notificación como leída
        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"
                        UPDATE [dgmesnie].[Notificacion]
                        SET [Visto] = 1, [FechaVisto] = @FechaVisto
                        WHERE [IdNotificacion] = @NotificationId";
                    var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@FechaVisto", DateTime.Now);
                    command.Parameters.AddWithValue("@NotificationId", notificationId);
                    int affectedRows = await command.ExecuteNonQueryAsync();
                    return affectedRows > 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Obtiene todas las notificaciones de un usuario
        public async Task<IEnumerable<Notificacion>> GetAllNotificationsAsync(int userId)
        {
            var notificaciones = new List<Notificacion>();
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT 
                        [IdNotificacion] AS [ID], 
                        [GuidNotificacion] AS [ID_Notificacion], 
                        [Titulo] AS [Titulo_Notificacion], 
                        Mensaje, 
                        [FechaNotificacion] AS [Fecha_Notificacion], 
                        Link, 
                        [IdUsuario] AS [ID_Usuario], 
                        Visto, 
                        [FechaVisto] AS [Fecha_Visto],
                        Imagen
                    FROM [dgmesnie].[Notificacion]
                    WHERE [IdUsuario] = @UserId AND [Activo] = 1
                    ORDER BY [FechaNotificacion] DESC";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var notificacion = new Notificacion
                        {
                            ID = reader.GetInt32(reader.GetOrdinal("ID")),
                            ID_Notificacion = reader.GetGuid(reader.GetOrdinal("ID_Notificacion")),
                            Titulo_Notificacion = reader.GetString(reader.GetOrdinal("Titulo_Notificacion")),
                            Mensaje = reader.GetString(reader.GetOrdinal("Mensaje")),
                            Fecha_Notificacion = reader.GetDateTime(reader.GetOrdinal("Fecha_Notificacion")),
                            Link = reader.GetString(reader.GetOrdinal("Link")),
                            ID_Usuario = reader.GetInt32(reader.GetOrdinal("ID_Usuario")),
                            Visto = reader.GetBoolean(reader.GetOrdinal("Visto")),
                            Fecha_Visto = reader.IsDBNull(reader.GetOrdinal("Fecha_Visto")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("Fecha_Visto")),
                            Imagen = reader.IsDBNull(reader.GetOrdinal("Imagen")) ? null : reader.GetString(reader.GetOrdinal("Imagen"))
                        };
                        notificaciones.Add(notificacion);
                    }
                }
            }
            return notificaciones;
        }

        // Genera notificaciones de prueba para los usuarios
        public async Task<bool> GenerateNotificationsScriptAsync()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var script = @"DECLARE @ID_Usuario INT;
                DECLARE @Titulo_Notificacion NVARCHAR(255) = 'Notificación de prueba';
                DECLARE @Mensaje NVARCHAR(MAX) = 'Este es un mensaje de prueba para el funcionamiento de las notificaciones en el sistema SNIER (Se anexan documentos e imagen de ejemplo)';
                DECLARE @Fecha_Notificacion DATETIME = GETDATE();
                DECLARE @Link NVARCHAR(255) = '/documentos/necesidades/Listado_Necesidades.pdf';
                DECLARE @Visto BIT = 0;
                DECLARE @Fecha_Visto DATETIME = NULL;
                DECLARE @Imagen NVARCHAR(255) = '/img/codigo.png';
                DECLARE UserCursor CURSOR FOR
                SELECT TOP 500 [IdUsuario] FROM [dgmesnie].[Usuario] WHERE [Vigente] = 1 ORDER BY [IdUsuario];
                OPEN UserCursor;
                FETCH NEXT FROM UserCursor INTO @ID_Usuario;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM [dgmesnie].[Notificacion]
                        WHERE [IdUsuario] = @ID_Usuario
                        AND [Titulo] = @Titulo_Notificacion
                        AND [Mensaje] = @Mensaje
                    )
                    BEGIN
                        INSERT INTO [dgmesnie].[Notificacion]
                            ([GuidNotificacion], [Titulo], [Mensaje], [FechaNotificacion], [Link], [IdUsuario], [Visto], [FechaVisto], [Imagen], [Activo])
                        VALUES 
                            (NEWID(), @Titulo_Notificacion, @Mensaje, @Fecha_Notificacion, @Link, @ID_Usuario, @Visto, @Fecha_Visto, @Imagen, 1);
                    END
                    FETCH NEXT FROM UserCursor INTO @ID_Usuario;
                END
                CLOSE UserCursor;
                DEALLOCATE UserCursor;";
                using (var command = new SqlCommand(script, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                return true;
            }
        }

        //CREACION DE NOTIFICACIONES
        public async Task<bool> GuardarNotificacionScriptAsync(Notificacion model)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string filtro = "";

                    if (model.Destino == "Rol" && !string.IsNullOrEmpty(model.Rol))
                    {
                        filtro = @"
                            INNER JOIN [dgmesnie].[UsuarioRol] ru ON u.[IdUsuario] = ru.[IdUsuario] AND ru.[Vigente] = 1
                            WHERE ru.[RolId] = @Rol AND u.[Vigente] = 1";
                    }
                    else if (model.Destino == "Usuarios" && model.UsuariosSeleccionados != null && model.UsuariosSeleccionados.Any())
                    {
                        filtro = "WHERE u.[IdUsuario] IN (SELECT TRY_CAST(value AS int) FROM STRING_SPLIT(@UsuariosSeleccionados, ',')) AND u.[Vigente] = 1";
                    }
                    else
                    {
                        filtro = "WHERE u.[Vigente] = 1"; // Todos
                    }

                    var script = $@"
                        DECLARE @Fecha_Notificacion DATETIME = GETDATE();
                        DECLARE @Visto BIT = 0;
                        DECLARE @Fecha_Visto DATETIME = NULL;

                        INSERT INTO [dgmesnie].[Notificacion]
                            ([GuidNotificacion], [Titulo], [Mensaje], [FechaNotificacion], [Link], [IdUsuario], [Visto], [FechaVisto], [Imagen], [Activo])
                        SELECT NEWID(), @Titulo, @Mensaje, @Fecha_Notificacion, @Link, u.[IdUsuario], @Visto, @Fecha_Visto, @Imagen, 1
                        FROM [dgmesnie].[Usuario] u
                        {filtro};
                    ";

                    using (var command = new SqlCommand(script, connection))
                    {
                        command.Parameters.AddWithValue("@Titulo", model.Titulo_Notificacion);
                        command.Parameters.AddWithValue("@Mensaje", model.Mensaje);
                        command.Parameters.AddWithValue("@Link", (object)model.Link ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Imagen", (object)model.Imagen ?? "/img/notificacion.png");
                        if (model.Destino == "Rol")
                            command.Parameters.AddWithValue("@Rol", model.Rol);

                        if (model.Destino == "Usuarios")
                            command.Parameters.AddWithValue("@UsuariosSeleccionados", string.Join(",", model.UsuariosSeleccionados));

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return true; // vuelve al listado de usuarios
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    Console.WriteLine("Notification: " + notificationId);

                    var query = "DELETE FROM [dgmesnie].[Notificacion] WHERE [IdNotificacion] = @Id";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", notificationId);
                        var rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        // Obtiene el detalle de una notificación por su ID
        public async Task<Notificacion> ObtenerNotificacionPorId(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT 
                    [IdNotificacion] AS [ID],
                    [GuidNotificacion] AS [ID_Notificacion], 
                    [Titulo] AS [Titulo_Notificacion], 
                    [Mensaje], 
                    [FechaNotificacion] AS [Fecha_Notificacion], 
                    [Link], 
                    [IdUsuario] AS [ID_Usuario], 
                    [Visto], 
                    [FechaVisto] AS [Fecha_Visto],
                    [Imagen]
                FROM [dgmesnie].[Notificacion]
                WHERE [IdNotificacion] = @Id";
                return await connection.QuerySingleOrDefaultAsync<Notificacion>(query, new { Id = id });
            }
        }

        // ============================
        // CRÉDITOS
        // ============================

        // Obtiene todos los créditos
        public async Task<IEnumerable<Credito>> ObtenerCreditos()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"SELECT CreditoID, Nombre, Cargo, ImagenUrl, Seccion, PaginaWeb, Actividades, Resena FROM Creditos";
                var creditos = await connection.QueryAsync<Credito>(query);
                return creditos;
            }
        }

        // Obtiene un crédito por su ID
        public async Task<Credito> ObtenerCreditoPorId(int creditoId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT * FROM Creditos WHERE CreditoID = @CreditoID";
                var credito = await connection.QuerySingleOrDefaultAsync<Credito>(query, new { CreditoID = creditoId });
                return credito;
            }
        }

        // ============================
        // ENCUESTAS
        // ============================

        // Inserta una encuesta de usuario
        public async Task InsertarEncuesta(Encuesta encuesta)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Correo", encuesta.Correo);
                parameters.Add("@Nombre", encuesta.Nombre);
                parameters.Add("@EncontroInformacion", encuesta.EncontroInformacion);
                parameters.Add("@FueUtil", encuesta.FueUtil);
                parameters.Add("@InformacionBuscada", encuesta.InformacionBuscada);
                parameters.Add("@CalificacionExperiencia", encuesta.CalificacionExperiencia);
                parameters.Add("@AgregarComentario", encuesta.AgregarComentario);
                parameters.Add("@ComentarioAdicional", encuesta.ComentarioAdicional);

                await conn.ExecuteAsync("InsertarEncuesta", parameters, commandType: CommandType.StoredProcedure);
            }
        }

        // ============================
        // MÉTODOS DE PRUEBA / OTROS
        // ============================

        // Crea un usuario de prueba
        public async Task<int> CrearUsuario(UsuarioApp usuario)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"
                INSERT INTO [dgmesnie].[Usuario]
                   ([Correo], [ClaveHash], [Nombre], [Vigente], [FechaAlta], [FechaActualizacion])
                OUTPUT INSERTED.[IdUsuario]
                VALUES (@Email, @PasswordHash, @Nombre, 1, SYSUTCDATETIME(), SYSUTCDATETIME())
            ", new
            {
                usuario.Email,
                usuario.PasswordHash,
                Nombre = string.IsNullOrWhiteSpace(usuario.Usuario) ? usuario.Email : usuario.Usuario
            });

            usuario.Id = id;
            return id;
        }

        // Busca un usuario de prueba por email normalizado
        public async Task<UsuarioApp> BuscarUsuarioPorEmail(string emailNomarlizado)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QuerySingleOrDefaultAsync<UsuarioApp>(
                @"SELECT TOP (1)
                        [IdUsuario] AS [Id],
                        [Nombre] AS [Usuario],
                        [Correo] AS [Email],
                        UPPER([Correo]) AS [EmailNormalizado],
                        [ClaveHash] AS [PasswordHash]
                  FROM [dgmesnie].[Usuario]
                  WHERE [Vigente] = 1
                    AND UPPER([Correo]) = @emailNomarlizado",
                new { emailNomarlizado = emailNomarlizado?.ToUpperInvariant() }
            );
        }

        public async Task<UserViewModel> BuscarPorCorreo(string email)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QuerySingleOrDefaultAsync<UserViewModel>(
                SelectUsuarioDetallado + " WHERE u.[Correo] = @email",
                new { email }
            );
        }
    }
}

