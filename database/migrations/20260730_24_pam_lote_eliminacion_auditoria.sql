/*
  Auditoría independiente para retiros controlados de lotes PAM.
  La tabla no usa llaves foráneas para que la evidencia sobreviva al
  borrado de los registros operativos del lote.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dgmesnie.PAMLoteEliminacionAuditoria', N'U') IS NULL
    BEGIN
        CREATE TABLE dgmesnie.PAMLoteEliminacionAuditoria
        (
            AuditoriaId              BIGINT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PAMLoteEliminacionAuditoria PRIMARY KEY,
            LoteIdOriginal           BIGINT NOT NULL,
            LoteUid                  UNIQUEIDENTIFIER NOT NULL,
            Nombre                   NVARCHAR(200) NOT NULL,
            FechaCorte               DATE NOT NULL,
            EstadoAnterior           NVARCHAR(30) NOT NULL,
            UsuarioLote              NVARCHAR(150) NULL,
            FechaRegistroUtc         DATETIME2(0) NOT NULL,
            FechaEliminacionUtc      DATETIME2(0) NOT NULL
                CONSTRAINT DF_PAMLoteEliminacionAuditoria_Fecha DEFAULT SYSUTCDATETIME(),
            EliminadoPor             NVARCHAR(300) NOT NULL,
            Motivo                   NVARCHAR(1000) NOT NULL,
            TotalArchivos            INT NOT NULL,
            TotalAnalisis            INT NOT NULL,
            TotalHallazgos           INT NOT NULL,
            TotalRevisiones          INT NOT NULL,
            TotalCambios             INT NOT NULL,
            TotalPaquetes            INT NOT NULL,
            FuenteIdOriginal         BIGINT NULL,
            ArchivoNombreOriginal    NVARCHAR(500) NULL,
            HashSha256               CHAR(64) NULL,
            RutaArchivoOriginal      NVARCHAR(1000) NULL,
            RutaCuarentena           NVARCHAR(1000) NULL,
            TamanoBytes              BIGINT NULL,
            SnapshotJson             NVARCHAR(MAX) NOT NULL,
            CONSTRAINT UQ_PAMLoteEliminacionAuditoria_LoteUid UNIQUE (LoteUid),
            CONSTRAINT CK_PAMLoteEliminacionAuditoria_Hash
                CHECK (HashSha256 IS NULL OR LEN(HashSha256) = 64),
            CONSTRAINT CK_PAMLoteEliminacionAuditoria_Conteos
                CHECK
                (
                    TotalArchivos >= 0
                    AND TotalAnalisis >= 0
                    AND TotalHallazgos >= 0
                    AND TotalRevisiones >= 0
                    AND TotalCambios >= 0
                    AND TotalPaquetes >= 0
                )
        );
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
