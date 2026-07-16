SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

/*
  PAM - Trazabilidad de proyectos de transmision
  Fase 1: esquema historico y fotografia inicial de InformePormenorizadoModernizacion.

  El script es idempotente: puede ejecutarse nuevamente sin duplicar la carga inicial.
  No modifica ni elimina registros de dgmesnie.InformePormenorizadoModernizacion.
*/

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dgmesnie.PAMFuente', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMFuente
        (
            FuenteId                 BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PAMFuente PRIMARY KEY,
            NombreDocumento          NVARCHAR(500) NOT NULL,
            TipoDocumento            NVARCHAR(80) NOT NULL,
            FechaDocumento           DATE NULL,
            FechaCorte               DATE NOT NULL,
            RutaArchivo              NVARCHAR(1000) NULL,
            HojaPaginaSeccion        NVARCHAR(250) NULL,
            VersionDocumento         NVARCHAR(100) NULL,
            HashSha256               CHAR(64) NULL,
            Observaciones            NVARCHAR(1000) NULL,
            FechaRegistroUtc         DATETIME2(0) NOT NULL CONSTRAINT DF_PAMFuente_FechaRegistro DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro          NVARCHAR(150) NULL,
            Activo                   BIT NOT NULL CONSTRAINT DF_PAMFuente_Activo DEFAULT (1),
            CONSTRAINT CK_PAMFuente_Hash CHECK (HashSha256 IS NULL OR LEN(HashSha256) = 64)
        );

        CREATE UNIQUE INDEX UX_PAMFuente_HashSha256
            ON dgmesnie.PAMFuente(HashSha256)
            WHERE HashSha256 IS NOT NULL;
    END;

    IF OBJECT_ID(N'dgmesnie.PAMCarga', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMCarga
        (
            CargaId                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PAMCarga PRIMARY KEY,
            FuenteId                 BIGINT NOT NULL,
            TipoCarga                NVARCHAR(50) NOT NULL,
            EstadoCarga              NVARCHAR(30) NOT NULL,
            FechaInicioUtc           DATETIME2(0) NOT NULL CONSTRAINT DF_PAMCarga_FechaInicio DEFAULT SYSUTCDATETIME(),
            FechaFinUtc              DATETIME2(0) NULL,
            UsuarioCarga             NVARCHAR(150) NULL,
            RegistrosLeidos          INT NOT NULL CONSTRAINT DF_PAMCarga_Leidos DEFAULT (0),
            RegistrosNuevos          INT NOT NULL CONSTRAINT DF_PAMCarga_Nuevos DEFAULT (0),
            RegistrosModificados     INT NOT NULL CONSTRAINT DF_PAMCarga_Modificados DEFAULT (0),
            RegistrosSinCambio       INT NOT NULL CONSTRAINT DF_PAMCarga_SinCambio DEFAULT (0),
            RegistrosObservados      INT NOT NULL CONSTRAINT DF_PAMCarga_Observados DEFAULT (0),
            MensajeResultado         NVARCHAR(2000) NULL,
            CONSTRAINT FK_PAMCarga_Fuente FOREIGN KEY (FuenteId) REFERENCES dgmesnie.PAMFuente(FuenteId),
            CONSTRAINT CK_PAMCarga_Estado CHECK (EstadoCarga IN (N'Preliminar', N'Validada', N'Aplicada', N'Rechazada', N'Error'))
        );
    END;

    IF OBJECT_ID(N'dgmesnie.PAMProyecto', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMProyecto
        (
            ProyectoId               BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PAMProyecto PRIMARY KEY,
            ProyectoUid              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PAMProyecto_Uid DEFAULT NEWSEQUENTIALID(),
            ProyectoModernizacionIdOrigen INT NULL,
            FechaAltaUtc             DATETIME2(0) NOT NULL CONSTRAINT DF_PAMProyecto_FechaAlta DEFAULT SYSUTCDATETIME(),
            FechaBajaUtc             DATETIME2(0) NULL,
            Activo                   BIT NOT NULL CONSTRAINT DF_PAMProyecto_Activo DEFAULT (1),
            CONSTRAINT UQ_PAMProyecto_Uid UNIQUE (ProyectoUid)
        );

        CREATE UNIQUE INDEX UX_PAMProyecto_ProyectoModernizacionIdOrigen
            ON dgmesnie.PAMProyecto(ProyectoModernizacionIdOrigen)
            WHERE ProyectoModernizacionIdOrigen IS NOT NULL;
    END;

    IF OBJECT_ID(N'dgmesnie.PAMProyectoVersion', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMProyectoVersion
        (
            ProyectoVersionId        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PAMProyectoVersion PRIMARY KEY,
            ProyectoId               BIGINT NOT NULL,
            NumeroVersion            INT NOT NULL,
            FuenteId                 BIGINT NOT NULL,
            CargaId                  BIGINT NOT NULL,
            VigenteDesde             DATE NOT NULL,
            VigenteHasta             DATE NULL,
            EsVersionVigente         BIT NOT NULL,
            OrigenPrograma           NVARCHAR(30) NOT NULL,
            Programa                 NVARCHAR(150) NOT NULL,
            AnioPrograma             SMALLINT NULL,
            TipoProyecto             NVARCHAR(50) NULL,
            EstadoVigenciaCartera    NVARCHAR(30) NOT NULL,
            MotivoCambio             NVARCHAR(500) NULL,
            ClaveProyecto            NVARCHAR(200) NULL,
            Numero                   INT NULL,
            NumeroOriginal           INT NULL,
            GRT                      NVARCHAR(100) NULL,
            NombreProyecto           NVARCHAR(500) NOT NULL,
            TipoFinanciamiento       NVARCHAR(150) NULL,
            AnioInstruccion          INT NULL,
            EtapaProyecto            NVARCHAR(150) NULL,
            MontoProyectoMdp         DECIMAL(18,2) NULL,
            ElementosEquiposAsociados NVARCHAR(MAX) NULL,
            FechaEstimadaInicio      NVARCHAR(100) NULL,
            FeoIndicadaOficioSener   NVARCHAR(150) NULL,
            FeoFactible              NVARCHAR(150) NULL,
            PorcentajeAvanceEjecucion DECIMAL(9,2) NULL,
            CircunstanciasAtrasos    NVARCHAR(MAX) NULL,
            AccionesMitigacionCorreccion NVARCHAR(MAX) NULL,
            EstadoRealProyecto       NVARCHAR(MAX) NULL,
            ComentariosNivelPriorizacion NVARCHAR(200) NULL,
            ClasificacionSener       NVARCHAR(200) NULL,
            FechaProgramacionTrimestre NVARCHAR(50) NULL,
            QuincenaPublicacion      NVARCHAR(50) NULL,
            UniversoPresentacionPresidencia NVARCHAR(50) NULL,
            Mva                      DECIMAL(18,2) NULL,
            Mvar                     DECIMAL(18,2) NULL,
            KmC                      DECIMAL(18,2) NULL,
            ActivoEnFuente           BIT NOT NULL,
            HashContenido            CHAR(64) NULL,
            FechaRegistroUtc         DATETIME2(0) NOT NULL CONSTRAINT DF_PAMProyectoVersion_FechaRegistro DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro          NVARCHAR(150) NULL,
            CONSTRAINT FK_PAMProyectoVersion_Proyecto FOREIGN KEY (ProyectoId) REFERENCES dgmesnie.PAMProyecto(ProyectoId),
            CONSTRAINT FK_PAMProyectoVersion_Fuente FOREIGN KEY (FuenteId) REFERENCES dgmesnie.PAMFuente(FuenteId),
            CONSTRAINT FK_PAMProyectoVersion_Carga FOREIGN KEY (CargaId) REFERENCES dgmesnie.PAMCarga(CargaId),
            CONSTRAINT UQ_PAMProyectoVersion_Numero UNIQUE (ProyectoId, NumeroVersion),
            CONSTRAINT CK_PAMProyectoVersion_Vigencia CHECK (VigenteHasta IS NULL OR VigenteHasta >= VigenteDesde),
            CONSTRAINT CK_PAMProyectoVersion_Origen CHECK (OrigenPrograma IN (N'PAM', N'PAMRNT', N'OTRO')),
            CONSTRAINT CK_PAMProyectoVersion_Estado CHECK (EstadoVigenciaCartera IN (N'Vigente', N'Cancelado', N'Suspendido', N'Sustituido', N'En revision'))
        );

        CREATE UNIQUE INDEX UX_PAMProyectoVersion_Vigente
            ON dgmesnie.PAMProyectoVersion(ProyectoId)
            WHERE EsVersionVigente = 1;
        CREATE INDEX IX_PAMProyectoVersion_Clave
            ON dgmesnie.PAMProyectoVersion(ClaveProyecto, EsVersionVigente);
        CREATE INDEX IX_PAMProyectoVersion_Corte
            ON dgmesnie.PAMProyectoVersion(VigenteDesde, VigenteHasta);
    END;

    IF OBJECT_ID(N'dgmesnie.PAMProyectoRelacionVersion', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMProyectoRelacionVersion
        (
            ProyectoRelacionVersionId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PAMProyectoRelacionVersion PRIMARY KEY,
            ProyectoPadreId           BIGINT NOT NULL,
            ProyectoHijoId            BIGINT NOT NULL,
            TipoRelacion              NVARCHAR(50) NOT NULL,
            FuenteId                  BIGINT NOT NULL,
            CargaId                   BIGINT NOT NULL,
            VigenteDesde              DATE NOT NULL,
            VigenteHasta              DATE NULL,
            EsRelacionVigente         BIT NOT NULL,
            EstadoValidacion          NVARCHAR(30) NOT NULL,
            HojaPaginaSeccion         NVARCHAR(250) NULL,
            Observaciones             NVARCHAR(1000) NULL,
            FechaRegistroUtc          DATETIME2(0) NOT NULL CONSTRAINT DF_PAMRelacion_FechaRegistro DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro           NVARCHAR(150) NULL,
            CONSTRAINT FK_PAMRelacion_Padre FOREIGN KEY (ProyectoPadreId) REFERENCES dgmesnie.PAMProyecto(ProyectoId),
            CONSTRAINT FK_PAMRelacion_Hijo FOREIGN KEY (ProyectoHijoId) REFERENCES dgmesnie.PAMProyecto(ProyectoId),
            CONSTRAINT FK_PAMRelacion_Fuente FOREIGN KEY (FuenteId) REFERENCES dgmesnie.PAMFuente(FuenteId),
            CONSTRAINT FK_PAMRelacion_Carga FOREIGN KEY (CargaId) REFERENCES dgmesnie.PAMCarga(CargaId),
            CONSTRAINT CK_PAMRelacion_Distintos CHECK (ProyectoPadreId <> ProyectoHijoId),
            CONSTRAINT CK_PAMRelacion_Vigencia CHECK (VigenteHasta IS NULL OR VigenteHasta >= VigenteDesde),
            CONSTRAINT CK_PAMRelacion_Tipo CHECK (TipoRelacion IN (N'Componente de', N'Antecedente', N'Complementario', N'Sustituye a', N'Relacionado territorialmente')),
            CONSTRAINT CK_PAMRelacion_Validacion CHECK (EstadoValidacion IN (N'Pendiente', N'Validada', N'Rechazada'))
        );

        CREATE INDEX IX_PAMRelacion_PadreVigente
            ON dgmesnie.PAMProyectoRelacionVersion(ProyectoPadreId, EsRelacionVigente);
        CREATE INDEX IX_PAMRelacion_HijoVigente
            ON dgmesnie.PAMProyectoRelacionVersion(ProyectoHijoId, EsRelacionVigente);
    END;

    IF OBJECT_ID(N'dgmesnie.PAMCambio', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMCambio
        (
            CambioId                 BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PAMCambio PRIMARY KEY,
            CargaId                  BIGINT NOT NULL,
            ProyectoId               BIGINT NULL,
            ProyectoVersionAnteriorId BIGINT NULL,
            ProyectoVersionNuevaId   BIGINT NULL,
            TipoCambio               NVARCHAR(30) NOT NULL,
            Campo                    NVARCHAR(150) NULL,
            ValorAnterior            NVARCHAR(MAX) NULL,
            ValorNuevo               NVARCHAR(MAX) NULL,
            FechaRegistroUtc         DATETIME2(0) NOT NULL CONSTRAINT DF_PAMCambio_FechaRegistro DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro          NVARCHAR(150) NULL,
            CONSTRAINT FK_PAMCambio_Carga FOREIGN KEY (CargaId) REFERENCES dgmesnie.PAMCarga(CargaId),
            CONSTRAINT FK_PAMCambio_Proyecto FOREIGN KEY (ProyectoId) REFERENCES dgmesnie.PAMProyecto(ProyectoId),
            CONSTRAINT FK_PAMCambio_VersionAnterior FOREIGN KEY (ProyectoVersionAnteriorId) REFERENCES dgmesnie.PAMProyectoVersion(ProyectoVersionId),
            CONSTRAINT FK_PAMCambio_VersionNueva FOREIGN KEY (ProyectoVersionNuevaId) REFERENCES dgmesnie.PAMProyectoVersion(ProyectoVersionId),
            CONSTRAINT CK_PAMCambio_Tipo CHECK (TipoCambio IN (N'Alta', N'Modificacion', N'Baja', N'Reactivacion', N'Cambio relacion'))
        );
    END;

    IF OBJECT_ID(N'dgmesnie.PAMRevisionPendiente', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMRevisionPendiente
        (
            RevisionId               BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PAMRevisionPendiente PRIMARY KEY,
            CargaId                  BIGINT NOT NULL,
            ProyectoIdSugerido       BIGINT NULL,
            ClaveRecibida            NVARCHAR(200) NULL,
            NombreRecibido           NVARCHAR(500) NULL,
            TipoRevision             NVARCHAR(50) NOT NULL,
            Motivo                   NVARCHAR(1000) NOT NULL,
            DatosRecibidosJson       NVARCHAR(MAX) NULL,
            Estado                   NVARCHAR(20) NOT NULL CONSTRAINT DF_PAMRevision_Estado DEFAULT N'Pendiente',
            Resolucion               NVARCHAR(1000) NULL,
            FechaRegistroUtc         DATETIME2(0) NOT NULL CONSTRAINT DF_PAMRevision_FechaRegistro DEFAULT SYSUTCDATETIME(),
            FechaResolucionUtc       DATETIME2(0) NULL,
            UsuarioResolucion        NVARCHAR(150) NULL,
            CONSTRAINT FK_PAMRevision_Carga FOREIGN KEY (CargaId) REFERENCES dgmesnie.PAMCarga(CargaId),
            CONSTRAINT FK_PAMRevision_Proyecto FOREIGN KEY (ProyectoIdSugerido) REFERENCES dgmesnie.PAMProyecto(ProyectoId),
            CONSTRAINT CK_PAMRevision_Estado CHECK (Estado IN (N'Pendiente', N'Aprobada', N'Rechazada')),
            CONSTRAINT CK_PAMRevision_Json CHECK (DatosRecibidosJson IS NULL OR ISJSON(DatosRecibidosJson) = 1)
        );
    END;

    DECLARE @FuenteInicialId BIGINT;
    SELECT @FuenteInicialId = FuenteId
    FROM dgmesnie.PAMFuente
    WHERE NombreDocumento = N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx'
      AND FechaCorte = CONVERT(date, '2025-11-25');

    IF @FuenteInicialId IS NULL
    BEGIN
        INSERT dgmesnie.PAMFuente
        (
            NombreDocumento, TipoDocumento, FechaDocumento, FechaCorte,
            VersionDocumento, Observaciones, UsuarioRegistro
        )
        VALUES
        (
            N'INF Pormenorizado Tablas Rev 251125 SENER.xlsx', N'Excel',
            CONVERT(date, '2025-11-25'), CONVERT(date, '2025-11-25'),
            N'Fotografia inicial PAM',
            N'Fuente de los registros existentes en dgmesnie.InformePormenorizadoModernizacion.',
            SUSER_SNAME()
        );
        SET @FuenteInicialId = SCOPE_IDENTITY();
    END;

    DECLARE @CargaInicialId BIGINT;
    SELECT @CargaInicialId = CargaId
    FROM dgmesnie.PAMCarga
    WHERE FuenteId = @FuenteInicialId
      AND TipoCarga = N'Migracion inicial'
      AND EstadoCarga = N'Aplicada';

    IF @CargaInicialId IS NULL
    BEGIN
        INSERT dgmesnie.PAMCarga
        (
            FuenteId, TipoCarga, EstadoCarga, FechaFinUtc, UsuarioCarga,
            RegistrosLeidos, RegistrosNuevos, MensajeResultado
        )
        SELECT
            @FuenteInicialId, N'Migracion inicial', N'Aplicada', SYSUTCDATETIME(), SUSER_SNAME(),
            COUNT(*), COUNT(*), N'Fotografia inicial creada sin modificar la tabla de origen.'
        FROM dgmesnie.InformePormenorizadoModernizacion;
        SET @CargaInicialId = SCOPE_IDENTITY();
    END;

    INSERT dgmesnie.PAMProyecto (ProyectoModernizacionIdOrigen, FechaAltaUtc, Activo)
    SELECT ipm.ProyectoModernizacionId,
           COALESCE(ipm.FechaRegistro, ipm.FechaCarga, SYSUTCDATETIME()),
           1
    FROM dgmesnie.InformePormenorizadoModernizacion ipm
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dgmesnie.PAMProyecto p
        WHERE p.ProyectoModernizacionIdOrigen = ipm.ProyectoModernizacionId
    );

    INSERT dgmesnie.PAMProyectoVersion
    (
        ProyectoId, NumeroVersion, FuenteId, CargaId, VigenteDesde, VigenteHasta, EsVersionVigente,
        OrigenPrograma, Programa, AnioPrograma, TipoProyecto, EstadoVigenciaCartera, MotivoCambio,
        ClaveProyecto, Numero, NumeroOriginal, GRT, NombreProyecto, TipoFinanciamiento, AnioInstruccion,
        EtapaProyecto, MontoProyectoMdp, ElementosEquiposAsociados, FechaEstimadaInicio,
        FeoIndicadaOficioSener, FeoFactible, PorcentajeAvanceEjecucion, CircunstanciasAtrasos,
        AccionesMitigacionCorreccion, EstadoRealProyecto, ComentariosNivelPriorizacion,
        ClasificacionSener, FechaProgramacionTrimestre, QuincenaPublicacion,
        UniversoPresentacionPresidencia, Mva, Mvar, KmC, ActivoEnFuente, UsuarioRegistro
    )
    SELECT
        p.ProyectoId, 1, @FuenteInicialId, @CargaInicialId, CONVERT(date, '2025-11-25'), NULL, 1,
        N'PAM', N'PAM - Informe Pormenorizado', 2025, N'Proyecto',
        CASE WHEN ipm.EtapaProyecto LIKE N'%Cancelaci%n%' OR ipm.EtapaProyecto LIKE N'%Cancelado%'
             THEN N'Cancelado' ELSE N'Vigente' END,
        N'Migracion inicial desde InformePormenorizadoModernizacion',
        NULLIF(LTRIM(RTRIM(ipm.ClavePem)), N''), ipm.Numero, ipm.NumeroOriginal, ipm.GRT,
        ipm.NombreProyecto, ipm.TipoFinanciamiento, ipm.AnioInstruccion, ipm.EtapaProyecto,
        ipm.MontoProyectoMdp, ipm.ElementosEquiposAsociados, ipm.FechaEstimadaInicio,
        ipm.FeoIndicadaOficioSener, ipm.FeoFactible, ipm.PorcentajeAvanceEjecucion,
        ipm.CircunstanciasAtrasos, ipm.AccionesMitigacionCorreccion, ipm.EstadoRealProyecto,
        ipm.ComentariosNivelPriorizacion, ipm.ClasificacionSener, ipm.FechaProgramacionTrimestre,
        ipm.QuincenaPublicacion, ipm.UniversoPresentacionPresidencia, ipm.Mva, ipm.Mvar, ipm.KmC,
        ipm.Activo, SUSER_SNAME()
    FROM dgmesnie.InformePormenorizadoModernizacion ipm
    INNER JOIN dgmesnie.PAMProyecto p
        ON p.ProyectoModernizacionIdOrigen = ipm.ProyectoModernizacionId
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoVersion v
        WHERE v.ProyectoId = p.ProyectoId AND v.NumeroVersion = 1
    );

    INSERT dgmesnie.PAMCambio
    (
        CargaId, ProyectoId, ProyectoVersionNuevaId, TipoCambio, Campo,
        ValorNuevo, UsuarioRegistro
    )
    SELECT
        @CargaInicialId, v.ProyectoId, v.ProyectoVersionId, N'Alta', N'Registro completo',
        N'Fotografia inicial PAM', SUSER_SNAME()
    FROM dgmesnie.PAMProyectoVersion v
    WHERE v.CargaId = @CargaInicialId
      AND NOT EXISTS
      (
          SELECT 1 FROM dgmesnie.PAMCambio c
          WHERE c.CargaId = @CargaInicialId
            AND c.ProyectoVersionNuevaId = v.ProyectoVersionId
            AND c.TipoCambio = N'Alta'
      );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

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
    v.VigenteDesde, v.ActivoEnFuente, v.FuenteId, f.NombreDocumento AS FuenteDocumento,
    f.FechaCorte, f.HojaPaginaSeccion AS FuenteUbicacion
FROM dgmesnie.PAMProyecto p
INNER JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoId = p.ProyectoId AND v.EsVersionVigente = 1
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
WHERE p.Activo = 1;
');

EXEC(N'
CREATE OR ALTER VIEW dgmesnie.vw_PAMProyectoHistorico
AS
SELECT
    p.ProyectoUid, p.ProyectoModernizacionIdOrigen,
    v.*, f.NombreDocumento AS FuenteDocumento, f.FechaCorte, f.HojaPaginaSeccion AS FuenteUbicacion
FROM dgmesnie.PAMProyecto p
INNER JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoId = p.ProyectoId
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId;
');

EXEC(N'
CREATE OR ALTER VIEW dgmesnie.vw_PAMProyectoJerarquiaVigente
AS
SELECT
    r.ProyectoRelacionVersionId, r.TipoRelacion, r.EstadoValidacion, r.VigenteDesde,
    padre.ProyectoId AS ProyectoPadreId, padre.ClaveProyecto AS ClavePadre, padre.NombreProyecto AS NombrePadre,
    hijo.ProyectoId AS ProyectoHijoId, hijo.ClaveProyecto AS ClaveHijo, hijo.NombreProyecto AS NombreHijo,
    r.FuenteId, f.NombreDocumento AS FuenteDocumento, r.HojaPaginaSeccion, r.Observaciones
FROM dgmesnie.PAMProyectoRelacionVersion r
INNER JOIN dgmesnie.vw_PAMProyectoVigente padre ON padre.ProyectoId = r.ProyectoPadreId
INNER JOIN dgmesnie.vw_PAMProyectoVigente hijo ON hijo.ProyectoId = r.ProyectoHijoId
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = r.FuenteId
WHERE r.EsRelacionVigente = 1;
');

SELECT
    (SELECT COUNT(*) FROM dgmesnie.PAMProyecto) AS Proyectos,
    (SELECT COUNT(*) FROM dgmesnie.PAMProyectoVersion) AS Versiones,
    (SELECT COUNT(*) FROM dgmesnie.vw_PAMProyectoVigente WHERE EstadoVigenciaCartera = N'Vigente') AS VigentesCartera,
    (SELECT COUNT(*) FROM dgmesnie.vw_PAMProyectoVigente WHERE EstadoVigenciaCartera = N'Cancelado') AS Cancelados,
    (SELECT COUNT(*) FROM dgmesnie.PAMFuente) AS Fuentes,
    (SELECT COUNT(*) FROM dgmesnie.PAMCarga) AS Cargas;
