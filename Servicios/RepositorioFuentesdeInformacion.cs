using Microsoft.Data.SqlClient;
using NSIE.Models;
using Dapper;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Agrega este using

namespace NSIE.Servicios
{
    public interface IRepositorioFuentesdeInformacion
    {
        Task<List<FuenteInformacionModel>> ObtenerFuentesAsync(string filtro = null);
        Task<List<FuenteInformacionModel>> ObtenerFuentesPorEntidadAsync(string entidad);
        Task<List<FuenteTotalModel>> ObtenerTotalesPorFuenteAsync();
        Task<FuenteInformacionModel> ObtenerFuentePorIdAsync(int id);
        Task<int> CrearFuenteAsync(FuenteInformacionModel fuente);
        Task<bool> ActualizarEntidadAsync(string entidadOriginal, string entidadNueva);
        Task<bool> EliminarEntidadAsync(string entidad);
        Task<bool> CrearEntidadAsync(string nombre);
        Task<bool> ActualizarFuenteAsync(FuenteInformacionModel fuente);
        Task<bool> EliminarFuenteAsync(int id);
    }



    public class RepositorioFuentesdeInformacion : IRepositorioFuentesdeInformacion
    {
        private readonly string connectionString;
        private readonly ILogger<RepositorioFuentesdeInformacion> _logger; // Agrega logger

        public RepositorioFuentesdeInformacion(IConfiguration configuration, ILogger<RepositorioFuentesdeInformacion> logger)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<List<FuenteInformacionModel>> ObtenerFuentesAsync(string filtro = null)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                _logger.LogInformation("Conexión abierta para ObtenerFuentesAsync con filtro: {Filtro}", filtro);

