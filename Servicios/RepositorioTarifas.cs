using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using System.Data;

namespace NSIE.Servicios
{
    public interface IRepositorioTarifas
    {
        Task<List<TarifaMediaInflacion>> ObtenerTarifasAsync();
        Task<IEnumerable<TarifaDetalle>> ObtenerTarifasPorMesAnioYDivisionAsync(string mesAnio, string division);
        Task<TarifasTerritorialesResumen> ObtenerResumenTerritorialAsync(
            IReadOnlyCollection<string> divisiones,
            CancellationToken cancellationToken = default);
    }


    public class RepositorioTarifas : IRepositorioTarifas
    {
        private readonly string connectionString;

        public RepositorioTarifas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<TarifaMediaInflacion>> ObtenerTarifasAsync()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT * FROM TarifasMediasInflacion";
                var tarifas = await connection.QueryAsync<TarifaMediaInflacion>(query);
                return tarifas.ToList();
            }
        }

        public async Task<IEnumerable<TarifaDetalle>> ObtenerTarifasPorMesAnioYDivisionAsync(string mesAnio, string division)
        {
            var resultados = new List<TarifaDetalle>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("ObtenerTarifasPorMesAnioYDivision", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@MesAnio", mesAnio);
                    command.Parameters.AddWithValue("@Division", division);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            resultados.Add(new TarifaDetalle
                            {
                                Tarifa = reader["Tarifa"].ToString(),
                                Segmento = reader["Segmento"].ToString(),
                                Unidades = reader["Unidades"].ToString(),
                                Concepto = reader["Concepto"].ToString(),
                                Division = reader["División"].ToString(),
                                Int_Horario = reader["Int_Horario"].ToString(),
                                Valor = reader.IsDBNull(reader.GetOrdinal("Valor")) ? 0 : reader.GetDouble(reader.GetOrdinal("Valor"))
                            });
                        }
                    }
                }
            }

            return resultados;
        }

        public async Task<TarifasTerritorialesResumen> ObtenerResumenTerritorialAsync(
            IReadOnlyCollection<string> divisiones,
            CancellationToken cancellationToken = default)
        {
            var divisionesNormalizadas = divisiones
                .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
                .Select(nombre => nombre.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(17)
                .ToArray();

            if (divisionesNormalizadas.Length == 0)
            {
                return new TarifasTerritorialesResumen();
            }

            const string query = """
                WITH TarifasNormalizadas AS
                (
                    SELECT
                        LTRIM(RTRIM(Division_nombre)) AS Division,
                        LTRIM(RTRIM(Esquema)) AS Esquema,
                        LTRIM(RTRIM(Tarifa)) AS Tarifa,
                        LTRIM(RTRIM(ISNULL(Tarifa_sector, ''))) AS Sector,
                        LTRIM(RTRIM(Concepto)) AS Concepto,
                        CASE
                            WHEN MONTH(Fecha) = 1 AND DAY(Fecha) BETWEEN 1 AND 12
                                THEN DATEFROMPARTS(YEAR(Fecha), DAY(Fecha), 1)
                            ELSE DATEFROMPARTS(YEAR(Fecha), MONTH(Fecha), 1)
                        END AS Periodo,
                        LTRIM(RTRIM(Unidades)) AS Unidades,
                        LTRIM(RTRIM(Segmento)) AS Segmento,
                        LTRIM(RTRIM(Int_Horario)) AS IntervaloHorario,
                        LTRIM(RTRIM(ISNULL(Nivel_Tension, ''))) AS NivelTension,
                        Var_mensual AS VariacionMensual,
                        Dato AS Valor,
                        TM_Nacional_Mensual AS TarifaMediaNacional
                    FROM dbo.vTarifas_Historico
                    WHERE Division_nombre IN @Divisiones
                      AND Concepto = N'Tarifa Media'
                      AND Dato IS NOT NULL
                ),
                UltimoPeriodo AS
                (
                    SELECT Division, MAX(Periodo) AS Periodo
                    FROM TarifasNormalizadas
                    GROUP BY Division
                )
                SELECT
                    t.Division,
                    t.Esquema,
                    t.Tarifa,
                    t.Sector,
                    t.Concepto,
                    t.Periodo,
                    t.Unidades,
                    t.Segmento,
                    t.IntervaloHorario,
                    t.NivelTension,
                    t.VariacionMensual,
                    t.Valor,
                    t.TarifaMediaNacional
                FROM TarifasNormalizadas t
                INNER JOIN UltimoPeriodo u
                    ON u.Division = t.Division
                   AND u.Periodo = t.Periodo
                ORDER BY t.Division, t.Tarifa;
                """;

            await using var connection = new SqlConnection(connectionString);
            var command = new CommandDefinition(
                query,
                new { Divisiones = divisionesNormalizadas },
                commandTimeout: 30,
                cancellationToken: cancellationToken);
            var registros = (await connection.QueryAsync<TarifaTerritorialDetalle>(command)).AsList();

            return new TarifasTerritorialesResumen
            {
                Periodo = registros.Count == 0 ? null : registros.Max(registro => registro.Periodo),
                Divisiones = registros
                    .Select(registro => registro.Division)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(nombre => nombre, StringComparer.Create(new System.Globalization.CultureInfo("es-MX"), true))
                    .ToArray(),
                Registros = registros
            };
        }

    }



}







