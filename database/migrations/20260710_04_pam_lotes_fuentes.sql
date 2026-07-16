SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dgmesnie.PAMLoteActualizacion', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMLoteActualizacion
        (
            LoteId                   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PAMLoteActualizacion PRIMARY KEY,
            Nombre                   NVARCHAR(200) NOT NULL,
            FechaCorte               DATE NOT NULL,
            Notas                    NVARCHAR(1000) NULL,
            Estado                   NVARCHAR(30) NOT NULL CONSTRAINT DF_PAMLote_Estado DEFAULT N'Cargado',
            TotalArchivos            INT NOT NULL CONSTRAINT DF_PAMLote_TotalArchivos DEFAULT (0),
            TotalRegistrosDetectados INT NOT NULL CONSTRAINT DF_PAMLote_Registros DEFAULT (0),
            TotalCambiosPropuestos   INT NOT NULL CONSTRAINT DF_PAMLote_Cambios DEFAULT (0),
            TotalObservados          INT NOT NULL CONSTRAINT DF_PAMLote_Observados DEFAULT (0),
            FechaRegistroUtc         DATETIME2(0) NOT NULL CONSTRAINT DF_PAMLote_FechaRegistro DEFAULT SYSUTCDATETIME(),
            FechaActualizacionUtc    DATETIME2(0) NULL,
            UsuarioId                INT NULL,
            UsuarioNombre            NVARCHAR(150) NULL,
            CONSTRAINT CK_PAMLote_Estado CHECK (Estado IN (N'Cargado', N'Extrayendo', N'En revisión', N'Aplicado', N'Rechazado', N'Error'))
        );
        CREATE INDEX IX_PAMLote_FechaEstado
            ON dgmesnie.PAMLoteActualizacion(FechaRegistroUtc DESC, Estado);
    END;

    IF OBJECT_ID(N'dgmesnie.PAMLoteFuente', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMLoteFuente
        (
            LoteFuenteId             BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PAMLoteFuente PRIMARY KEY,
            LoteId                   BIGINT NOT NULL,
            FuenteId                 BIGINT NOT NULL,
            CargaId                  BIGINT NULL,
            Papel                    NVARCHAR(30) NOT NULL,
            NombreOriginal           NVARCHAR(500) NOT NULL,
            Extension                NVARCHAR(15) NOT NULL,
            TipoContenido            NVARCHAR(150) NULL,
            TamanoBytes              BIGINT NOT NULL,
            FechaRegistroUtc         DATETIME2(0) NOT NULL CONSTRAINT DF_PAMLoteFuente_FechaRegistro DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_PAMLoteFuente_Lote FOREIGN KEY (LoteId) REFERENCES dgmesnie.PAMLoteActualizacion(LoteId),
            CONSTRAINT FK_PAMLoteFuente_Fuente FOREIGN KEY (FuenteId) REFERENCES dgmesnie.PAMFuente(FuenteId),
            CONSTRAINT FK_PAMLoteFuente_Carga FOREIGN KEY (CargaId) REFERENCES dgmesnie.PAMCarga(CargaId),
            CONSTRAINT UQ_PAMLoteFuente UNIQUE (LoteId, FuenteId),
            CONSTRAINT CK_PAMLoteFuente_Papel CHECK (Papel IN (N'Datos', N'Evidencia', N'Mixta')),
            CONSTRAINT CK_PAMLoteFuente_Tamano CHECK (TamanoBytes > 0)
        );
    END;

    IF OBJECT_ID(N'dgmesnie.PAMProyectoVersionFuente', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMProyectoVersionFuente
        (
            ProyectoVersionFuenteId  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PAMProyectoVersionFuente PRIMARY KEY,
            ProyectoVersionId        BIGINT NOT NULL,
            FuenteId                 BIGINT NOT NULL,
            Papel                    NVARCHAR(30) NOT NULL,
            CampoRespaldado          NVARCHAR(150) NULL,
            HojaPaginaSeccion        NVARCHAR(250) NULL,
            Observaciones            NVARCHAR(1000) NULL,
            FechaRegistroUtc         DATETIME2(0) NOT NULL CONSTRAINT DF_PAMVersionFuente_FechaRegistro DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro          NVARCHAR(150) NULL,
            CONSTRAINT FK_PAMVersionFuente_Version FOREIGN KEY (ProyectoVersionId) REFERENCES dgmesnie.PAMProyectoVersion(ProyectoVersionId),
            CONSTRAINT FK_PAMVersionFuente_Fuente FOREIGN KEY (FuenteId) REFERENCES dgmesnie.PAMFuente(FuenteId),
            CONSTRAINT UQ_PAMVersionFuente UNIQUE (ProyectoVersionId, FuenteId, CampoRespaldado),
            CONSTRAINT CK_PAMVersionFuente_Papel CHECK (Papel IN (N'Primaria', N'Complementaria'))
        );
        CREATE INDEX IX_PAMVersionFuente_Fuente ON dgmesnie.PAMProyectoVersionFuente(FuenteId);
    END;

    IF OBJECT_ID(N'dgmesnie.PAMCambioFuente', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMCambioFuente
        (
            CambioFuenteId           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PAMCambioFuente PRIMARY KEY,
            CambioId                 BIGINT NOT NULL,
            FuenteId                 BIGINT NOT NULL,
            Papel                    NVARCHAR(30) NOT NULL,
            HojaPaginaSeccion        NVARCHAR(250) NULL,
            Observaciones            NVARCHAR(1000) NULL,
            FechaRegistroUtc         DATETIME2(0) NOT NULL CONSTRAINT DF_PAMCambioFuente_FechaRegistro DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro          NVARCHAR(150) NULL,
            CONSTRAINT FK_PAMCambioFuente_Cambio FOREIGN KEY (CambioId) REFERENCES dgmesnie.PAMCambio(CambioId),
            CONSTRAINT FK_PAMCambioFuente_Fuente FOREIGN KEY (FuenteId) REFERENCES dgmesnie.PAMFuente(FuenteId),
            CONSTRAINT UQ_PAMCambioFuente UNIQUE (CambioId, FuenteId),
            CONSTRAINT CK_PAMCambioFuente_Papel CHECK (Papel IN (N'Primaria', N'Complementaria'))
        );
        CREATE INDEX IX_PAMCambioFuente_Fuente ON dgmesnie.PAMCambioFuente(FuenteId);
    END;

    INSERT dgmesnie.PAMProyectoVersionFuente
        (ProyectoVersionId, FuenteId, Papel, CampoRespaldado, HojaPaginaSeccion, Observaciones, UsuarioRegistro)
    SELECT v.ProyectoVersionId, v.FuenteId, N'Primaria', NULL, f.HojaPaginaSeccion,
           N'Vínculo inicial generado desde la fuente principal de la versión.', SUSER_SNAME()
    FROM dgmesnie.PAMProyectoVersion v
    INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMProyectoVersionFuente vf
        WHERE vf.ProyectoVersionId = v.ProyectoVersionId
          AND vf.FuenteId = v.FuenteId
          AND vf.CampoRespaldado IS NULL
    );

    INSERT dgmesnie.PAMCambioFuente
        (CambioId, FuenteId, Papel, HojaPaginaSeccion, Observaciones, UsuarioRegistro)
    SELECT c.CambioId, carga.FuenteId, N'Primaria', f.HojaPaginaSeccion,
           N'Vínculo inicial generado desde la carga que originó el cambio.', SUSER_SNAME()
    FROM dgmesnie.PAMCambio c
    INNER JOIN dgmesnie.PAMCarga carga ON carga.CargaId = c.CargaId
    INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = carga.FuenteId
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dgmesnie.PAMCambioFuente cf
        WHERE cf.CambioId = c.CambioId AND cf.FuenteId = carga.FuenteId
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    (SELECT COUNT(*) FROM dgmesnie.PAMProyectoVersionFuente) AS VinculosVersionFuente,
    (SELECT COUNT(*) FROM dgmesnie.PAMCambioFuente) AS VinculosCambioFuente;
