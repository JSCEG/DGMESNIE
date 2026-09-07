-- Ampliación aditiva. Reversión operativa: volver al lector anterior; conservar estas tablas.
-- No actualiza ni elimina proyectos, versiones anteriores ni decisiones de Considerar.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dgmesnie.CarteraConvocatoriaExpediente', N'U') IS NULL
BEGIN
    CREATE TABLE dgmesnie.CarteraConvocatoriaExpediente (
        ExpedienteId BIGINT IDENTITY PRIMARY KEY,
        CargaId BIGINT NOT NULL REFERENCES dgmesnie.CarteraConvocatoriaCarga(CargaId),
        Folio NVARCHAR(60) NOT NULL,
        FuenteSha256 CHAR(64) NOT NULL,
        VersionEsquema INT NOT NULL,
        DatosJson NVARCHAR(MAX) NOT NULL,
        RegistradoUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        RegistradoPor NVARCHAR(150) NOT NULL,
        CONSTRAINT CK_ConvocatoriaExpediente_Json CHECK (ISJSON(DatosJson) = 1),
        CONSTRAINT UQ_ConvocatoriaExpediente_Fuente UNIQUE(CargaId,Folio,FuenteSha256,VersionEsquema)
    );
    CREATE INDEX IX_ConvocatoriaExpediente_Folio ON dgmesnie.CarteraConvocatoriaExpediente(CargaId,Folio,ExpedienteId DESC);
END;
COMMIT;
