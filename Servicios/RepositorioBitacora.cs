using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NSIE.Controllers;
using NSIE.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Data;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Web; // Asegúrate de tener esta referencia



namespace NSIE.Servicios
{
    public interface IRepositorioBitacora
    {
        Task RegistrarActividadAsync(string userId, string userName, string actionName, string controllerName, string pageName, string tipo, string elemento, string idElemento, string valor, string additionalData = null);
        Task<IEnumerable<UserActivityModel>> ObtenerUsuariosActivosAsync(DateTime timeThreshold);
        Task<IEnumerable<UserActivityModel>> ObtenerActividadPorPeriodoAsync(DateTime startDate, DateTime endDate);
        Task GenerarReporteDiarioAsync(IEnumerable<UserActivityModel> actividadDiaria);

        //Monitor de Acciones por Usuario
        Task<List<Usuario>> ObtenerUsuariosAsync();
        Task<List<UserActivityModel>> ObtenerActividadPorUsuarioAsync(int usuarioId, DateTime startDate, DateTime endDate);

        //Monitoreo General
        //Cuenta los Accesos ala plataforma
        Task<int> GetTotalAccessCountAsync();
        Task<List<AccesoDetalle>> GetDetallesAccesoAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<List<TipoAccesoTotal>> GetTotalAccessCountByTypeAsync();


        //Accesos por fecha
        Task<List<AccesoPorFecha>> GetAccesosPorFechaAsync(DateTime fechaInicio, DateTime fechaFin);

        Task<List<AccesoPorUsuario>> GetAccesosPorUsuarioAsync();
        Task<List<AccesoPorCargo>> GetAccesosPorCargoAsync();
        Task<List<AccesoPorUnidad>> GetAccesosPorUnidadAsync();
        Task<List<AccesoPorUsuario>> GetAccesosHoyPorUsuarioAsync();
        Task<List<UsuarioActivo>> ObtenerUsuariosActivosAsync();


        // Obtener las ultimas acciones del usuario
        Task<IEnumerable<UltimaAccionDto>> ObtenerUltimasAccionesAsync(int usuarioId, int top = 5);

        // Obtener historial de acciones por página y elemento
        Task<IEnumerable<UserActivityModel>> ObtenerHistorialPorElementoAsync(string pageName, string idElemento = null, int? userId = null);
    }




    public class RepositorioBitacora : IRepositorioBitacora
    {
        private readonly string connectionString;

        public RepositorioBitacora(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Obtiene las ultimas 5 acciones dle usuairo como shortcut para su entrada
        public async Task<IEnumerable<UltimaAccionDto>> ObtenerUltimasAccionesAsync(
                int usuarioId, int top = 5)
        {
            const string sql = "EXEC dbo.sp_ObtenerUltimasAccionesUsuario @IdUsuario, @Top";
            using var conn = new SqlConnection(connectionString);
            return await conn.QueryAsync<UltimaAccionDto>(sql,
                        new { IdUsuario = usuarioId, Top = top });
        }

        public async Task RegistrarActividadAsync(string userId, string userName, string actionName, string controllerName, string pageName, string tipo, string elemento, string idElemento, string valor, string additionalData = null)
        {
            try
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
                var localTimestamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

                // Decodificar valores
                actionName = HttpUtility.HtmlDecode(actionName);
                controllerName = HttpUtility.HtmlDecode(controllerName);
                pageName = HttpUtility.HtmlDecode(pageName);
                userName = HttpUtility.HtmlDecode(userName);
                additionalData = HttpUtility.HtmlDecode(additionalData);

                using (var connection = new SqlConnection(connectionString))
                {
                    var query = "INSERT INTO UserActivityLog (UserId, UserName, ActionName, ControllerName, PageName, Tipo, Elemento, IdElemento, Valor, Timestamp, AdditionalData) " +
                                "VALUES (@UserId, @UserName, @ActionName, @ControllerName, @PageName, @Tipo, @Elemento, @IdElemento, @Valor, @Timestamp, @AdditionalData)";

                    var parameters = new
                    {
                        UserId = userId,
                        UserName = userName,
                        ActionName = actionName,
                        ControllerName = controllerName,
                        PageName = pageName,
                        Tipo = tipo,
                        Elemento = elemento,
                        IdElemento = idElemento,
                        Valor = valor,
                        Timestamp = localTimestamp,
                        AdditionalData = additionalData
                    };

                    await connection.ExecuteAsync(query, parameters);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar la actividad: {ex.Message}");
                throw;
            }
        }

        // Obtener historial de acciones filtrado por página y elemento
        public async Task<IEnumerable<UserActivityModel>> ObtenerHistorialPorElementoAsync(string pageName, string idElemento = null, int? userId = null)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = @"SELECT [Id], [UserId], [UserName], [ActionName], [ControllerName], [PageName], 
                                         [Tipo], [Elemento], [IdElemento], [Valor], [Timestamp], [AdditionalData]
                                  FROM [dbo].[UserActivityLog]
                                  WHERE [PageName] = @PageName";

                    var parameters = new Dictionary<string, object>
                    {
                        { "@PageName", pageName }
                    };

                    if (!string.IsNullOrEmpty(idElemento))
                    {
                        query += " AND [IdElemento] = @IdElemento";
                        parameters.Add("@IdElemento", idElemento);
                    }

                    if (userId.HasValue)
                    {
                        query += " AND [UserId] = @UserId";
                        parameters.Add("@UserId", userId.Value.ToString());
                    }

                    query += " ORDER BY [Timestamp] DESC";

                    var historial = await connection.QueryAsync<UserActivityModel>(query, parameters);
                    return historial;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener historial por elemento: {ex.Message}");
                throw;
            }
        }




