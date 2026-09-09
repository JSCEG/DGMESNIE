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

    // Selección del área (libro "Actualización de 246"): historial por carga (fecha de corte, archivo, hash) y filas por
    // folio de cada carga. Los reportes usan siempre la última carga; las anteriores quedan para trazabilidad. Al guardar,
    // la decisión se aplica a la cartera activa: Considerar → firme; Descarte → desechado; el resto conserva su situación
    // (los duplicados retirados siguen retirados; lo que era firme y ya no se considera pasa a seguimiento).
    private const string SeleccionTable = "dgmesnie.CarteraConvocatoriaSeleccion";
    private const string SeleccionCargaTable = "dgmesnie.CarteraConvocatoriaSeleccionCarga";
    private const string SeleccionColumns = "CargaId, Folio, Considerar, Descarte, Motivo, CoincideReferencia, Preseleccionados, Factibles, ApoyaSen, ObrasOnerosas, ExcluyenteConOtros, ProyectosQueExcluye, PreferenteEntreExcluyentes, Preferente, CenaceEstudios, ProyectosSustitutos, MixtosI, Sistema, NombreArchivo AS FileName, Sha256, CargadoUtc AS LoadedUtc, CargadoPor AS LoadedBy";
    private const string SeleccionCargaColumns = "CargaId, NombreArchivo AS FileName, Sha256, FechaCorte, Filas, Considerar, Descarte, Preferentes, CenaceEstudios, CargadoUtc AS LoadedUtc, CargadoPor AS LoadedBy";

    private static async Task EnsureSeleccionTablesAsync(IDbConnection db, IDbTransaction? tx = null)
    {
        await db.ExecuteAsync($@"
IF OBJECT_ID(N'{SeleccionCargaTable}', N'U') IS NULL
CREATE TABLE {SeleccionCargaTable} (
    CargaId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    NombreArchivo nvarchar(260) NOT NULL,
    Sha256 char(64) NOT NULL,
    FechaCorte date NOT NULL,
    Filas int NOT NULL,
    Considerar int NOT NULL,
    Descarte int NOT NULL,
    Preferentes int NOT NULL,
    CenaceEstudios int NOT NULL,
    CargadoUtc datetime2 NOT NULL CONSTRAINT DF_CarteraConvocatoriaSeleccionCarga_CargadoUtc DEFAULT SYSUTCDATETIME(),
    CargadoPor nvarchar(200) NULL
);
-- Versión previa sin historial (una fila por folio): se reemplaza por el esquema con CargaId.
IF OBJECT_ID(N'{SeleccionTable}', N'U') IS NOT NULL AND COL_LENGTH(N'{SeleccionTable}', N'CargaId') IS NULL
    DROP TABLE {SeleccionTable};
IF OBJECT_ID(N'{SeleccionTable}', N'U') IS NULL
CREATE TABLE {SeleccionTable} (
    CargaId int NOT NULL,
    Folio nvarchar(60) NOT NULL,
    Considerar bit NOT NULL,
    Descarte bit NOT NULL,
    Motivo nvarchar(400) NULL,
    CoincideReferencia nvarchar(60) NULL,
    Preseleccionados nvarchar(200) NULL,
    Factibles nvarchar(200) NULL,
    ApoyaSen nvarchar(120) NULL,
    ObrasOnerosas nvarchar(120) NULL,
    ExcluyenteConOtros nvarchar(400) NULL,
    ProyectosQueExcluye nvarchar(max) NULL,
    PreferenteEntreExcluyentes nvarchar(120) NULL,
    Preferente bit NOT NULL,
    CenaceEstudios bit NOT NULL,
    ProyectosSustitutos nvarchar(max) NULL,
    MixtosI bit NOT NULL,
    Sistema nvarchar(60) NULL,
    NombreArchivo nvarchar(260) NOT NULL,
    Sha256 char(64) NOT NULL,
    CargadoUtc datetime2 NOT NULL CONSTRAINT DF_CarteraConvocatoriaSeleccion_CargadoUtc DEFAULT SYSUTCDATETIME(),
    CargadoPor nvarchar(200) NULL,
    CONSTRAINT PK_CarteraConvocatoriaSeleccion PRIMARY KEY (CargaId, Folio),
    CONSTRAINT FK_CarteraConvocatoriaSeleccion_Carga FOREIGN KEY (CargaId) REFERENCES {SeleccionCargaTable}(CargaId)
);", transaction: tx);
    }

    public async Task<(int Guardados, int Firmes, int Descartados, int Revision)> GuardarSeleccionConvocatoriaAsync(CarteraConvocatoriaSeleccionDocument document, string usuario)
    {
        ArgumentNullException.ThrowIfNull(document);
        using var db = Connection;
        db.Open();
        using var tx = db.BeginTransaction();
        try
        {
            await EnsureSeleccionTablesAsync(db, tx);
            // Mismo archivo ya cargado (mismo hash y misma fecha de corte): no se duplica la versión.
            var existente = await db.QueryFirstOrDefaultAsync<int?>($"SELECT TOP 1 CargaId FROM {SeleccionCargaTable} WHERE Sha256=@Sha256 AND FechaCorte=@FechaCorte ORDER BY CargaId DESC;",
                new { document.Sha256, FechaCorte = document.FechaCorte.Date }, tx);
            int cargaId;
            var saved = 0;
            if (existente.HasValue)
            {
                cargaId = existente.Value;
            }
            else
            {
                cargaId = await db.ExecuteScalarAsync<int>($@"
INSERT {SeleccionCargaTable} (NombreArchivo, Sha256, FechaCorte, Filas, Considerar, Descarte, Preferentes, CenaceEstudios, CargadoPor)
OUTPUT INSERTED.CargaId
VALUES (@FileName, @Sha256, @FechaCorte, @Filas, @Considerar, @Descarte, @Preferentes, @CenaceEstudios, @Usuario);",
                    new { document.FileName, document.Sha256, FechaCorte = document.FechaCorte.Date, Filas = document.Rows.Count,
                          Considerar = document.Rows.Count(r => r.Considerar), Descarte = document.Rows.Count(r => r.Descarte),
                          Preferentes = document.Rows.Count(r => r.Preferente), CenaceEstudios = document.Rows.Count(r => r.CenaceEstudios), Usuario = usuario }, tx);
                foreach (var r in document.Rows)
                {
                    saved += await db.ExecuteAsync($@"
INSERT {SeleccionTable} (CargaId, Folio, Considerar, Descarte, Motivo, CoincideReferencia, Preseleccionados, Factibles, ApoyaSen, ObrasOnerosas, ExcluyenteConOtros,
    ProyectosQueExcluye, PreferenteEntreExcluyentes, Preferente, CenaceEstudios, ProyectosSustitutos, MixtosI, Sistema, NombreArchivo, Sha256, CargadoPor)
VALUES (@CargaId, @Folio, @Considerar, @Descarte, @Motivo, @CoincideReferencia, @Preseleccionados, @Factibles, @ApoyaSen, @ObrasOnerosas, @ExcluyenteConOtros,
    @ProyectosQueExcluye, @PreferenteEntreExcluyentes, @Preferente, @CenaceEstudios, @ProyectosSustitutos, @MixtosI, @Sistema, @FileName, @Sha256, @Usuario);",
                        new { CargaId = cargaId, r.Folio, r.Considerar, r.Descarte, r.Motivo, r.CoincideReferencia, r.Preseleccionados, r.Factibles, r.ApoyaSen, r.ObrasOnerosas, r.ExcluyenteConOtros,
                              r.ProyectosQueExcluye, r.PreferenteEntreExcluyentes, r.Preferente, r.CenaceEstudios, r.ProyectosSustitutos, r.MixtosI, r.Sistema, r.FileName, r.Sha256, Usuario = usuario }, tx);
                }
            }

            // Aplica la decisión de esta carga a la cartera vigente (sólo Mixtos II activo).
            await db.ExecuteAsync($@"
UPDATE p SET
    ConsideracionClave = CASE WHEN s.Considerar=1 THEN N'firme' WHEN s.Descarte=1 THEN N'no-va' WHEN p.ConsideracionClave=N'firme' THEN N'revision' ELSE p.ConsideracionClave END,
    EstatusUniverso    = CASE WHEN s.Considerar=1 THEN N'CONSIDERADO' WHEN s.Descarte=1 THEN N'DESECHADO' WHEN p.ConsideracionClave=N'firme' THEN N'NO CONSIDERADO' ELSE p.EstatusUniverso END,
    EstadoSeguimiento  = CASE WHEN s.Considerar=1 THEN N'continua' WHEN s.Descarte=1 THEN N'no-continua' WHEN p.ConsideracionClave=N'firme' THEN N'revision' ELSE p.EstadoSeguimiento END
FROM dgmesnie.CarteraConvocatoriaProyecto p
JOIN {SeleccionTable} s ON s.Folio = p.Folio AND s.CargaId = @CargaId
WHERE p.Activo = 1 AND p.Fuente = N'Mixtos II';", new { CargaId = cargaId }, tx);

            var counts = await db.QuerySingleAsync<(int Firmes, int Descartados, int Revision)>($@"
SELECT SUM(CASE WHEN ConsideracionClave=N'firme' THEN 1 ELSE 0 END) AS Firmes,
       SUM(CASE WHEN ConsideracionClave=N'no-va' THEN 1 ELSE 0 END) AS Descartados,
       SUM(CASE WHEN ConsideracionClave=N'revision' THEN 1 ELSE 0 END) AS Revision
FROM dgmesnie.CarteraConvocatoriaProyecto WHERE Activo = 1 AND Fuente = N'Mixtos II';", transaction: tx);
            tx.Commit();
            return (saved, counts.Firmes, counts.Descartados, counts.Revision);
        }
        catch { tx.Rollback(); throw; }
    }

    private async Task<int?> UltimaSeleccionCargaAsync(IDbConnection db)
    {
        if (!await db.ExecuteScalarAsync<bool>($"SELECT CASE WHEN OBJECT_ID(N'{SeleccionCargaTable}',N'U') IS NULL OR COL_LENGTH(N'{SeleccionTable}', N'CargaId') IS NULL THEN 0 ELSE 1 END")) return null;
        return await db.QueryFirstOrDefaultAsync<int?>($"SELECT TOP 1 CargaId FROM {SeleccionCargaTable} ORDER BY FechaCorte DESC, CargaId DESC;");
    }

    public async Task<CarteraConvocatoriaSeleccion?> ObtenerSeleccionConvocatoriaAsync(string folio)
    {
        using var db = Connection;
        var cargaId = await UltimaSeleccionCargaAsync(db);
        if (!cargaId.HasValue) return null;
        return await db.QueryFirstOrDefaultAsync<CarteraConvocatoriaSeleccion>($"SELECT {SeleccionColumns} FROM {SeleccionTable} WHERE CargaId=@cargaId AND Folio=@folio;", new { cargaId, folio });
    }

    public async Task<List<CarteraConvocatoriaSeleccion>> ObtenerSeleccionesConvocatoriaAsync()
    {
        using var db = Connection;
        var cargaId = await UltimaSeleccionCargaAsync(db);
        if (!cargaId.HasValue) return new();
        return (await db.QueryAsync<CarteraConvocatoriaSeleccion>($"SELECT {SeleccionColumns} FROM {SeleccionTable} WHERE CargaId=@cargaId;", new { cargaId })).ToList();
    }

    // Historial de cargas del libro de selección (la más reciente primero).
    public async Task<List<CarteraConvocatoriaSeleccionCarga>> ObtenerSeleccionCargasConvocatoriaAsync()
    {
        using var db = Connection;
        if (!await db.ExecuteScalarAsync<bool>($"SELECT CASE WHEN OBJECT_ID(N'{SeleccionCargaTable}',N'U') IS NULL THEN 0 ELSE 1 END")) return new();
        return (await db.QueryAsync<CarteraConvocatoriaSeleccionCarga>($"SELECT {SeleccionCargaColumns} FROM {SeleccionCargaTable} ORDER BY FechaCorte DESC, CargaId DESC;")).ToList();
    }

    // Calculadoras financieras consolidadas: historial por carga y una fila por folio con los supuestos del
    // modelo. Los reportes leen la carga más reciente; las anteriores quedan para trazabilidad.
    private const string CalculadoraTable = "dgmesnie.CarteraConvocatoriaCalculadora";
    private const string CalculadoraCargaTable = "dgmesnie.CarteraConvocatoriaCalculadoraCarga";
    private const string CalculadoraColumns = "CargaId, Folio, Proyecto, Tecnologia, Inversionista, MwAc, MwDc, SaeMw, SaeMwh, SaeHoras, Cod, CapexTotal, CapexCentral, CapexBaterias, CapexInterconexion, DevEx, RetornoProyecto, RetornoPrivado, RetornoObjetivo, RetornoInterconexion, ParticipacionPrivada, ContribucionCfe, PrecioEnergia, PlazoPpa, PlazoReversion, Apalancamiento, PlazoDeuda, TirAntesIsr, TirDespuesIsr, MoicProyecto, MoicPrivado, EbitdaAcumulado, IngresosAcumulados, UtilidadAcumulada, GeneracionAcumulada, OpexAnio1, Observaciones, NombreArchivo AS FileName, Sha256, CargadoUtc AS LoadedUtc, CargadoPor AS LoadedBy";

    private static async Task EnsureCalculadoraTablesAsync(IDbConnection db, IDbTransaction? tx = null)
    {
        await db.ExecuteAsync($@"
IF OBJECT_ID(N'{CalculadoraCargaTable}', N'U') IS NULL
CREATE TABLE {CalculadoraCargaTable} (
    CargaId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    NombreArchivo nvarchar(260) NOT NULL,
    Sha256 char(64) NOT NULL,
    FechaCorte date NOT NULL,
    Filas int NOT NULL,
    CargadoUtc datetime2 NOT NULL CONSTRAINT DF_CarteraConvocatoriaCalculadoraCarga_CargadoUtc DEFAULT SYSUTCDATETIME(),
    CargadoPor nvarchar(200) NULL
);
IF OBJECT_ID(N'{CalculadoraTable}', N'U') IS NULL
CREATE TABLE {CalculadoraTable} (
    CargaId int NOT NULL,
    Folio nvarchar(60) NOT NULL,
    Proyecto nvarchar(260) NULL,
    Tecnologia nvarchar(120) NULL,
    Inversionista nvarchar(260) NULL,
    MwAc decimal(18,4) NULL, MwDc decimal(18,4) NULL, SaeMw decimal(18,4) NULL, SaeMwh decimal(18,4) NULL, SaeHoras decimal(18,4) NULL,
    Cod date NULL,
    CapexTotal decimal(24,4) NULL, CapexCentral decimal(24,4) NULL, CapexBaterias decimal(24,4) NULL, CapexInterconexion decimal(24,4) NULL, DevEx decimal(24,4) NULL,
    RetornoProyecto decimal(18,8) NULL, RetornoPrivado decimal(18,8) NULL, RetornoObjetivo decimal(18,8) NULL, RetornoInterconexion decimal(18,8) NULL,
    ParticipacionPrivada decimal(18,8) NULL, ContribucionCfe decimal(18,8) NULL,
    PrecioEnergia decimal(18,4) NULL, PlazoPpa decimal(18,4) NULL, PlazoReversion decimal(18,4) NULL,
    Apalancamiento decimal(18,8) NULL, PlazoDeuda decimal(18,4) NULL,
    TirAntesIsr decimal(18,8) NULL, TirDespuesIsr decimal(18,8) NULL, MoicProyecto decimal(18,8) NULL, MoicPrivado decimal(18,8) NULL,
    EbitdaAcumulado decimal(24,4) NULL, IngresosAcumulados decimal(24,4) NULL, UtilidadAcumulada decimal(24,4) NULL, GeneracionAcumulada decimal(24,4) NULL,
    OpexAnio1 decimal(24,4) NULL,
    Observaciones nvarchar(max) NULL,
    DatosJson nvarchar(max) NULL,
    NombreArchivo nvarchar(260) NOT NULL,
    Sha256 char(64) NOT NULL,
    CargadoUtc datetime2 NOT NULL CONSTRAINT DF_CarteraConvocatoriaCalculadora_CargadoUtc DEFAULT SYSUTCDATETIME(),
    CargadoPor nvarchar(200) NULL,
    CONSTRAINT PK_CarteraConvocatoriaCalculadora PRIMARY KEY (CargaId, Folio),
    CONSTRAINT FK_CarteraConvocatoriaCalculadora_Carga FOREIGN KEY (CargaId) REFERENCES {CalculadoraCargaTable}(CargaId)
);", transaction: tx);
    }

    public async Task<int> GuardarCalculadorasConvocatoriaAsync(CarteraConvocatoriaCalculadoraDocument document, string usuario)
    {
        ArgumentNullException.ThrowIfNull(document);
        using var db = Connection;
        db.Open();
        using var tx = db.BeginTransaction();
        try
        {
            await EnsureCalculadoraTablesAsync(db, tx);
            var existente = await db.QueryFirstOrDefaultAsync<int?>($"SELECT TOP 1 CargaId FROM {CalculadoraCargaTable} WHERE Sha256=@Sha256 AND FechaCorte=@FechaCorte ORDER BY CargaId DESC;",
                new { document.Sha256, FechaCorte = document.FechaCorte.Date }, tx);
            if (existente.HasValue) { tx.Commit(); return 0; }

            var cargaId = await db.ExecuteScalarAsync<int>($@"
INSERT {CalculadoraCargaTable} (NombreArchivo, Sha256, FechaCorte, Filas, CargadoPor)
OUTPUT INSERTED.CargaId VALUES (@FileName, @Sha256, @FechaCorte, @Filas, @Usuario);",
                new { document.FileName, document.Sha256, FechaCorte = document.FechaCorte.Date, Filas = document.Rows.Count, Usuario = usuario }, tx);

            var saved = 0;
            foreach (var r in document.Rows)
            {
                saved += await db.ExecuteAsync($@"
INSERT {CalculadoraTable} (CargaId, Folio, Proyecto, Tecnologia, Inversionista, MwAc, MwDc, SaeMw, SaeMwh, SaeHoras, Cod,
    CapexTotal, CapexCentral, CapexBaterias, CapexInterconexion, DevEx, RetornoProyecto, RetornoPrivado, RetornoObjetivo, RetornoInterconexion,
    ParticipacionPrivada, ContribucionCfe, PrecioEnergia, PlazoPpa, PlazoReversion, Apalancamiento, PlazoDeuda, TirAntesIsr, TirDespuesIsr,
    MoicProyecto, MoicPrivado, EbitdaAcumulado, IngresosAcumulados, UtilidadAcumulada, GeneracionAcumulada, OpexAnio1, Observaciones, DatosJson,
    NombreArchivo, Sha256, CargadoPor)
VALUES (@CargaId, @Folio, @Proyecto, @Tecnologia, @Inversionista, @MwAc, @MwDc, @SaeMw, @SaeMwh, @SaeHoras, @Cod,
    @CapexTotal, @CapexCentral, @CapexBaterias, @CapexInterconexion, @DevEx, @RetornoProyecto, @RetornoPrivado, @RetornoObjetivo, @RetornoInterconexion,
    @ParticipacionPrivada, @ContribucionCfe, @PrecioEnergia, @PlazoPpa, @PlazoReversion, @Apalancamiento, @PlazoDeuda, @TirAntesIsr, @TirDespuesIsr,
    @MoicProyecto, @MoicPrivado, @EbitdaAcumulado, @IngresosAcumulados, @UtilidadAcumulada, @GeneracionAcumulada, @OpexAnio1, @Observaciones, @DatosJson,
    @FileName, @Sha256, @Usuario);",
                    new
                    {
                        CargaId = cargaId, r.Folio, r.Proyecto, r.Tecnologia, r.Inversionista, r.MwAc, r.MwDc, r.SaeMw, r.SaeMwh, r.SaeHoras, r.Cod,
                        r.CapexTotal, r.CapexCentral, r.CapexBaterias, r.CapexInterconexion, r.DevEx, r.RetornoProyecto, r.RetornoPrivado, r.RetornoObjetivo, r.RetornoInterconexion,
                        r.ParticipacionPrivada, r.ContribucionCfe, r.PrecioEnergia, r.PlazoPpa, r.PlazoReversion, r.Apalancamiento, r.PlazoDeuda, r.TirAntesIsr, r.TirDespuesIsr,
                        r.MoicProyecto, r.MoicPrivado, r.EbitdaAcumulado, r.IngresosAcumulados, r.UtilidadAcumulada, r.GeneracionAcumulada, r.OpexAnio1, r.Observaciones,
                        DatosJson = JsonConvert.SerializeObject(r.Campos), r.FileName, r.Sha256, Usuario = usuario
                    }, tx);
            }
            tx.Commit();
            return saved;
        }
        catch { tx.Rollback(); throw; }
    }

    private async Task<int?> UltimaCalculadoraCargaAsync(IDbConnection db)
    {
        if (!await db.ExecuteScalarAsync<bool>($"SELECT CASE WHEN OBJECT_ID(N'{CalculadoraCargaTable}',N'U') IS NULL THEN 0 ELSE 1 END")) return null;
        return await db.QueryFirstOrDefaultAsync<int?>($"SELECT TOP 1 CargaId FROM {CalculadoraCargaTable} ORDER BY FechaCorte DESC, CargaId DESC;");
    }

    public async Task<CarteraConvocatoriaCalculadora?> ObtenerCalculadoraConvocatoriaAsync(string folio)
    {
        using var db = Connection;
        var cargaId = await UltimaCalculadoraCargaAsync(db);
        if (!cargaId.HasValue) return null;
        var fila = await db.QueryFirstOrDefaultAsync<CarteraConvocatoriaCalculadora>($"SELECT {CalculadoraColumns} FROM {CalculadoraTable} WHERE CargaId=@cargaId AND Folio=@folio;", new { cargaId, folio });
        if (fila == null) return null;
        var json = await db.QueryFirstOrDefaultAsync<string?>($"SELECT DatosJson FROM {CalculadoraTable} WHERE CargaId=@cargaId AND Folio=@folio;", new { cargaId, folio });
        if (!string.IsNullOrWhiteSpace(json))
            fila.Campos = JsonConvert.DeserializeObject<List<ConvocatoriaCampoFuente>>(json) ?? new();
        return fila;
    }

    public async Task<List<CarteraConvocatoriaCalculadora>> ObtenerCalculadorasConvocatoriaAsync()
    {
        using var db = Connection;
        var cargaId = await UltimaCalculadoraCargaAsync(db);
        if (!cargaId.HasValue) return new();
        return (await db.QueryAsync<CarteraConvocatoriaCalculadora>($"SELECT {CalculadoraColumns} FROM {CalculadoraTable} WHERE CargaId=@cargaId;", new { cargaId })).ToList();
    }

    public async Task<List<CarteraConvocatoriaCalculadoraCarga>> ObtenerCalculadoraCargasConvocatoriaAsync()
    {
        using var db = Connection;
        if (!await db.ExecuteScalarAsync<bool>($"SELECT CASE WHEN OBJECT_ID(N'{CalculadoraCargaTable}',N'U') IS NULL THEN 0 ELSE 1 END")) return new();
        return (await db.QueryAsync<CarteraConvocatoriaCalculadoraCarga>($"SELECT CargaId, NombreArchivo AS FileName, Sha256, FechaCorte, Filas, CargadoUtc AS LoadedUtc, CargadoPor AS LoadedBy FROM {CalculadoraCargaTable} ORDER BY FechaCorte DESC, CargaId DESC;")).ToList();
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
