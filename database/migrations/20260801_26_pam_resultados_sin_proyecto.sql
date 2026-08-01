SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Un detalle descartado u omitido puede no corresponder a un proyecto de la
-- cartera. La bitácora debe conservarlo aun sin un ProyectoId asociado.
IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dgmesnie.PAMAplicacionResultado')
      AND name = N'ProyectoId'
      AND is_nullable = 0
)
BEGIN
    ALTER TABLE dgmesnie.PAMAplicacionResultado
        ALTER COLUMN ProyectoId BIGINT NULL;
END;

COMMIT TRANSACTION;
