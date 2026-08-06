SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @Metodo nvarchar(80)=N'investigacion_individual_conciliada_v2';
DECLARE @E TABLE(ProyectoId bigint PRIMARY KEY,Cantidad int,Lote nvarchar(80));
INSERT @E VALUES
(242,3,N'PAM-UBICACION-INDIVIDUAL-20260803-032'),
(86,1,N'PAM-UBICACION-INDIVIDUAL-20260803-033'),
(266,1,N'PAM-UBICACION-INDIVIDUAL-20260803-034'),
(186,2,N'PAM-UBICACION-INDIVIDUAL-20260803-035'),
(277,4,N'PAM-UBICACION-INDIVIDUAL-20260803-040'),
(281,3,N'PAM-UBICACION-INDIVIDUAL-20260803-041');

BEGIN TRY
 BEGIN TRANSACTION;
 IF EXISTS(
   SELECT 1 FROM @E e
   OUTER APPLY(
     SELECT COUNT(*) Cantidad
     FROM dgmesnie.PAMProyectoUbicacion u WITH(UPDLOCK,HOLDLOCK)
     WHERE u.ProyectoId=e.ProyectoId AND u.MetodoUbicacion=@Metodo
       AND u.Observaciones LIKE N'%' + e.Lote + N'.%'
   ) x WHERE x.Cantidad<>e.Cantidad
 ) THROW 514201,N'Reversión cancelada: el lote ya no coincide con los 14 registros esperados.',1;

 DELETE u
 FROM dgmesnie.PAMProyectoUbicacion u
 JOIN @E e ON e.ProyectoId=u.ProyectoId
 WHERE u.MetodoUbicacion=@Metodo AND u.Observaciones LIKE N'%' + e.Lote + N'.%';

 IF @@ROWCOUNT<>14
  THROW 514202,N'Reversión cancelada: no se eliminaron exactamente 14 registros.',1;
 COMMIT TRANSACTION;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
