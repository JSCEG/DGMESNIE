using Microsoft.Data.SqlClient;
using NSIE.Models;
using Dapper;
using System.Data;

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
                    var query = "EXEC [dbo].[spObtener_ListaUsuarios]";
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
                    "sp_ObtenerUsuarioSession",
                    new { IdUsuario = id },
                    commandType: CommandType.StoredProcedure
                );
                return user;
            }
        }

        // Actualiza los datos de un usuario
        public async Task<bool> ActualizarUsuario(UserViewModel usuario)
        {
            var sql = @"UPDATE USUARIO 
                        SET Nombre = @Nombre, Correo = @Correo, RFC = @RFC, Cargo = @Cargo, Unidad_de_Adscripcion = @Unidad_de_Adscripcion, 
                            ClaveEmpleado = @ClaveEmpleado, SesionActiva = @SesionActiva, Vigente = @Vigente  
                        WHERE IdUsuario = @IdUsuario;";
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
            var sql = @"INSERT INTO USUARIO (Nombre, Correo, RFC, Cargo, Unidad_de_Adscripcion, ClaveEmpleado, SesionActiva, Vigente, Clave) 
                        VALUES (@Nombre, @Correo, @RFC, @Cargo, @Unidad_de_Adscripcion, @ClaveEmpleado, @SesionActiva, @Vigente, @Clave);
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
                        var sqlRolesUsuario = "DELETE FROM [Roles_Usuarios] WHERE IdUsuario = @IdUsuario;";
                        await connection.ExecuteAsync(sqlRolesUsuario, new { IdUsuario = usuarioId }, transaction);

                        // Elimina del Log de evaluaciones
                        var sqlLogEvaluaciones = "DELETE FROM [LogEvaluaciones] WHERE IdUsuario = @IdUsuario;";
                        await connection.ExecuteAsync(sqlLogEvaluaciones, new { IdUsuario = usuarioId }, transaction);

                        // Elimina recuperación de contraseña
                        var sqlRecuperarContraseña = "DELETE FROM [Recuperar_contrasena] WHERE IdUsuario = @IdUsuario;";
                        await connection.ExecuteAsync(sqlRecuperarContraseña, new { IdUsuario = usuarioId }, transaction);

                        // Elimina comentarios de proyectos estratégicos
                        var sqlComentariosPE = "DELETE FROM [ComentariosProyectosEstrategicos] WHERE IdUsuario = @IdUsuario;";
                        await connection.ExecuteAsync(sqlComentariosPE, new { IdUsuario = usuarioId }, transaction);

                        // Elimina al usuario
                        var sqlUsuario = "DELETE FROM [USUARIO] WHERE IdUsuario = @IdUsuario;";
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
                var roles = await connection.QueryAsync<Rol>("SELECT * FROM Roles");
                return roles;
            }
        }

        // Obtiene todos los mercados
        public async Task<IEnumerable<Mercado>> ObtenerTodosLosMercados()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var mercados = await connection.QueryAsync<Mercado>("SELECT * FROM Mercados");
                return mercados;
            }
        }

        // Actualiza el rol de un usuario
        public async Task<bool> ActualizarRolUsuario(RolesUsuarioViewModel rolUsuario)
        {
            var sql = @"UPDATE Roles_Usuarios 
                        SET Rol_ID = @Rol_ID, Mercado_ID = @Mercado_ID, RolUsuario_Comentarios = @RolUsuario_Comentarios 
                        WHERE IdUsuario = @IdUsuario;";
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var result = await connection.ExecuteAsync(sql, rolUsuario);
                return result > 0;
            }
        }

        // Registra el rol de un usuario
        public async Task<bool> RegistraRolUsuario(RolesUsuarioViewModel rolUsuario)
        {
            var sql = @"INSERT INTO Roles_Usuarios (IdUsuario, Rol_ID, Mercado_ID, RolUsuario_Vigente, RolUsuario_QuienRegistro, RolUsuario_FechaMod, RolUsuario_Comentarios) 
                        VALUES (@IdUsuario, @Rol_ID, @Mercado_ID, @RolUsuario_Vigente, @RolUsuario_QuienRegistro, @RolUsuario_FechaMod, @RolUsuario_Comentarios);";
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var affectedRows = await connection.ExecuteAsync(sql, rolUsuario);
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
                        ID,
                        Titulo_Notificacion, 
                        Mensaje, 
                        Fecha_Notificacion, 
                        Link,
                        ID_Usuario
                    FROM Notificaciones 
                    WHERE ID_Usuario = @UserId AND Visto = 0
                    ORDER BY Fecha_Notificacion DESC";
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
                var query = @"SELECT COUNT(*) FROM Notificaciones WHERE ID_Usuario = @UserId AND Visto = 0";
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
                        UPDATE Notificaciones
                        SET Visto = 1, Fecha_Visto = @FechaVisto
                        WHERE ID = @NotificationId";
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
                        ID, 
                        ID_Notificacion, 
                        Titulo_Notificacion, 
                        Mensaje, 
                        Fecha_Notificacion, 
                        Link, 
                        ID_Usuario, 
                        Visto, 
                        Fecha_Visto,
                        Imagen
                    FROM Notificaciones 
                    WHERE ID_Usuario = @UserId
                    ORDER BY Fecha_Notificacion DESC";
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
                SELECT TOP 500 IdUsuario FROM [dbo].[USUARIO] WHERE Vigente = 1 ORDER BY IdUsuario;
                OPEN UserCursor;
                FETCH NEXT FROM UserCursor INTO @ID_Usuario;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM [dbo].[Notificaciones]
                        WHERE [ID_Usuario] = @ID_Usuario
                        AND [Titulo_Notificacion] = @Titulo_Notificacion
                        AND [Mensaje] = @Mensaje
                    )
                    BEGIN
                        INSERT INTO [dbo].[Notificaciones] 
                            ([ID_Notificacion], [Titulo_Notificacion], [Mensaje], [Fecha_Notificacion], [Link], [ID_Usuario], [Visto], [Fecha_Visto], [Imagen])
                        VALUES 
                            (NEWID(), @Titulo_Notificacion, @Mensaje, @Fecha_Notificacion, @Link, @ID_Usuario, @Visto, @Fecha_Visto, @Imagen);
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
                            INNER JOIN [dbo].[Roles_Usuarios] ru ON u.IdUsuario = ru.IdUsuario
                            INNER JOIN [dbo].[Roles] r ON ru.Rol_ID = r.Rol_ID
                            WHERE r.Rol_ID = @Rol AND u.Vigente = 1";
                    }
                    else if (model.Destino == "Usuarios" && model.UsuariosSeleccionados != null && model.UsuariosSeleccionados.Any())
                    {
                        filtro = "WHERE u.IdUsuario IN (SELECT value FROM STRING_SPLIT(@UsuariosSeleccionados, ','))";
                    }
                    else
                    {
                        filtro = "WHERE u.Vigente = 1"; // Todos
                    }

                    var script = $@"
                        DECLARE @Fecha_Notificacion DATETIME = GETDATE();
                        DECLARE @Visto BIT = 0;
                        DECLARE @Fecha_Visto DATETIME = NULL;

                        INSERT INTO [dbo].[Notificaciones] 
                            ([ID_Notificacion], [Titulo_Notificacion], [Mensaje], [Fecha_Notificacion], [Link], [ID_Usuario], [Visto], [Fecha_Visto], [Imagen])
                        SELECT NEWID(), @Titulo, @Mensaje, @Fecha_Notificacion, @Link, u.IdUsuario, @Visto, @Fecha_Visto, @Imagen
                        FROM [dbo].[USUARIO] u
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
            catch (Exception ex)
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

                    var query = "DELETE FROM [dbo].[Notificaciones] WHERE ID = @Id";
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
                    ID_Notificacion, 
                    Titulo_Notificacion, 
                    Mensaje, 
                    Fecha_Notificacion, 
                    Link, 
                    ID_Usuario, 
                    Visto, 
                    Fecha_Visto 
                FROM Notificaciones 
                WHERE ID_Notificacion = @Id";
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
                INSERT INTO [dbo].[UsuariosPrueba]
                   ([Usuario], [Email], [EmailNormalizado], [PasswordHash])
                VALUES (@Usuario, @Email, @EmailNormalizado, @PasswordHash)
            ", usuario);
            return id;
        }

        // Busca un usuario de prueba por email normalizado
        public async Task<UsuarioApp> BuscarUsuarioPorEmail(string emailNomarlizado)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QuerySingleOrDefaultAsync<UsuarioApp>(
                "SELECT * FROM UsuariosPrueba Where EmailNormalizado=@emailNormalizado",
                new { emailNomarlizado }
            );
        }

        public async Task<UserViewModel> BuscarPorCorreo(string email)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QuerySingleOrDefaultAsync<UserViewModel>(
                "SELECT * FROM USUARIO Where Correo=@email",
                new { email }
            );
        }
    }
}

