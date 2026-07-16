SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

/*
  Fase 18: habilita el tipo de relación 'Fase de' en PAMProyectoRelacionVersion.
  Permite modelar fases constructivas como proyectos hijo (fase 1 concursada,
  fases siguientes por concursar) con su propia FEO, monto y avance, siempre con
  fuente documental. No carga relaciones: esas entran por el pipeline de revisión
  con evidencia, igual que el resto de la jerarquía.
*/

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS
    (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_PAMRelacion_Tipo'
          AND parent_object_id = OBJECT_ID(N'dgmesnie.PAMProyectoRelacionVersion')
    )
        ALTER TABLE dgmesnie.PAMProyectoRelacionVersion
        DROP CONSTRAINT CK_PAMRelacion_Tipo;

    ALTER TABLE dgmesnie.PAMProyectoRelacionVersion WITH CHECK
    ADD CONSTRAINT CK_PAMRelacion_Tipo
    CHECK (TipoRelacion IN
           (N'Componente de', N'Antecedente', N'Complementario', N'Sustituye a',
            N'Relacionado territorialmente', N'Fase de'));

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

SELECT definition AS CheckActual
FROM sys.check_constraints
WHERE name = N'CK_PAMRelacion_Tipo';
