SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dgmesnie.PAMCambioPropuesto', N'U') IS NULL
        THROW 51001, N'No existe dgmesnie.PAMCambioPropuesto. Ejecute primero las migraciones PAM anteriores.', 1;

    IF OBJECT_ID(N'dgmesnie.PAMCambioDecisionHistorial', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMCambioDecisionHistorial
        (
            CambioDecisionId       BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PAMCambioDecisionHistorial PRIMARY KEY,
            CambioPropuestoId      BIGINT NOT NULL,
            DecisionCampo          NVARCHAR(20) NOT NULL,
            NotaProyecto           NVARCHAR(1000) NULL,
            UsuarioDecisionId      INT NULL,
            UsuarioDecision        NVARCHAR(150) NOT NULL,
            FechaDecisionUtc       DATETIME2(0) NOT NULL
                CONSTRAINT DF_PAMCambioDecisionHistorial_Fecha DEFAULT SYSUTCDATETIME(),
            VersionDecision        ROWVERSION NOT NULL,
            CONSTRAINT FK_PAMCambioDecisionHistorial_Cambio
                FOREIGN KEY (CambioPropuestoId)
                REFERENCES dgmesnie.PAMCambioPropuesto(CambioPropuestoId),
            CONSTRAINT CK_PAMCambioDecisionHistorial_Decision
                CHECK (DecisionCampo IN (N'Aplicar', N'Mantener SQL', N'Omitir'))
        );
    END;

    IF COL_LENGTH(N'dgmesnie.PAMCambioDecisionHistorial', N'CambioDecisionId') IS NULL
       OR COL_LENGTH(N'dgmesnie.PAMCambioDecisionHistorial', N'CambioPropuestoId') IS NULL
       OR COL_LENGTH(N'dgmesnie.PAMCambioDecisionHistorial', N'DecisionCampo') IS NULL
       OR COL_LENGTH(N'dgmesnie.PAMCambioDecisionHistorial', N'UsuarioDecision') IS NULL
       OR COL_LENGTH(N'dgmesnie.PAMCambioDecisionHistorial', N'FechaDecisionUtc') IS NULL
       OR COL_LENGTH(N'dgmesnie.PAMCambioDecisionHistorial', N'VersionDecision') IS NULL
        THROW 51002, N'PAMCambioDecisionHistorial existe con una estructura incompatible.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dgmesnie.PAMCambioDecisionHistorial')
          AND name = N'IX_PAMCambioDecisionHistorial_CambioActual'
    )
    BEGIN
        CREATE INDEX IX_PAMCambioDecisionHistorial_CambioActual
            ON dgmesnie.PAMCambioDecisionHistorial(CambioPropuestoId, CambioDecisionId DESC)
            INCLUDE (DecisionCampo, NotaProyecto, UsuarioDecisionId, UsuarioDecision, FechaDecisionUtc);
    END;

    EXEC sys.sp_executesql N'
        CREATE OR ALTER TRIGGER dgmesnie.TR_PAMCambioDecisionHistorial_AppendOnly
        ON dgmesnie.PAMCambioDecisionHistorial
        INSTEAD OF UPDATE, DELETE
        AS
        BEGIN
            SET NOCOUNT ON;
            THROW 51003, N''El historial de decisiones de campo es de solo anexado; no admite actualizaciones ni eliminaciones.'', 1;
        END;';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    OBJECT_ID(N'dgmesnie.PAMCambioDecisionHistorial') AS HistorialDecisionesCampo,
    OBJECT_ID(N'dgmesnie.TR_PAMCambioDecisionHistorial_AppendOnly') AS TriggerSoloAnexado;
