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
        Task<List<AccesoDetalle>> GetDetallesAccesoPorUsuarioAsync(string nombreUsuario, DateTime fechaInicio, DateTime fechaFin);

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
                    "SELECT * FROM [dbo].[USUARIO] WHERE [Correo] = @Email",
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
                    "SELECT * FROM [dbo].[USUARIO] WHERE [IdUsuario] = @IdUsuario",
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
                    "INSERT INTO [dbo].[Recuperar_contrasena] ([IdUsuario], [Token], [Fecha]) VALUES (@IdUsuario, @Token, @Fecha)",
                    new { IdUsuario = userId, Token = token, Fecha = creationTime }
                );
            }
        }

        public async Task DeletePasswordResetToken(int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(
                    "DELETE FROM [dbo].[Recuperar_contrasena] WHERE [IdUsuario] = @IdUsuario",
                    new { IdUsuario = userId }
                );
            }
        }

        public async Task UpdatePassword(int userId, string newPasswordHash)
        {
            var sql = "UPDATE USUARIO SET Clave = @NewPassword WHERE IdUsuario = @UserId;";
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
                    "SELECT TOP 1 * FROM [dbo].[Recuperar_contrasena] WHERE [Token] = @Token ORDER BY [Fecha] DESC",
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
                    "SELECT COUNT(*) FROM [dbo].[Accesos]"
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
                [dbo].[Accesos] A
            WHERE 
                A.FechaHora BETWEEN @FechaInicio AND @FechaFin
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

        public async Task<List<AccesoDetalle>> GetDetallesAccesoPorUsuarioAsync(string nombreUsuario, DateTime fechaInicio, DateTime fechaFin)
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
                    AND U.Nombre = @NombreUsuario
                ORDER BY 
                    A.[FechaHoraLocal] DESC;";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                command.Parameters.AddWithValue("@FechaFin", fechaFin);
                command.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);

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


        //Componentes
        public int ObtenerTotalVisitas()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                return connection.QuerySingleOrDefault<int>(
                    "SELECT COUNT(*) FROM Accesos");
            }
        }



    }




}

