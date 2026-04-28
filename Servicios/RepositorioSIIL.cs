using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Servicios.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSIE.Models;
using System.Data;

namespace NSIE.Servicios
{
    public class RepositorioSIIL : IRepositorioSIIL
    {
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _env;

        //Seguridad
        // private static readonly HashSet<string> TablasPermitidas = new()
        // {
        //     "Registro_Muestras",
        //     "Pronostico_Pozos",
        //     "Entidades_Federativas",
        //     "Municipios"
        // };

        public RepositorioSIIL(IConfiguration configuration, IWebHostEnvironment env)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _env = env;
        }

        public async Task<List<T>> ObtenerTodos<T>(string nombreTabla) where T : class
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = $"SELECT * FROM {nombreTabla}";
                    var result = await connection.QueryAsync<T>(query);
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener todos los registros de {nombreTabla}: {ex.Message}");
                throw;
            }
        }

        public async Task<T> ObtenerPorId<T>(string nombreTabla, int id) where T : class
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = $"SELECT * FROM {nombreTabla} WHERE Id = @Id";
                    var result = await connection.QueryFirstOrDefaultAsync<T>(query, new { Id = id });
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener registro de {nombreTabla} con ID {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<int> Insertar<T>(string nombreTabla, T objeto) where T : class
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var propiedades = typeof(T).GetProperties()
                        .Where(p =>
                            (p.PropertyType.IsValueType || p.PropertyType == typeof(string)) &&
                            !Attribute.IsDefined(p, typeof(Dapper.Contrib.Extensions.WriteAttribute)) &&
                            p.Name != "Id"   // 🔴 ignorar IDENTITY
                        );

                    var columnas = string.Join(", ", propiedades.Select(p => p.Name));
                    var parametros = string.Join(", ", propiedades.Select(p => $"@{p.Name}"));
                    var valores = propiedades.ToDictionary(p => p.Name, p => p.GetValue(objeto));

                    var query = $@"
                        INSERT INTO {nombreTabla} ({columnas})
                        VALUES ({parametros});
                        SELECT CAST(SCOPE_IDENTITY() as int);
                    ";

                    var idGenerado = await connection.ExecuteScalarAsync<int>(query, valores);

                    return idGenerado;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al insertar en {nombreTabla}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> Actualizar<T>(string nombreTabla, T objeto) where T : class
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var propiedades = typeof(T).GetProperties();
                    var idPropiedad = propiedades.FirstOrDefault(p => p.Name.ToLower() == "id");

                    if (idPropiedad == null)
                        throw new ArgumentException("El objeto debe tener una propiedad 'Id'");

                    var actualizaciones = string.Join(", ", propiedades
                        .Where(p => p.Name.ToLower() != "id")
                        .Select(p => $"{p.Name} = @{p.Name}"));

                    var valores = propiedades.ToDictionary(p => p.Name, p => p.GetValue(objeto));

                    var query = $"UPDATE {nombreTabla} SET {actualizaciones} WHERE Id = @Id";
                    var resultado = await connection.ExecuteAsync(query, valores);
                    return resultado > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar en {nombreTabla}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> Eliminar<T>(string nombreTabla, int id) where T : class
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = $"DELETE FROM {nombreTabla} WHERE Id = @Id";
                    var resultado = await connection.ExecuteAsync(query, new { Id = id });
                    return resultado > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar de {nombreTabla}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<T>> ObtenerPorFiltro<T>(string nombreTabla, string condicion, Dictionary<string, object> parametros) where T : class
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = $"SELECT * FROM {nombreTabla}";
                    if (!string.IsNullOrEmpty(condicion))
                    {
                        query += $" WHERE {condicion}";
                    }

                    var dinamicParams = new DynamicParameters();
                    foreach (var param in parametros)
                    {
                        dinamicParams.Add($"@{param.Key}", param.Value);
                    }

                    var result = await connection.QueryAsync<T>(query, dinamicParams);
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener registros filtrados de {nombreTabla}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ExisteBarreno(string barrenoId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"SELECT COUNT(1)
                            FROM Barrenaciones
                            WHERE BarrenoID = @BarrenoID";

                var existe = await connection.ExecuteScalarAsync<int>(
                    query,
                    new { BarrenoID = barrenoId }
                );

                return existe > 0;
            }
        }

        public async Task<bool> BuscarBarreno()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = @"
                        SELECT CASE 
                            WHEN EXISTS (SELECT 1 FROM Barrenaciones)
                            THEN CAST(1 AS BIT)
                            ELSE CAST(0 AS BIT)
                        END
                    ";

                    var existe = await connection.ExecuteScalarAsync<bool>(query);

                    return existe;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verificando existencia de barrenos: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<string>> ObtenerBarrenos()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = @"SELECT BarrenoID FROM Barrenaciones ORDER BY BarrenoID";

                    var barrenos = await connection.QueryAsync<string>(query);

                    return barrenos;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo barrenos: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Genera el identificador único para Registro_Muestras según la fórmula:
        /// [CÓDIGO_FUENTE] - [CÓDIGO_ORIGEN] - [TIMESTAMP]
        /// 
        /// REGLAS DE NEGOCIO:
        /// 1. Fuente "Arcilla" → ARC
        /// 2. Fuente "Salmuera" → SLM
        /// 3. Si Fuente = Salmuera → Origen siempre = SLM (regla especial)
        /// 4. Si Fuente = Arcilla:
        ///    - Origen "Barrenación" → BRR
        ///    - Origen "Prospectiva" → PRS
        /// 5. Formato fecha: yyyy/MM/dd HH:mm (24 horas)
        /// 
        /// EJEMPLOS REALES:
        /// - GenerarIdMuestra("Salmuera", "", DateTime.Now) → "SLM-SLM-2026/01/23 06:21"
        /// - GenerarIdMuestra("Arcilla", "Prospectiva", DateTime.Now) → "ARC-PRS-2026/01/23 06:38"
        /// - GenerarIdMuestra("Arcilla", "Barrenación", DateTime.Now) → "ARC-BRR-2026/01/23 06:39"
        /// 
        /// NOTA CRÍTICA: Este ID es la PK de Registro_Muestras y FK en:
        /// - Calculo_Litio_Probable
        /// - Resultados_Analiticos
        /// </summary>
        /// <param name="fuente">"Arcilla" o "Salmuera" (case-insensitive)</param>
        /// <param name="origen">"Prospectiva" o "Barrenación" (ignorado si fuente=Salmuera)</param>
        /// <param name="fecha">Fecha y hora del registro</param>
        /// <returns>ID único con formato: XXX-XXX-YYYY/MM/DD HH:mm</returns>
        /// <exception cref="ArgumentException">Si fuente no es válida</exception>
        public string GenerarIdMuestra(string fuente, string origen, DateTime fecha)
        {
            // 1. Validar entrada de fuente
            if (string.IsNullOrWhiteSpace(fuente))
                throw new ArgumentException("La fuente no puede estar vacía", nameof(fuente));

            // 2. Normalizar y determinar código de fuente (TIPO DE RECURSO)
            string tipo = fuente.ToUpper().Contains("ARCILLA") ? "ARC" :
                         fuente.ToUpper().Contains("SALMUERA") ? "SLM" :
                         throw new ArgumentException($"Fuente inválida: '{fuente}'. Debe ser 'Arcilla' o 'Salmuera'", nameof(fuente));

            // 3. Determinar código de origen (ORIGEN DE LA MUESTRA)
            string codigoOrigen;

            if (tipo == "SLM")
            {
                // Regla especial: Si es Salmuera, el origen siempre es SLM
                // (Basado en registro 'Prueba 1' de Fuente 12)
                codigoOrigen = "SLM";
            }
            else // tipo == "ARC"
            {
                // Validar que se proporcionó origen para Arcilla
                if (string.IsNullOrWhiteSpace(origen))
                    throw new ArgumentException("El origen es requerido para fuente Arcilla", nameof(origen));

                // Mapeo basado en Fuente 12
                if (origen.ToUpper().Contains("BARRENACIÓN") || origen.ToUpper().Contains("BARRENACION") || origen.ToUpper().Contains("TESTIGO"))
                {
                    codigoOrigen = "BRR";
                }
                else if (origen.ToUpper().Contains("PROSPECTIVA") || origen.ToUpper().Contains("SUPERFICIE"))
                {
                    codigoOrigen = "PRS";
                }
                else
                {
                    throw new ArgumentException($"Origen inválido para Arcilla: '{origen}'. Debe ser 'Prospectiva' o 'Barrenación'", nameof(origen));
                }
            }

            // 4. Concatenar con formato de fecha estricto (Fuente 11)
            // Formato: yyyy/MM/dd HH:mm (24 horas)
            // Nota: SQL Server soporta este string como PK (VARCHAR 50)
            return $"{tipo}-{codigoOrigen}-{fecha:yyyy/MM/dd HH:mm}";
        }

        // Implementación de métodos adicionales para usuarios y roles
        public async Task<UserViewModel> ObtenerUsuarioPorCorreo(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<UserViewModel>(
                "SELECT * FROM USUARIO Where Correo=@email",
                new { email }
            );
        }

        public async Task<UserViewModel> ObtenerRolPorUsuarioId(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
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

        // Implementación de método para obtener municipios por estado
        public async Task<IEnumerable<Estado>> ObtenerTodos()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Estado>("SELECT EF_ID, EF_Nombre FROM Entidades_Federativas");
        }
        public async Task<IEnumerable<Municipio>> ObtenerMunicipiosPorEstado(int estadoId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Municipio>(
                "SELECT MunicipioID, Municipio_Nombre FROM Municipios WHERE EF_ID = @estadoId",
                new { estadoId }
            );
        }
    }
}