        public async Task<IEnumerable<UserActivityModel>> ObtenerUsuariosActivosAsync(DateTime timeThreshold)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = @"SELECT DISTINCT UserName, PageName, ActionName, Timestamp 
                              FROM UserActivityLog 
                              WHERE Timestamp >= @TimeThreshold";

                    var usuariosActivos = await connection.QueryAsync<UserActivityModel>(query, new { TimeThreshold = timeThreshold });

                    return usuariosActivos;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener usuarios activos: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<UserActivityModel>> ObtenerActividadPorPeriodoAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = @"SELECT UserId, UserName, ActionName, ControllerName, PageName, Tipo, Elemento, IdElemento, Valor, Timestamp, AdditionalData
                              FROM UserActivityLog 
                              WHERE Timestamp >= @StartDate AND Timestamp <= @EndDate";

                    var actividadPorPeriodo = await connection.QueryAsync<UserActivityModel>(query, new { StartDate = startDate, EndDate = endDate });

                    return actividadPorPeriodo;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener la actividad por periodo: {ex.Message}");
                throw;
            }
        }


        public async Task GenerarReporteDiarioAsync(IEnumerable<UserActivityModel> actividadDiaria)
        {
            // Implementa aquí la lógica para generar y enviar el reporte
            // Esto podría incluir la creación de un archivo PDF o Excel, y enviarlo por correo electrónico
            await Task.CompletedTask;
        }

        public async Task<List<Usuario>> ObtenerUsuariosAsync()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT [IdUsuario] , [Nombre] FROM [dbo].[USUARIO] WHERE [Vigente] = 1";
                var usuarios = await connection.QueryAsync<Usuario>(query);
                return usuarios.ToList();
            }
        }
        public async Task<List<UserActivityModel>> ObtenerActividadPorUsuarioAsync(int usuarioId, DateTime startDate, DateTime endDate)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
            SELECT [Id], [UserId], [UserName], [ActionName], [ControllerName], [PageName], 
                   [Tipo], [Elemento], [IdElemento], [Valor], [Timestamp], [AdditionalData]
            FROM [dbo].[UserActivityLog]
            WHERE [UserId] = @UsuarioId AND [Timestamp] BETWEEN @StartDate AND @EndDate
            ORDER BY [Timestamp] DESC";

                var actividad = await connection.QueryAsync<UserActivityModel>(query, new { UsuarioId = usuarioId, StartDate = startDate, EndDate = endDate });
                return actividad.ToList();
            }
        }


        // Para el Monitoreo
        // Obtener el total de accesos
        public async Task<int> GetTotalAccessCountAsync()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                return await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM [dbo].[Accesos]"
                );
            }

        }

        // Obtener detalles de acceso entre dos fechas
        public async Task<List<AccesoDetalle>> GetDetallesAccesoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var detallesAcceso = new List<AccesoDetalle>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                SELECT 
                    A.Id AS AccesoId,
                    U.Nombre,
                    A.TipoAcceso,
                    A.IP,
                    A.[FechaHoraLocal],
                    U.Unidad_de_Adscripcion,
                    U.Cargo
                FROM 
                    [dbo].[Accesos] A
                INNER JOIN 
                    [dbo].[USUARIO] U ON A.IdUsuario = U.IdUsuario
                WHERE 
                    A.[FechaHoraLocal] BETWEEN @FechaInicio AND @FechaFin
                ORDER BY 
                    A.[FechaHoraLocal] DESC;";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                command.Parameters.AddWithValue("@FechaFin", fechaFin);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var detalle = new AccesoDetalle
                        {
                            AccesoId = reader.GetInt32(reader.GetOrdinal("AccesoId")),
                            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                            TipoAcceso = reader.GetString(reader.GetOrdinal("TipoAcceso")),
                            IP = reader.GetString(reader.GetOrdinal("IP")),
                            FechaHoraLocal = reader.GetDateTime(reader.GetOrdinal("FechaHoraLocal")),
                            UnidadDeAdscripcion = reader.GetString(reader.GetOrdinal("Unidad_de_Adscripcion")),
                            Cargo = reader.GetString(reader.GetOrdinal("Cargo"))
                        };
                        detallesAcceso.Add(detalle);
                    }
                }
            }

            return detallesAcceso;
        }

        // Obtener el total de accesos por tipo
        public async Task<List<TipoAccesoTotal>> GetTotalAccessCountByTypeAsync()
        {
            var accesosPorTipo = new List<TipoAccesoTotal>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
        SELECT 
            A.TipoAcceso,
            COUNT(*) AS Total
        FROM 
            [dbo].[Accesos] A
        GROUP BY 
            A.TipoAcceso;";

                var command = new SqlCommand(query, connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var tipoAccesoTotal = new TipoAccesoTotal
                        {
                            TipoAcceso = reader.GetString(reader.GetOrdinal("TipoAcceso")),
                            Total = reader.GetInt32(reader.GetOrdinal("Total"))
                        };
                        accesosPorTipo.Add(tipoAccesoTotal);
                    }
                }
            }

            return accesosPorTipo;
        }


        public async Task<List<AccesoPorFecha>> GetAccesosPorFechaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var accesosPorFecha = new List<AccesoPorFecha>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
            SELECT 
                CAST(FechaHoraLocal AS DATE) AS Fecha,
                COUNT(*) AS TotalAccesos
            FROM 
                [dbo].[Accesos]
            WHERE 
                FechaHoraLocal BETWEEN @FechaInicio AND @FechaFin
            GROUP BY 
                CAST(FechaHoraLocal AS DATE)
            ORDER BY 
                Fecha ASC;";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                command.Parameters.AddWithValue("@FechaFin", fechaFin);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var accesoPorFecha = new AccesoPorFecha
                        {
                            Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                            TotalAccesos = reader.GetInt32(reader.GetOrdinal("TotalAccesos"))
                        };
                        accesosPorFecha.Add(accesoPorFecha);
                    }
                }
            }

            return accesosPorFecha;
        }


        public async Task<List<AccesoPorUsuario>> GetAccesosPorUsuarioAsync()
        {
            var accesosPorUsuario = new List<AccesoPorUsuario>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
            SELECT 
                U.Nombre AS Usuario,
                COUNT(*) AS TotalAccesos
            FROM 
                [dbo].[Accesos] A
            INNER JOIN 
                [dbo].[USUARIO] U ON A.IdUsuario = U.IdUsuario
            GROUP BY 
                U.Nombre
            ORDER BY 
                TotalAccesos DESC";

                var command = new SqlCommand(query, connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var accesoPorUsuario = new AccesoPorUsuario
                        {
                            Usuario = reader.GetString(reader.GetOrdinal("Usuario")),
                            TotalAccesos = reader.GetInt32(reader.GetOrdinal("TotalAccesos"))
                        };
                        accesosPorUsuario.Add(accesoPorUsuario);
                    }
                }
            }

            return accesosPorUsuario;
        }

        public async Task<List<AccesoPorCargo>> GetAccesosPorCargoAsync()
        {
            var accesosPorCargo = new List<AccesoPorCargo>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
            SELECT 
                U.Cargo AS Cargo,
                COUNT(*) AS TotalAccesos
            FROM 
                [dbo].[Accesos] A
            INNER JOIN 
                [dbo].[USUARIO] U ON A.IdUsuario = U.IdUsuario
            GROUP BY 
                U.Cargo
            ORDER BY 
                TotalAccesos DESC";

                var command = new SqlCommand(query, connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var accesoPorCargo = new AccesoPorCargo
                        {
                            Cargo = reader.GetString(reader.GetOrdinal("Cargo")),
                            TotalAccesos = reader.GetInt32(reader.GetOrdinal("TotalAccesos"))
                        };
                        accesosPorCargo.Add(accesoPorCargo);
                    }
                }
            }

            return accesosPorCargo;
        }


        public async Task<List<AccesoPorUnidad>> GetAccesosPorUnidadAsync()
        {
            var accesosPorUnidad = new List<AccesoPorUnidad>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
            SELECT 
                U.Unidad_de_Adscripcion AS UnidadDeAdscripcion,
                COUNT(*) AS TotalAccesos
            FROM 
                [dbo].[Accesos] A
            INNER JOIN 
                [dbo].[USUARIO] U ON A.IdUsuario = U.IdUsuario
            GROUP BY 
                U.Unidad_de_Adscripcion
            ORDER BY 
                TotalAccesos DESC";

                var command = new SqlCommand(query, connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var accesoPorUnidad = new AccesoPorUnidad
                        {
                            UnidadDeAdscripcion = reader.GetString(reader.GetOrdinal("UnidadDeAdscripcion")),
                            TotalAccesos = reader.GetInt32(reader.GetOrdinal("TotalAccesos"))
                        };
                        accesosPorUnidad.Add(accesoPorUnidad);
                    }
                }
            }

            return accesosPorUnidad;
        }


        public async Task<List<AccesoPorUsuario>> GetAccesosHoyPorUsuarioAsync()
        {
            var accesosPorUsuarioHoy = new List<AccesoPorUsuario>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
            SELECT 
                U.Nombre AS Usuario,
                COUNT(*) AS TotalAccesos
            FROM 
                [dbo].[Accesos] A
            INNER JOIN 
                [dbo].[USUARIO] U ON A.IdUsuario = U.IdUsuario
            WHERE 
                CAST(A.[FechaHoraLocal] AS DATE) = CAST(GETDATE() AS DATE)
            GROUP BY 
                U.Nombre
            ORDER BY 
                TotalAccesos DESC";

                var command = new SqlCommand(query, connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var accesoPorUsuario = new AccesoPorUsuario
                        {
                            Usuario = reader.GetString(reader.GetOrdinal("Usuario")),
                            TotalAccesos = reader.GetInt32(reader.GetOrdinal("TotalAccesos"))
                        };
                        accesosPorUsuarioHoy.Add(accesoPorUsuario);
                    }
                }
            }

            return accesosPorUsuarioHoy;
        }

        public async Task<List<UsuarioActivo>> ObtenerUsuariosActivosAsync()
        {
            var usuariosActivos = new List<UsuarioActivo>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
            SELECT 
                U.Nombre, 
                U.Cargo, 
                U.[Unidad_de_Adscripcion] as Area, 
                MAX(A.FechaHora) as UltimaActividad,
                (CASE WHEN MAX(A.FechaHora) > DATEADD(MINUTE, -5, GETDATE()) THEN 1 ELSE 0 END) AS EsActivo
            FROM [dbo].[USUARIO] U
            INNER JOIN [dbo].[Accesos] A ON U.IdUsuario = A.IdUsuario
            GROUP BY U.Nombre, U.Cargo, U.Unidad_de_Adscripcion";

                var command = new SqlCommand(query, connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var ultimaActividad = reader.GetDateTime(reader.GetOrdinal("UltimaActividad"));
                        var cdmxZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                        var cdmxTime = TimeZoneInfo.ConvertTimeFromUtc(ultimaActividad.ToUniversalTime(), cdmxZone);

                        var usuarioActivo = new UsuarioActivo
                        {
                            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                            Cargo = reader.GetString(reader.GetOrdinal("Cargo")),
                            Area = reader.GetString(reader.GetOrdinal("Area")),
                            EsActivo = reader.GetInt32(reader.GetOrdinal("EsActivo")) == 1,
                            UltimaActividad = cdmxTime
                        };
                        usuariosActivos.Add(usuarioActivo);
                    }
                }
            }

            return usuariosActivos;
        }

    }


}












