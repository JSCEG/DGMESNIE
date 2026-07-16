SET XACT_ABORT ON;
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dgmesnie.PAMProyectoClaveVersion', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMProyectoClaveVersion
        (
            ProyectoClaveVersionId BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PAMProyectoClaveVersion PRIMARY KEY,
            ProyectoId BIGINT NOT NULL,
            ClaveProyecto NVARCHAR(200) NOT NULL,
            TipoClave NVARCHAR(30) NOT NULL,
            FuenteId BIGINT NOT NULL,
            CargaId BIGINT NOT NULL,
            PaqueteDetalleId BIGINT NULL,
            VigenteDesde DATE NOT NULL,
            VigenteHasta DATE NULL,
            EsVigente BIT NOT NULL,
            FechaRegistroUtc DATETIME2(0) NOT NULL
                CONSTRAINT DF_PAMProyectoClaveVersion_Fecha DEFAULT SYSUTCDATETIME(),
            UsuarioRegistro NVARCHAR(150) NOT NULL,
            CONSTRAINT FK_PAMProyectoClaveVersion_Proyecto
                FOREIGN KEY (ProyectoId) REFERENCES dgmesnie.PAMProyecto(ProyectoId),
            CONSTRAINT FK_PAMProyectoClaveVersion_Fuente
                FOREIGN KEY (FuenteId) REFERENCES dgmesnie.PAMFuente(FuenteId),
            CONSTRAINT FK_PAMProyectoClaveVersion_Carga
                FOREIGN KEY (CargaId) REFERENCES dgmesnie.PAMCarga(CargaId),
            CONSTRAINT FK_PAMProyectoClaveVersion_Detalle
                FOREIGN KEY (PaqueteDetalleId) REFERENCES dgmesnie.PAMAplicacionPaqueteDetalle(PaqueteDetalleId),
            CONSTRAINT CK_PAMProyectoClaveVersion_Tipo
                CHECK (TipoClave IN (N'Principal', N'Alterna', N'Fuente')),
            CONSTRAINT CK_PAMProyectoClaveVersion_Clave
                CHECK (NULLIF(LTRIM(RTRIM(ClaveProyecto)), N'') IS NOT NULL),
            CONSTRAINT CK_PAMProyectoClaveVersion_Vigencia
                CHECK (VigenteHasta IS NULL OR VigenteHasta >= VigenteDesde),
            CONSTRAINT UQ_PAMProyectoClaveVersion_Detalle UNIQUE (PaqueteDetalleId)
        );

        CREATE UNIQUE INDEX UX_PAMProyectoClaveVersion_ClaveVigente
            ON dgmesnie.PAMProyectoClaveVersion(ClaveProyecto)
            WHERE EsVigente = 1;

        CREATE INDEX IX_PAMProyectoClaveVersion_Proyecto
            ON dgmesnie.PAMProyectoClaveVersion(ProyectoId, EsVigente)
            INCLUDE (ClaveProyecto, TipoClave, FuenteId, VigenteDesde);
    END;

    IF OBJECT_ID(N'dgmesnie.PAMAplicacionResultado', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMAplicacionResultado
        (
            AplicacionResultadoId BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PAMAplicacionResultado PRIMARY KEY,
            PaqueteAplicacionId BIGINT NOT NULL,
            PaqueteDetalleId BIGINT NOT NULL,
            TipoResultado NVARCHAR(30) NOT NULL,
            ProyectoId BIGINT NOT NULL,
            ProyectoVersionAnteriorId BIGINT NULL,
            ProyectoVersionNuevaId BIGINT NULL,
            CambioId BIGINT NULL,
            ProyectoClaveVersionId BIGINT NULL,
            FechaAplicacionUtc DATETIME2(0) NOT NULL
                CONSTRAINT DF_PAMAplicacionResultado_Fecha DEFAULT SYSUTCDATETIME(),
            UsuarioAplicacionId INT NULL,
            UsuarioAplicacion NVARCHAR(150) NOT NULL,
            CONSTRAINT UQ_PAMAplicacionResultado_Detalle UNIQUE (PaqueteDetalleId),
            CONSTRAINT FK_PAMAplicacionResultado_Paquete
                FOREIGN KEY (PaqueteAplicacionId) REFERENCES dgmesnie.PAMAplicacionPaquete(PaqueteAplicacionId),
            CONSTRAINT FK_PAMAplicacionResultado_Detalle
                FOREIGN KEY (PaqueteDetalleId) REFERENCES dgmesnie.PAMAplicacionPaqueteDetalle(PaqueteDetalleId),
            CONSTRAINT FK_PAMAplicacionResultado_Proyecto
                FOREIGN KEY (ProyectoId) REFERENCES dgmesnie.PAMProyecto(ProyectoId),
            CONSTRAINT FK_PAMAplicacionResultado_VersionAnterior
                FOREIGN KEY (ProyectoVersionAnteriorId) REFERENCES dgmesnie.PAMProyectoVersion(ProyectoVersionId),
            CONSTRAINT FK_PAMAplicacionResultado_VersionNueva
                FOREIGN KEY (ProyectoVersionNuevaId) REFERENCES dgmesnie.PAMProyectoVersion(ProyectoVersionId),
            CONSTRAINT FK_PAMAplicacionResultado_Cambio
                FOREIGN KEY (CambioId) REFERENCES dgmesnie.PAMCambio(CambioId),
            CONSTRAINT FK_PAMAplicacionResultado_Clave
                FOREIGN KEY (ProyectoClaveVersionId) REFERENCES dgmesnie.PAMProyectoClaveVersion(ProyectoClaveVersionId),
            CONSTRAINT CK_PAMAplicacionResultado_Tipo
                CHECK (TipoResultado IN (N'Cambio aplicado', N'Clave vinculada', N'Proyecto creado', N'Antecedente creado'))
        );

        CREATE INDEX IX_PAMAplicacionResultado_Paquete
            ON dgmesnie.PAMAplicacionResultado(PaqueteAplicacionId, AplicacionResultadoId)
            INCLUDE (TipoResultado, ProyectoId, ProyectoVersionNuevaId, CambioId);
    END;

    EXEC sys.sp_executesql N'
        CREATE OR ALTER TRIGGER dgmesnie.TR_PAMAplicacionResultado_AppendOnly
        ON dgmesnie.PAMAplicacionResultado
        INSTEAD OF UPDATE, DELETE
        AS
        BEGIN
            SET NOCOUNT ON;
            THROW 51050, N''Los resultados de una aplicación son inmutables.'', 1;
        END;';

    EXEC(N'
    CREATE OR ALTER VIEW dgmesnie.vw_PAMProyectoClaveVigente
    AS
    SELECT v.ProyectoId, v.ClaveProyecto, N''Principal'' AS TipoClave,
           v.FuenteId, v.CargaId, v.VigenteDesde
    FROM dgmesnie.PAMProyectoVersion v
    WHERE v.EsVersionVigente = 1
      AND NULLIF(LTRIM(RTRIM(v.ClaveProyecto)), N'''') IS NOT NULL

    UNION ALL

    SELECT c.ProyectoId, c.ClaveProyecto, c.TipoClave,
           c.FuenteId, c.CargaId, c.VigenteDesde
    FROM dgmesnie.PAMProyectoClaveVersion c
    WHERE c.EsVigente = 1;');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    OBJECT_ID(N'dgmesnie.PAMProyectoClaveVersion') AS Claves,
    OBJECT_ID(N'dgmesnie.PAMAplicacionResultado') AS Resultados,
    OBJECT_ID(N'dgmesnie.vw_PAMProyectoClaveVigente') AS VistaClaves;
