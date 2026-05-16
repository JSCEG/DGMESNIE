using Microsoft.Data.SqlClient;
using NSIE.Models;
using Dapper;
using System.Data;
using NuGet.Protocol.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace NSIE.Servicios
{
    public interface IRepositorioAcceso
    {

        //Monitore y Uso de 
        Task<List<AccesoDetalle>> GetDetallesAccesoAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<List<AccesoDetalle>> GetDetallesAccesoPorUsuarioAsync(int userId, string? correoUsuario, DateTime fechaInicio, DateTime fechaFin);

        //Cuenta los Accesos ala plataforma
        Task<int> GetTotalAccessCountAsync();
        Task<List<TipoAccesoTotal>> GetTotalAccessCountByTypeAsync(DateTime fechaInicio, DateTime fechaFin);

        //Obtiene los usuarios de la plataforma
        Task<Usuario> GetUserByEmail(string email);

        Task<Usuario> GetUserById(int userId);


        int ObtenerTotalVisitas();

        //Realiza el registro del token a mi usuario para el cambio de contraseña
        Task SavePasswordResetToken(int userId, string token, DateTime creationTime);
        Task DeletePasswordResetToken(int userId);

        Task UpdatePassword(int userId, string newPassword);

        //Obtiene el token para cambio de contraseña
        Task<TokenResetPassword> GetUserByPasswordResetToken(string token);
    }



    public class RepositorioAcceso : IRepositorioAcceso
    {


        private readonly string connectionString;


        public RepositorioAcceso(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");

        }

        public async Task<Usuario> GetUserByEmail(string email)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                return await connection.QuerySingleOrDefaultAsync<Usuario>(
                    @"SELECT 
                            [IdUsuario],
                            [Correo],
                            [ClaveHash] AS [Clave],
                            [Nombre]
                      FROM [dgmesnie].[Usuario]
                      WHERE [Correo] = @Email AND [Vigente] = 1",
                    new { Email = email }
                );
            }
        }

        public async Task<Usuario> GetUserById(int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                return await connection.QuerySingleOrDefaultAsync<Usuario>(
                    @"SELECT 
                            [IdUsuario],
                            [Correo],
                            [ClaveHash] AS [Clave],
                            [Nombre]
                      FROM [dgmesnie].[Usuario]
                      WHERE [IdUsuario] = @IdUsuario AND [Vigente] = 1",
                    new { IdUsuario = userId }
                );
            }
        }



        public async Task SavePasswordResetToken(int userId, string token, DateTime creationTime)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(
                    @"INSERT INTO [dgmesnie].[RecuperacionContrasena]
                      ([IdUsuario], [Token], [FechaCreacion], [FechaExpiracion], [Usado])
                      VALUES (@IdUsuario, @Token, @FechaCreacion, @FechaExpiracion, 0)",
                    new
                    {
                        IdUsuario = userId,
                        Token = token,
                        FechaCreacion = creationTime,
                        FechaExpiracion = creationTime.AddMinutes(30)
                    }
                );
            }
        }

        public async Task DeletePasswordResetToken(int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(
                    "DELETE FROM [dgmesnie].[RecuperacionContrasena] WHERE [IdUsuario] = @IdUsuario",
                    new { IdUsuario = userId }
                );
            }
        }

        public async Task UpdatePassword(int userId, string newPasswordHash)
        {
            var sql = "UPDATE [dgmesnie].[Usuario] SET [ClaveHash] = @NewPassword, [FechaActualizacion] = SYSUTCDATETIME() WHERE [IdUsuario] = @UserId;";
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(sql, new { NewPassword = newPasswordHash, UserId = userId });
            }
        }

        public async Task<TokenResetPassword> GetUserByPasswordResetToken(string token)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                return await connection.QuerySingleOrDefaultAsync<TokenResetPassword>(
                    @"SELECT TOP 1 
                            [IdUsuario],
                            [Token],
                            [FechaCreacion] AS [Fecha]
                      FROM [dgmesnie].[RecuperacionContrasena]
                      WHERE [Token] = @Token AND [Usado] = 0
                      ORDER BY [FechaCreacion] DESC",
                    new { Token = token }
                );
            }
        }

        // public async Task GetUserByPasswordResetToken(string token)
        // {
        //     using (SqlConnection connection = new SqlConnection(connectionString))
        //     {
        //         await connection.OpenAsync();
        //         return await connection.QuerySingleOrDefaultAsync(
        //             "SELECT TOP 1 * FROM [dbo].[Recuperar_contrasena] WHERE [Token] = @Token ORDER BY [Fecha] DESC",
        //             new { Token = token }
        //         );
        //     }
        // }



        //Cuenta los Accesos ala plataforma
        public async Task<int> GetTotalAccessCountAsync()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                return await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM [dgmesnie].[Acceso]"
                );
            }
        }

        public async Task<List<TipoAccesoTotal>> GetTotalAccessCountByTypeAsync(DateTime fechaInicio, DateTime fechaFin)
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
                [dgmesnie].[Acceso] A
            WHERE 
                A.FechaAcceso BETWEEN @FechaInicio AND @FechaFin
            GROUP BY 
                A.TipoAcceso;";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                command.Parameters.AddWithValue("@FechaFin", fechaFin);

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


        public async Task<List<AccesoDetalle>> GetDetallesAccesoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var detallesAcceso = new List<AccesoDetalle>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
            SELECT 
                A.IdAcceso AS AccesoId,
                U.Nombre,
                A.TipoAcceso,
                A.IP,
                A.[FechaAcceso] AS [FechaHoraLocal],
                U.UnidadAdscripcion AS Unidad_de_Adscripcion,
                U.Cargo
            FROM 
                [dgmesnie].[Acceso] A
            INNER JOIN 
                [dgmesnie].[Usuario] U ON A.IdUsuario = U.IdUsuario
            WHERE 
                A.[FechaAcceso] BETWEEN @FechaInicio AND @FechaFin
            ORDER BY 
                A.[FechaAcceso] DESC;";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                command.Parameters.AddWithValue("@FechaFin", fechaFin);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var detalle = new AccesoDetalle
                        {
                            AccesoId = reader.GetInt64(reader.GetOrdinal("AccesoId")),
                            Nombre = reader.IsDBNull(reader.GetOrdinal("Nombre")) ? string.Empty : reader.GetString(reader.GetOrdinal("Nombre")),
                            TipoAcceso = reader.IsDBNull(reader.GetOrdinal("TipoAcceso")) ? string.Empty : reader.GetString(reader.GetOrdinal("TipoAcceso")),
                            IP = reader.IsDBNull(reader.GetOrdinal("IP")) ? string.Empty : reader.GetString(reader.GetOrdinal("IP")),
                            FechaHoraLocal = reader.GetDateTime(reader.GetOrdinal("FechaHoraLocal")),
                            UnidadDeAdscripcion = reader.IsDBNull(reader.GetOrdinal("Unidad_de_Adscripcion")) ? string.Empty : reader.GetString(reader.GetOrdinal("Unidad_de_Adscripcion")),
                            Cargo = reader.IsDBNull(reader.GetOrdinal("Cargo")) ? string.Empty : reader.GetString(reader.GetOrdinal("Cargo"))
                        };
                        detallesAcceso.Add(detalle);
                    }
                }
            }

            return detallesAcceso;
        }

        public async Task<List<AccesoDetalle>> GetDetallesAccesoPorUsuarioAsync(int userId, string? correoUsuario, DateTime fechaInicio, DateTime fechaFin)
        {
            var detallesAcceso = new List<AccesoDetalle>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                SELECT 
                    A.IdAcceso AS AccesoId,
                    COALESCE(UPorId.Nombre, UPorCorreo.Nombre, '') AS Nombre,
                    A.TipoAcceso,
                    A.IP,
                    A.[FechaAcceso] AS [FechaHoraLocal],
                    COALESCE(UPorId.UnidadAdscripcion, UPorCorreo.UnidadAdscripcion, '') AS Unidad_de_Adscripcion,
                    COALESCE(UPorId.Cargo, UPorCorreo.Cargo, '') AS Cargo
                FROM 
                    [dgmesnie].[Acceso] A
                LEFT JOIN 
                    [dgmesnie].[Usuario] UPorId ON A.IdUsuario = UPorId.IdUsuario
                LEFT JOIN 
                    [dgmesnie].[Usuario] UPorCorreo ON A.IdUsuario IS NULL AND UPorCorreo.Correo = A.Correo
                WHERE 
                    A.[FechaAcceso] BETWEEN @FechaInicio AND @FechaFin
                    AND (
                        A.IdUsuario = @UserId
                        OR (A.IdUsuario IS NULL AND A.Correo = @CorreoUsuario)
                    )
                ORDER BY 
                    A.[FechaAcceso] DESC;";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                command.Parameters.AddWithValue("@FechaFin", fechaFin);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CorreoUsuario", (object?)correoUsuario ?? DBNull.Value);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var detalle = new AccesoDetalle
                        {
                            AccesoId = reader.GetInt64(reader.GetOrdinal("AccesoId")),
                            Nombre = reader.IsDBNull(reader.GetOrdinal("Nombre")) ? string.Empty : reader.GetString(reader.GetOrdinal("Nombre")),
                            TipoAcceso = reader.IsDBNull(reader.GetOrdinal("TipoAcceso")) ? string.Empty : reader.GetString(reader.GetOrdinal("TipoAcceso")),
                            IP = reader.IsDBNull(reader.GetOrdinal("IP")) ? string.Empty : reader.GetString(reader.GetOrdinal("IP")),
                            FechaHoraLocal = reader.GetDateTime(reader.GetOrdinal("FechaHoraLocal")),
                            UnidadDeAdscripcion = reader.IsDBNull(reader.GetOrdinal("Unidad_de_Adscripcion")) ? string.Empty : reader.GetString(reader.GetOrdinal("Unidad_de_Adscripcion")),
                            Cargo = reader.IsDBNull(reader.GetOrdinal("Cargo")) ? string.Empty : reader.GetString(reader.GetOrdinal("Cargo"))
                        };
                        detallesAcceso.Add(detalle);
                    }
                }
            }

            return detallesAcceso;
        }


        //Componentes
        public int ObtenerTotalVisitas()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                return connection.QuerySingleOrDefault<int>(
                    "SELECT COUNT(*) FROM [dgmesnie].[Acceso]");
            }
        }



    }




}

