SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

-- Reversor recuperable. No elimina registros: desactiva únicamente el lote
-- PAM-UBICACION-20260802-01 mientras siga sin validación humana.
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Esperados INT = 49;

    IF
    (
        SELECT COUNT(*)
        FROM dgmesnie.PAMProyectoUbicacion WITH (UPDLOCK, HOLDLOCK)
        WHERE Activa = 1
          AND Validada = 0
          AND Observaciones LIKE N'%Lote=PAM-UBICACION-20260802-01%'
    ) <> @Esperados
        THROW 51100,
            N'Reversión cancelada: el lote activo no contiene exactamente 49 registros no validados.',
            1;

    UPDATE dgmesnie.PAMProyectoUbicacion
       SET Activa = 0,
           Observaciones = LEFT(CONCAT(
               Observaciones,
               N' Desactivada mediante reversor PAM-UBICACION-20260802-01.'), 1000)
     WHERE Activa = 1
       AND Validada = 0
       AND Observaciones LIKE N'%Lote=PAM-UBICACION-20260802-01%';

    IF @@ROWCOUNT <> @Esperados
        THROW 51101,
            N'Reversión cancelada: no se actualizaron exactamente 49 registros.',
            1;

    COMMIT TRANSACTION;

    SELECT COUNT(*) AS RegistrosDesactivados
    FROM dgmesnie.PAMProyectoUbicacion
    WHERE Activa = 0
      AND Validada = 0
      AND Observaciones LIKE N'%Lote=PAM-UBICACION-20260802-01%';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
