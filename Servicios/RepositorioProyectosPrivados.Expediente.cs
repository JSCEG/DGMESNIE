using System.Data;
using Dapper;
using Newtonsoft.Json;
using NSIE.Models.ProyectosPrivados;

namespace NSIE.Servicios;

public partial class RepositorioProyectosPrivados
{
    private const int ExpedienteSchemaVersion = 5;
    public async Task<CarteraConvocatoriaExpediente?> ObtenerExpedienteConvocatoriaAsync(string folio)
    {
        using var db = Connection;
        if (!await db.ExecuteScalarAsync<bool>("SELECT CASE WHEN OBJECT_ID(N'dgmesnie.CarteraConvocatoriaExpediente',N'U') IS NULL THEN 0 ELSE 1 END")) return null;
        var json = await db.QueryFirstOrDefaultAsync<string>(@"
SELECT TOP(1) e.DatosJson FROM dgmesnie.CarteraConvocatoriaExpediente e
JOIN dgmesnie.CarteraConvocatoriaProyecto p ON p.CargaId=e.CargaId AND p.Folio=e.Folio
WHERE p.Folio=@folio AND p.Activo=1 AND p.Fuente=N'Mixtos II' AND p.ConsideracionClave=N'firme'
ORDER BY e.ExpedienteId DESC;", new { folio });
        return json == null ? null : JsonConvert.DeserializeObject<CarteraConvocatoriaExpediente>(json);
    }

    // Último expediente de cada folio activo de Mixtos II (todas las consideraciones) para la ficha de cartera.
    public async Task<List<CarteraConvocatoriaExpediente>> ObtenerExpedientesConvocatoriaAsync()
    {
        using var db = Connection;
        if (!await db.ExecuteScalarAsync<bool>("SELECT CASE WHEN OBJECT_ID(N'dgmesnie.CarteraConvocatoriaExpediente',N'U') IS NULL THEN 0 ELSE 1 END")) return new();
        var rows = await db.QueryAsync<string>(@"
WITH ultimo AS (
    SELECT e.DatosJson, ROW_NUMBER() OVER (PARTITION BY e.Folio ORDER BY e.ExpedienteId DESC) AS rn
    FROM dgmesnie.CarteraConvocatoriaExpediente e
    JOIN dgmesnie.CarteraConvocatoriaProyecto p ON p.CargaId=e.CargaId AND p.Folio=e.Folio
    WHERE p.Activo=1 AND p.Fuente=N'Mixtos II')
SELECT DatosJson FROM ultimo WHERE rn=1;");
        return rows.Select(json => JsonConvert.DeserializeObject<CarteraConvocatoriaExpediente>(json)!).Where(d => d != null).ToList();
    }

    // Fecha prevista de operación comercial de cada folio firme, leída del último expediente
    // (BD_MIXTOS_II) para armar el programa de entradas por gerencia sin ampliar la tabla de cartera.
    public async Task<List<CarteraConvocatoriaOperacionProgramada>> ObtenerProgramaOperacionConvocatoriaAsync()
    {
        using var db = Connection;
        if (!await db.ExecuteScalarAsync<bool>("SELECT CASE WHEN OBJECT_ID(N'dgmesnie.CarteraConvocatoriaExpediente',N'U') IS NULL THEN 0 ELSE 1 END")) return new();
        return (await db.QueryAsync<CarteraConvocatoriaOperacionProgramada>(@"
WITH ultimo AS (
    SELECT e.Folio, e.DatosJson, ROW_NUMBER() OVER (PARTITION BY e.Folio ORDER BY e.ExpedienteId DESC) AS rn
    FROM dgmesnie.CarteraConvocatoriaExpediente e
    JOIN dgmesnie.CarteraConvocatoriaProyecto p ON p.CargaId=e.CargaId AND p.Folio=e.Folio
    WHERE p.Activo=1 AND p.Fuente=N'Mixtos II' AND p.ConsideracionClave=N'firme')
SELECT p.Folio, p.GerenciaControl AS Region, p.CapacidadMw AS Mw, f.[Value] AS OperationText
FROM ultimo u
JOIN dgmesnie.CarteraConvocatoriaProyecto p ON p.Folio=u.Folio AND p.Activo=1 AND p.Fuente=N'Mixtos II'
CROSS APPLY OPENJSON(u.DatosJson,'$.Records') WITH (Sheet nvarchar(100) '$.Sheet', Fields nvarchar(max) '$.Fields' AS JSON) r
CROSS APPLY OPENJSON(r.Fields) WITH (Name nvarchar(400) '$.Name', [Value] nvarchar(200) '$.Value') f
WHERE u.rn=1 AND r.Sheet=N'BD_MIXTOS_II' AND f.Name=N'Fecha Prevista para entrada en operación comercial';")).ToList();
    }

    // Marcas de clúster y excluyentes de CFE: tabla propia por folio (una fila vigente por folio),
    // independiente del corte de la cartera.
    private const string MarcasTable = "dgmesnie.CarteraConvocatoriaMarca";

    private static async Task EnsureMarcasTableAsync(IDbConnection db, IDbTransaction? tx = null)
    {
        await db.ExecuteAsync($@"
IF OBJECT_ID(N'{MarcasTable}', N'U') IS NULL
CREATE TABLE {MarcasTable} (
    Folio nvarchar(60) NOT NULL PRIMARY KEY,
    Cluster nvarchar(120) NULL,
    Excluyente1 nvarchar(120) NULL,
    Excluyente2 nvarchar(120) NULL,
    Considerar bit NULL,
    NombreArchivo nvarchar(260) NOT NULL,
    Sha256 char(64) NOT NULL,
    CargadoUtc datetime2 NOT NULL CONSTRAINT DF_CarteraConvocatoriaMarca_CargadoUtc DEFAULT SYSUTCDATETIME(),
    CargadoPor nvarchar(200) NULL
);", transaction: tx);
    }

    public async Task<int> GuardarMarcasConvocatoriaAsync(CarteraConvocatoriaMarcasDocument document, string usuario)
    {
        ArgumentNullException.ThrowIfNull(document);
        using var db = Connection;
        db.Open();
        using var tx = db.BeginTransaction();
        try
        {
            await EnsureMarcasTableAsync(db, tx);
            var saved = 0;
            foreach (var mark in document.Marks)
            {
                saved += await db.ExecuteAsync($@"
MERGE {MarcasTable} WITH (HOLDLOCK) AS target
USING (SELECT @Folio AS Folio) AS source ON target.Folio = source.Folio
WHEN MATCHED THEN UPDATE SET Cluster=@Cluster, Excluyente1=@Excluyente1, Excluyente2=@Excluyente2, Considerar=@Considerar,
    NombreArchivo=@FileName, Sha256=@Sha256, CargadoUtc=SYSUTCDATETIME(), CargadoPor=@Usuario
WHEN NOT MATCHED THEN INSERT (Folio, Cluster, Excluyente1, Excluyente2, Considerar, NombreArchivo, Sha256, CargadoPor)
    VALUES (@Folio, @Cluster, @Excluyente1, @Excluyente2, @Considerar, @FileName, @Sha256, @Usuario);",
                    new { mark.Folio, mark.Cluster, mark.Excluyente1, mark.Excluyente2, mark.Considerar, mark.FileName, mark.Sha256, Usuario = usuario }, tx);
            }
            tx.Commit();
            return saved;
        }
        catch { tx.Rollback(); throw; }
    }

    public async Task<CarteraConvocatoriaMarca?> ObtenerMarcaConvocatoriaAsync(string folio)
    {
        using var db = Connection;
        if (!await db.ExecuteScalarAsync<bool>($"SELECT CASE WHEN OBJECT_ID(N'{MarcasTable}',N'U') IS NULL THEN 0 ELSE 1 END")) return null;
        return await db.QueryFirstOrDefaultAsync<CarteraConvocatoriaMarca>($@"
SELECT Folio, Cluster, Excluyente1, Excluyente2, Considerar, NombreArchivo AS FileName, Sha256, CargadoUtc AS LoadedUtc, CargadoPor AS LoadedBy
FROM {MarcasTable} WHERE Folio=@folio;", new { folio });
    }

    public async Task<List<CarteraConvocatoriaMarca>> ObtenerMarcasConvocatoriaAsync()
    {
        using var db = Connection;
        if (!await db.ExecuteScalarAsync<bool>($"SELECT CASE WHEN OBJECT_ID(N'{MarcasTable}',N'U') IS NULL THEN 0 ELSE 1 END")) return new();
        return (await db.QueryAsync<CarteraConvocatoriaMarca>($@"
SELECT Folio, Cluster, Excluyente1, Excluyente2, Considerar, NombreArchivo AS FileName, Sha256, CargadoUtc AS LoadedUtc, CargadoPor AS LoadedBy
FROM {MarcasTable};")).ToList();
    }

    // Backfill específico: compara el corte con SQL y sólo inserta expedientes. No usa el upsert de cartera.
    public async Task<int> CompletarExpedientesConvocatoriaAsync(CarteraConvocatoriaImportDocument document, string usuario, bool aplicar = false)
    {
        using var db = Connection;
        db.Open();
        using var tx = db.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            var current = (await db.QueryAsync<CarteraConvocatoriaProyecto>(@"
SELECT Folio,Nombre AS Name,CapacidadMw AS Mw,CostoRedMdp AS NetworkCostMxn,
CostoRedMdd AS NetworkCostUsd,NumeroObras AS WorksCount,CargaId AS ImportId
FROM dgmesnie.CarteraConvocatoriaProyecto
WHERE Activo=1 AND Fuente=N'Mixtos II';", transaction: tx)).ToDictionary(p => p.Folio,StringComparer.OrdinalIgnoreCase);
            if (current.Count != document.Projects.Count || document.Dossiers.Count != current.Count)
                throw new InvalidDataException("El universo del libro difiere de la cartera vigente. Requiere conciliación antes de completar expedientes.");
            var differences = new List<string>();
            bool Same(decimal? a, decimal? b) => a.HasValue == b.HasValue && (!a.HasValue || Math.Abs(a.Value-b!.Value) <= 0.0001m);
            foreach (var p in document.Projects)
            {
                if (!current.TryGetValue(p.Folio,out var saved) || !string.Equals(saved.Name.Trim(),p.Name.Trim(),StringComparison.OrdinalIgnoreCase)
                    || Math.Abs(saved.Mw-p.Mw) > 0.001m || !Same(saved.NetworkCostMxn,p.NetworkCostMxn)
                    || !Same(saved.NetworkCostUsd,p.NetworkCostUsd) || saved.WorksCount != p.WorksCount)
                    differences.Add(p.Folio);
            }
            if (differences.Count > 0)
                throw new InvalidDataException("Datos base diferentes en " + differences.Count + " folios: " + string.Join(", ", differences.Take(10)));
            var imports = current.Values.Select(p => p.ImportId).Distinct().ToArray();
            if (imports.Length != 1 || !imports[0].HasValue) throw new InvalidDataException("La cartera no corresponde a una carga única.");
            var inserted = aplicar ? await GuardarExpedientesAsync(db,tx,document,imports[0]!.Value,usuario) : 0;
            tx.Commit();
            return inserted;
        }
        catch { tx.Rollback(); throw; }
    }

    private static async Task<int> GuardarExpedientesAsync(IDbConnection db, IDbTransaction tx,
        CarteraConvocatoriaImportDocument document, long importId, string usuario)
    {
        int inserted = 0;
        foreach (var dossier in document.Dossiers.Values)
        {
            inserted += await db.ExecuteAsync(@"
INSERT dgmesnie.CarteraConvocatoriaExpediente(CargaId,Folio,FuenteSha256,VersionEsquema,DatosJson,RegistradoPor)
SELECT @importId,@Folio,@Sha256,@schemaVersion,@json,@usuario
WHERE EXISTS(SELECT 1 FROM dgmesnie.CarteraConvocatoriaProyectoVersion WHERE CargaId=@importId AND Folio=@Folio)
AND NOT EXISTS(SELECT 1 FROM dgmesnie.CarteraConvocatoriaExpediente WITH(UPDLOCK,HOLDLOCK)
    WHERE CargaId=@importId AND Folio=@Folio AND FuenteSha256=@Sha256 AND VersionEsquema=@schemaVersion);",
                new { importId,dossier.Folio,dossier.Sha256,schemaVersion=ExpedienteSchemaVersion,json=JsonConvert.SerializeObject(dossier),usuario },tx);
        }
        return inserted;
    }
}
