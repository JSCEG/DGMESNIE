using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models;

namespace NSIE.Servicios
{
    public interface IRepositorioInversionDesarrolloEnergetico
    {
        Task EnsureTableAsync();
        Task<bool> TieneDatosAsync();
        Task<List<PvirseRegistro>> ObtenerAsync(string entidadFederativa = null, string tipoInversion = null);
        Task<List<string>> ObtenerEntidadesAsync();
        Task<List<string>> ObtenerTiposInversionAsync();
        Task<int> ReemplazarDatosAsync(IReadOnlyCollection<PvirseRegistro> registros, string fuenteArchivo);
    }

    public class RepositorioInversionDesarrolloEnergetico : IRepositorioInversionDesarrolloEnergetico
    {
        private const string TableName = "dgmesnie.PvirseCentralesElectricas";
        private readonly string _connectionString;

        public RepositorioInversionDesarrolloEnergetico(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task EnsureTableAsync()
        {
            const string sql = @"
IF SCHEMA_ID('dgmesnie') IS NULL
BEGIN
    EXEC('CREATE SCHEMA dgmesnie');
END;

IF OBJECT_ID('dgmesnie.PvirseCentralesElectricas', 'U') IS NULL
BEGIN
    CREATE TABLE dgmesnie.PvirseCentralesElectricas
    (
        PvirseRegistroId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Status NVARCHAR(200) NULL,
        StatusVf NVARCHAR(200) NULL,
        NombreReal NVARCHAR(500) NOT NULL,
        NoConsiderar NVARCHAR(100) NULL,
        Anio INT NULL,
        AdicionesOSustituciones NVARCHAR(100) NULL,
        ContratoOUnidad NVARCHAR(100) NULL,
        Tipo NVARCHAR(100) NULL,
        TipoVf NVARCHAR(100) NULL,
        Renovable NVARCHAR(100) NULL,
        Mw DECIMAL(18,2) NULL,
        Mes NVARCHAR(50) NULL,
        GerenciaDeControl NVARCHAR(100) NULL,
        RegionDeTransmision NVARCHAR(150) NULL,
        EntidadFederativa NVARCHAR(150) NULL,
        Municipio NVARCHAR(150) NULL,
        Firmes NVARCHAR(20) NULL,
        FuenteArchivo NVARCHAR(260) NULL,
        FechaCarga DATETIME2 NOT NULL CONSTRAINT DF_PvirseCentralesElectricas_FechaCarga DEFAULT SYSUTCDATETIME(),
        Activo BIT NOT NULL CONSTRAINT DF_PvirseCentralesElectricas_Activo DEFAULT 1
    );

    CREATE INDEX IX_PvirseCentralesElectricas_Entidad_Status ON dgmesnie.PvirseCentralesElectricas(EntidadFederativa, StatusVf, Status);
END;";

            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql);
        }

        public async Task<bool> TieneDatosAsync()
        {
            await EnsureTableAsync();
            await using var connection = new SqlConnection(_connectionString);
            var total = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM {TableName} WHERE Activo = 1;");
            return total > 0;
        }

        public async Task<List<PvirseRegistro>> ObtenerAsync(string entidadFederativa = null, string tipoInversion = null)
        {
            await EnsureTableAsync();

            const string sql = @"
SELECT *
FROM dgmesnie.PvirseCentralesElectricas
WHERE Activo = 1
  AND (@EntidadFederativa IS NULL OR EntidadFederativa = @EntidadFederativa)
  AND (@TipoInversion IS NULL OR Status = @TipoInversion)
ORDER BY Anio, EntidadFederativa, NombreReal;";

            await using var connection = new SqlConnection(_connectionString);
            var registros = await connection.QueryAsync<PvirseRegistro>(sql, new
            {
                EntidadFederativa = Normalize(entidadFederativa),
                TipoInversion = Normalize(tipoInversion)
            });

            return registros.ToList();
        }

        public async Task<List<string>> ObtenerEntidadesAsync()
        {
            await EnsureTableAsync();
            await using var connection = new SqlConnection(_connectionString);
            var registros = await connection.QueryAsync<string>($@"
SELECT DISTINCT EntidadFederativa
FROM {TableName}
WHERE Activo = 1 AND NULLIF(LTRIM(RTRIM(EntidadFederativa)), '') IS NOT NULL
ORDER BY EntidadFederativa;");

            return registros.ToList();
        }

        public async Task<List<string>> ObtenerTiposInversionAsync()
        {
            await EnsureTableAsync();
            await using var connection = new SqlConnection(_connectionString);
            var registros = await connection.QueryAsync<string>($@"
SELECT DISTINCT Status
FROM {TableName}
WHERE Activo = 1 AND NULLIF(LTRIM(RTRIM(Status)), '') IS NOT NULL
ORDER BY Status;");

            return registros.ToList();
        }

        public async Task<int> ReemplazarDatosAsync(IReadOnlyCollection<PvirseRegistro> registros, string fuenteArchivo)
        {
            await EnsureTableAsync();
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                await connection.ExecuteAsync($"DELETE FROM {TableName};", transaction: transaction);

                const string insertSql = @"
INSERT INTO dgmesnie.PvirseCentralesElectricas
(
    Status, StatusVf, NombreReal, NoConsiderar, Anio, AdicionesOSustituciones, ContratoOUnidad,
    Tipo, TipoVf, Renovable, Mw, Mes, GerenciaDeControl, RegionDeTransmision, EntidadFederativa,
    Municipio, Firmes, FuenteArchivo, FechaCarga, Activo
)
VALUES
(
    @Status, @StatusVf, @NombreReal, @NoConsiderar, @Anio, @AdicionesOSustituciones, @ContratoOUnidad,
    @Tipo, @TipoVf, @Renovable, @Mw, @Mes, @GerenciaDeControl, @RegionDeTransmision, @EntidadFederativa,
    @Municipio, @Firmes, @FuenteArchivo, SYSUTCDATETIME(), 1
);";

                foreach (var registro in registros)
                {
                    registro.FuenteArchivo = fuenteArchivo;
                    await connection.ExecuteAsync(insertSql, registro, transaction);
                }

                await transaction.CommitAsync();
                return registros.Count;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}