SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_PAMAplicacionResultado_Tipo'
      AND parent_object_id = OBJECT_ID(N'dgmesnie.PAMAplicacionResultado')
)
BEGIN
    ALTER TABLE dgmesnie.PAMAplicacionResultado
        DROP CONSTRAINT CK_PAMAplicacionResultado_Tipo;
END;

ALTER TABLE dgmesnie.PAMAplicacionResultado WITH CHECK
    ADD CONSTRAINT CK_PAMAplicacionResultado_Tipo
    CHECK
    (
        TipoResultado IN
        (
            N'Cambio aplicado',
            N'Clave vinculada',
            N'Proyecto creado',
            N'Antecedente creado',
            N'Cambio conservado',
            N'Cambio omitido',
            N'Acción descartada'
        )
    );

COMMIT TRANSACTION;
