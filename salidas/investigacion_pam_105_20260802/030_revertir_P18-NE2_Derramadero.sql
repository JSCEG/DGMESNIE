SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
BEGIN TRANSACTION;
DELETE FROM dgmesnie.PAMProyectoUbicacion
WHERE ProyectoId=102
  AND MetodoUbicacion=N'investigacion_individual_conciliada_v2'
  AND Observaciones LIKE N'%Lote=PAM-UBICACION-INDIVIDUAL-20260802-011.%';
IF @@ROWCOUNT<>1
BEGIN
 ROLLBACK TRANSACTION;
 THROW 511105,N'Reversion cancelada: no se localizo exactamente una fila.',1;
END;
COMMIT TRANSACTION;
