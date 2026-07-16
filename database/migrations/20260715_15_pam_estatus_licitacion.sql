SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

/*
  Fase 15: semáforo de estatus de licitación (minuta 2026-07-09).
  Agrega EstatusLicitacion a PAMProyectoVersion con los cinco estados acordados:
    En ejecución (verde) · Concursado (verde claro) · En concurso (amarillo)
    · Por concursar (naranja) · Sin iniciar (rojo).
  NULL = por clasificar (aún sin dato de la fuente); la UI lo muestra como
  "Por clasificar" y no lo confunde con "Sin iniciar".
  El pipeline de actualización llenará la columna en cada corte del Informe
  Pormenorizado; la vista vigente la expone para que el consumo sea transparente.
*/

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH(N'dgmesnie.PAMProyectoVersion', N'EstatusLicitacion') IS NULL
        ALTER TABLE dgmesnie.PAMProyectoVersion ADD EstatusLicitacion NVARCHAR(30) NULL;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_PAMProyectoVersion_EstatusLicitacion'
          AND parent_object_id = OBJECT_ID(N'dgmesnie.PAMProyectoVersion')
    )
    BEGIN
        ALTER TABLE dgmesnie.PAMProyectoVersion WITH CHECK
        ADD CONSTRAINT CK_PAMProyectoVersion_EstatusLicitacion
        CHECK (EstatusLicitacion IS NULL OR EstatusLicitacion IN
               (N'En ejecución', N'Concursado', N'En concurso', N'Por concursar', N'Sin iniciar'));
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_PAMProyectoVersion_EstatusVigente'
          AND object_id = OBJECT_ID(N'dgmesnie.PAMProyectoVersion')
    )
    BEGIN
        CREATE INDEX IX_PAMProyectoVersion_EstatusVigente
            ON dgmesnie.PAMProyectoVersion(EstatusLicitacion)
            WHERE EsVersionVigente = 1;
    END;

    /* Siembra conservadora: solo copia coincidencias EXACTAS de EtapaProyecto
       con el catálogo (no interpreta nada). Idempotente y sin efecto si la
       fuente no usa esos textos. */
    UPDATE v
    SET v.EstatusLicitacion = LTRIM(RTRIM(v.EtapaProyecto))
    FROM dgmesnie.PAMProyectoVersion v
    WHERE v.EsVersionVigente = 1
      AND v.EstatusLicitacion IS NULL
      AND LTRIM(RTRIM(v.EtapaProyecto)) IN
          (N'En ejecución', N'Concursado', N'En concurso', N'Por concursar', N'Sin iniciar');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

EXEC(N'
CREATE OR ALTER VIEW dgmesnie.vw_PAMProyectoVigente
AS
SELECT
    p.ProyectoId, p.ProyectoUid, p.ProyectoModernizacionIdOrigen,
    v.ProyectoVersionId, v.NumeroVersion, v.OrigenPrograma, v.Programa, v.AnioPrograma,
    v.TipoProyecto, v.EstadoVigenciaCartera, v.ClaveProyecto, v.Numero, v.NumeroOriginal,
    v.GRT, v.NombreProyecto, v.TipoFinanciamiento, v.AnioInstruccion, v.EtapaProyecto,
    v.MontoProyectoMdp, v.ElementosEquiposAsociados, v.FechaEstimadaInicio,
    v.FeoIndicadaOficioSener, v.FeoFactible, v.PorcentajeAvanceEjecucion,
    v.CircunstanciasAtrasos, v.AccionesMitigacionCorreccion, v.EstadoRealProyecto,
    v.ComentariosNivelPriorizacion, v.ClasificacionSener, v.FechaProgramacionTrimestre,
    v.QuincenaPublicacion, v.UniversoPresentacionPresidencia, v.Mva, v.Mvar, v.KmC,
    v.PrioridadPrograma, v.ZonaAtendida, v.FechaNecesaria, v.EstatusLicitacion,
    v.VigenteDesde, v.ActivoEnFuente, v.FuenteId, f.NombreDocumento AS FuenteDocumento,
    f.FechaCorte, f.HojaPaginaSeccion AS FuenteUbicacion
FROM dgmesnie.PAMProyecto p
INNER JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoId = p.ProyectoId AND v.EsVersionVigente = 1
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
WHERE p.Activo = 1;
');
GO

/* Radiografía posterior a la migración: distribución del semáforo y valores
   de EtapaProyecto para preparar el mapeo del pipeline. */
SELECT ISNULL(EstatusLicitacion, N'Por clasificar') AS Estatus, COUNT(*) AS Total
FROM dgmesnie.vw_PAMProyectoVigente
WHERE EstadoVigenciaCartera = N'Vigente'
GROUP BY ISNULL(EstatusLicitacion, N'Por clasificar')
ORDER BY Total DESC;

SELECT EtapaProyecto, COUNT(*) AS Total
FROM dgmesnie.vw_PAMProyectoVigente
WHERE EstadoVigenciaCartera = N'Vigente'
GROUP BY EtapaProyecto
ORDER BY Total DESC;
