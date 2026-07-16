SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

/*
  Fase 16: semáforo dinámico.
  1) Agrega 'En operación' al catálogo (la cartera real trae 45 proyectos operando;
     el catálogo de 5 de la minuta no lo contemplaba y no es "En ejecución").
  2) Tabla de equivalencias dgmesnie.PAMEstatusMapeo (EtapaProyecto -> Estatus)
     con los 16 textos reales del corte vigente.
  3) La vista vw_PAMProyectoVigente deriva el estatus:
        COALESCE(dato explícito de la versión, mapeo por etapa)
     => cuando el pipeline aplique un corte nuevo y cambien las etapas,
        el semáforo se recalcula SOLO. Ajustar equivalencias futuras = un INSERT.
  Reglas aplicadas (validar "Actividades y Estudios Previos"):
    Por Concursar (En Proceso de Decisión)  -> Por concursar
    Instruido y CON priorización            -> Por concursar (priorizado, rumbo a concurso)
    Instruido y SIN priorización            -> Sin iniciar
    Instruido 2025 y sin priorización       -> Sin iniciar
    Identificado / Por instruir PAMRNT 2025 -> Sin iniciar (aún no instruidos)
    Ejecución/Construcción                  -> En ejecución
    Actividades y Estudios Previos          -> En ejecución (pre-obra en curso; VALIDAR)
    En Operación                            -> En operación
    Multietapa                              -> estatus de su fase activa
*/

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS
    (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_PAMProyectoVersion_EstatusLicitacion'
          AND parent_object_id = OBJECT_ID(N'dgmesnie.PAMProyectoVersion')
    )
        ALTER TABLE dgmesnie.PAMProyectoVersion
        DROP CONSTRAINT CK_PAMProyectoVersion_EstatusLicitacion;

    ALTER TABLE dgmesnie.PAMProyectoVersion WITH CHECK
    ADD CONSTRAINT CK_PAMProyectoVersion_EstatusLicitacion
    CHECK (EstatusLicitacion IS NULL OR EstatusLicitacion IN
           (N'En operación', N'En ejecución', N'Concursado', N'En concurso', N'Por concursar', N'Sin iniciar'));

    IF OBJECT_ID(N'dgmesnie.PAMEstatusMapeo', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMEstatusMapeo
        (
            EtapaProyecto    NVARCHAR(150) NOT NULL CONSTRAINT PK_PAMEstatusMapeo PRIMARY KEY,
            Estatus          NVARCHAR(30) NOT NULL,
            Nota             NVARCHAR(300) NULL,
            FechaRegistroUtc DATETIME2(0) NOT NULL CONSTRAINT DF_PAMEstatusMapeo_Fecha DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro  NVARCHAR(150) NULL,
            CONSTRAINT CK_PAMEstatusMapeo_Estatus CHECK (Estatus IN
                (N'En operación', N'En ejecución', N'Concursado', N'En concurso', N'Por concursar', N'Sin iniciar'))
        );
    END;

    MERGE dgmesnie.PAMEstatusMapeo AS destino
    USING
    (
        VALUES
            (N'En Concurso', N'En concurso', N'Coincidencia directa con el catálogo.'),
            (N'Por Concursar', N'Por concursar', N'Coincidencia directa con el catálogo.'),
            (N'Por Concursar (En Proceso de Decisión)', N'Por concursar', N'Proceso de decisión previo a convocatoria.'),
            (N'Instruido y CON priorización', N'Por concursar', N'Priorizado: en pipeline rumbo a concurso.'),
            (N'Instruido y SIN priorización', N'Sin iniciar', N'Instruido sin actividad de licitación registrada.'),
            (N'Instruido 2025 y sin priorización', N'Sin iniciar', N'Instruido sin actividad de licitación registrada.'),
            (N'Identificado', N'Sin iniciar', N'Proyecto identificado PAMRNT, aún no instruido.'),
            (N'Por instruir PAMRNT 2025', N'Sin iniciar', N'Pendiente de instrucción.'),
            (N'Actividades y Estudios Previos', N'En ejecución', N'Pre-obra en curso. VALIDAR con Coordinación.'),
            (N'Ejecución/Construcción', N'En ejecución', N'Obra en curso.'),
            (N'En Operación', N'En operación', N'Proyecto concluido y operando.'),
            (N'Etapa 1: Ejecución/Construcción Fase 2: Actividades y Estudios Previos', N'En ejecución', N'Multietapa: fase activa en obra. Candidato a relaciones ''Fase de''.'),
            (N'Etapa 1: En Concurso Etapa 2: Actividades y Estudios Previos', N'En concurso', N'Multietapa: fase activa en concurso. Candidato a relaciones ''Fase de''.'),
            (N'Etapa 1: En Concurso Etapa 2: Ejecución/Construcción Etapa 3: Ejecución/Construcción', N'En concurso', N'Multietapa: fase 1 en concurso. Candidato a relaciones ''Fase de''.'),
            (N'Etapa 1: En Concurso Fase 2: Actividades y Estudios Previos', N'En concurso', N'Multietapa: fase activa en concurso. Candidato a relaciones ''Fase de''.'),
            (N'Etapa 1: En Operación Etapa 2: En concurso', N'En concurso', N'Multietapa: fase 1 opera, fase activa en concurso. Candidato a relaciones ''Fase de''.'),
            (N'Fases 1 y 2 en Ejecución/Construcción', N'En ejecución', N'Multietapa: ambas fases en obra. Candidato a relaciones ''Fase de''.')
    ) AS origen (EtapaProyecto, Estatus, Nota)
    ON destino.EtapaProyecto = origen.EtapaProyecto
    WHEN MATCHED THEN
        UPDATE SET Estatus = origen.Estatus, Nota = origen.Nota
    WHEN NOT MATCHED THEN
        INSERT (EtapaProyecto, Estatus, Nota, UsuarioRegistro)
        VALUES (origen.EtapaProyecto, origen.Estatus, origen.Nota, SUSER_SNAME());

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
    v.PrioridadPrograma, v.ZonaAtendida, v.FechaNecesaria,
    COALESCE(NULLIF(LTRIM(RTRIM(v.EstatusLicitacion)), N''''), m.Estatus) AS EstatusLicitacion,
    v.VigenteDesde, v.ActivoEnFuente, v.FuenteId, f.NombreDocumento AS FuenteDocumento,
    f.FechaCorte, f.HojaPaginaSeccion AS FuenteUbicacion
FROM dgmesnie.PAMProyecto p
INNER JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoId = p.ProyectoId AND v.EsVersionVigente = 1
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
LEFT JOIN dgmesnie.PAMEstatusMapeo m ON m.EtapaProyecto = LTRIM(RTRIM(v.EtapaProyecto))
WHERE p.Activo = 1;
');
GO

/* Radiografía: el semáforo ya derivado tras el mapeo dinámico. */
SELECT ISNULL(EstatusLicitacion, N'Por clasificar') AS Estatus, COUNT(*) AS Total
FROM dgmesnie.vw_PAMProyectoVigente
WHERE EstadoVigenciaCartera = N'Vigente'
GROUP BY ISNULL(EstatusLicitacion, N'Por clasificar')
ORDER BY Total DESC;

SELECT EtapaProyecto, COUNT(*) AS Total
FROM dgmesnie.vw_PAMProyectoVigente
WHERE EstadoVigenciaCartera = N'Vigente' AND EstatusLicitacion IS NULL
GROUP BY EtapaProyecto
ORDER BY Total DESC;