                var query = @"SELECT ID, Entidad, Tipo, Rubro, Etiqueta, Dato_Informacion,
                            Desagregacion, Sub_desagregacion, Unidades,
                            Periodicidad_Corte_de_Informacion, Fuente_Link, Comentario
                      FROM dbo.FuentesdeInformacion
                      WHERE (@Filtro IS NULL OR Entidad LIKE '%' + @Filtro + '%' OR Rubro LIKE '%' + @Filtro + '%')";
                var resultado = await connection.QueryAsync<FuenteInformacionModel>(query, new { Filtro = filtro });
                _logger.LogInformation("Consulta ejecutada. Registros obtenidos: {Count}", resultado.Count());
                return resultado.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ObtenerFuentesAsync: {Message}", ex.Message);
                throw;
            }
        }

        // Método específico para obtener fuentes por entidad exacta
        public async Task<List<FuenteInformacionModel>> ObtenerFuentesPorEntidadAsync(string entidad)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                _logger.LogInformation("Conexión abierta para ObtenerFuentesPorEntidadAsync con entidad: {Entidad}", entidad);

                var query = @"SELECT ID, Entidad, Tipo, Rubro, Etiqueta, Dato_Informacion,
                            Desagregacion, Sub_desagregacion, Unidades,
                            Periodicidad_Corte_de_Informacion, Fuente_Link, Comentario
                      FROM dbo.FuentesdeInformacion
                      WHERE Entidad = @Entidad";
                var resultado = await connection.QueryAsync<FuenteInformacionModel>(query, new { Entidad = entidad });
                _logger.LogInformation("Consulta ejecutada para entidad exacta. Registros obtenidos: {Count}", resultado.Count());
                return resultado.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ObtenerFuentesPorEntidadAsync: {Message}", ex.Message);
                throw;
            }
        }

        // Totales por fuente
        public async Task<List<FuenteTotalModel>> ObtenerTotalesPorFuenteAsync()
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"SELECT Entidad, COUNT(*) AS Total FROM dbo.FuentesdeInformacion GROUP BY Entidad";
                var resultado = await connection.QueryAsync<FuenteTotalModel>(query);
                return resultado.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ObtenerTotalesPorFuenteAsync: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<FuenteInformacionModel> ObtenerFuentePorIdAsync(int id)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"SELECT ID, Entidad, Tipo, Rubro, Etiqueta, Dato_Informacion,
                            Desagregacion, Sub_desagregacion, Unidades,
                            Periodicidad_Corte_de_Informacion, Fuente_Link, Comentario
                      FROM dbo.FuentesdeInformacion WHERE ID = @Id";
                var resultado = await connection.QueryFirstOrDefaultAsync<FuenteInformacionModel>(query, new { Id = id });
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ObtenerFuentePorIdAsync: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<int> CrearFuenteAsync(FuenteInformacionModel fuente)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"INSERT INTO dbo.FuentesdeInformacion 
                            (Entidad, Tipo, Rubro, Etiqueta, Dato_Informacion, Desagregacion, 
                             Sub_desagregacion, Unidades, Periodicidad_Corte_de_Informacion, Fuente_Link, Comentario)
                          OUTPUT INSERTED.ID
                          VALUES 
                            (@Entidad, @Tipo, @Rubro, @Etiqueta, @Dato_Informacion, @Desagregacion, 
                             @Sub_desagregacion, @Unidades, @Periodicidad_Corte_de_Informacion, @Fuente_Link, @Comentario);";

                var id = await connection.ExecuteScalarAsync<int>(query, fuente);
                _logger.LogInformation("Fuente creada exitosamente con ID: {Id}", id);
                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CrearFuenteAsync: {Message}", ex.Message);
                throw; // Re-lanzar para que el controlador lo maneje
            }
        }

        public async Task<bool> ActualizarEntidadAsync(string entidadOriginal, string entidadNueva)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"UPDATE dbo.FuentesdeInformacion 
                            SET Entidad = @EntidadNueva 
                            WHERE Entidad = @EntidadOriginal";

                var filasAfectadas = await connection.ExecuteAsync(query, new
                {
                    EntidadOriginal = entidadOriginal,
                    EntidadNueva = entidadNueva
                });

                _logger.LogInformation("Entidad actualizada. Filas afectadas: {FilasAfectadas}", filasAfectadas);
                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ActualizarEntidadAsync: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> EliminarEntidadAsync(string entidad)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"DELETE FROM dbo.FuentesdeInformacion WHERE Entidad = @Entidad";

                var filasAfectadas = await connection.ExecuteAsync(query, new { Entidad = entidad });

                _logger.LogInformation("Entidad eliminada. Filas afectadas: {FilasAfectadas}", filasAfectadas);
                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EliminarEntidadAsync: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> CrearEntidadAsync(string nombre)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Verificar si ya existe una entidad con ese nombre
                var existeQuery = @"SELECT COUNT(*) FROM dbo.FuentesdeInformacion WHERE Entidad = @Nombre";
                var existe = await connection.QuerySingleAsync<int>(existeQuery, new { Nombre = nombre });

                if (existe > 0)
                {
                    _logger.LogWarning("Intento de crear entidad que ya existe: {Nombre}", nombre);
                    return false; // La entidad ya existe
                }

                // Crear una fuente placeholder para la entidad
                var query = @"INSERT INTO dbo.FuentesdeInformacion 
                            (Entidad, Tipo, Rubro, Etiqueta, Dato_Informacion)
                          VALUES 
                            (@Entidad, 'Placeholder', 'General', 'Entidad creada', 'Entidad creada automáticamente')";

                var filasAfectadas = await connection.ExecuteAsync(query, new { Entidad = nombre });

                _logger.LogInformation("Entidad creada exitosamente: {Nombre}", nombre);
                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CrearEntidadAsync: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> ActualizarFuenteAsync(FuenteInformacionModel fuente)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"UPDATE dbo.FuentesdeInformacion 
                            SET Entidad = @Entidad, Tipo = @Tipo, Rubro = @Rubro, Etiqueta = @Etiqueta,
                                Dato_Informacion = @Dato_Informacion, Desagregacion = @Desagregacion,
                                Sub_desagregacion = @Sub_desagregacion, Unidades = @Unidades,
                                Periodicidad_Corte_de_Informacion = @Periodicidad_Corte_de_Informacion,
                                Fuente_Link = @Fuente_Link, Comentario = @Comentario
                            WHERE ID = @Id";

                var parametros = new
                {
                    Id = fuente.ID,
                    Entidad = fuente.Entidad,
                    Tipo = fuente.Tipo,
                    Rubro = fuente.Rubro,
                    Etiqueta = fuente.Etiqueta,
                    Dato_Informacion = fuente.Dato_Informacion,
                    Desagregacion = fuente.Desagregacion,
                    Sub_desagregacion = fuente.Sub_desagregacion,
                    Unidades = fuente.Unidades,
                    Periodicidad_Corte_de_Informacion = fuente.Periodicidad_Corte_de_Informacion,
                    Fuente_Link = fuente.Fuente_Link,
                    Comentario = fuente.Comentario
                };

                var filasAfectadas = await connection.ExecuteAsync(query, parametros);
                _logger.LogInformation("Fuente actualizada. ID: {Id}, Filas afectadas: {FilasAfectadas}", fuente.ID, filasAfectadas);
                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ActualizarFuenteAsync: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> EliminarFuenteAsync(int id)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"DELETE FROM dbo.FuentesdeInformacion WHERE ID = @Id";

                var filasAfectadas = await connection.ExecuteAsync(query, new { Id = id });

                _logger.LogInformation("Fuente eliminada. ID: {Id}, Filas afectadas: {FilasAfectadas}", id, filasAfectadas);
                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EliminarFuenteAsync: {Message}", ex.Message);
                throw;
            }
        }

    }

    public class FuenteTotalModel
    {
        public string Entidad { get; set; }
        public int Total { get; set; }
    }
}

