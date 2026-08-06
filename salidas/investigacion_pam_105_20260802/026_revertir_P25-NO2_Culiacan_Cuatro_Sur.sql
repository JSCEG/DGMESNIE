SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
BEGIN TRANSACTION;
DELETE FROM dgmesnie.PAMProyectoUbicacion
WHERE ProyectoId=258
  AND MetodoUbicacion=N'investigacion_individual_conciliada_v2'
  AND Observaciones LIKE N'%Lote=PAM-UBICACION-INDIVIDUAL-20260802-009.%';
IF @@ROWCOUNT<>2
BEGIN
 ROLLBACK TRANSACTION;
 THROW 51905,N'Reversion cancelada: no se localizaron exactamente dos filas.',1;
END;
COMMIT TRANSACTION;
