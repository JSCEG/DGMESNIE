SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @Metodo nvarchar(80)=N'investigacion_individual_conciliada_v2';
DECLARE @E TABLE(ProyectoId bigint PRIMARY KEY,Cantidad int,Lote nvarchar(80));
INSERT @E VALUES
(120,1,N'PAM-UBICACION-INDIVIDUAL-20260803-022'),
(138,1,N'PAM-UBICACION-INDIVIDUAL-20260803-023'),
(161,1,N'PAM-UBICACION-INDIVIDUAL-20260803-024'),
(122,1,N'PAM-UBICACION-INDIVIDUAL-20260803-025'),
(126,1,N'PAM-UBICACION-INDIVIDUAL-20260803-026'),
(83,1,N'PAM-UBICACION-INDIVIDUAL-20260803-027'),
(129,1,N'PAM-UBICACION-INDIVIDUAL-20260803-029'),
(75,2,N'PAM-UBICACION-INDIVIDUAL-20260803-030'),
(238,2,N'PAM-UBICACION-INDIVIDUAL-20260803-031');

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
 ) THROW 513201,N'Reversión cancelada: el lote ya no coincide con los 11 registros esperados.',1;

 DELETE u
 FROM dgmesnie.PAMProyectoUbicacion u
 JOIN @E e ON e.ProyectoId=u.ProyectoId
 WHERE u.MetodoUbicacion=@Metodo AND u.Observaciones LIKE N'%' + e.Lote + N'.%';

 IF @@ROWCOUNT<>11
  THROW 513202,N'Reversión cancelada: no se eliminaron exactamente 11 registros.',1;
 COMMIT TRANSACTION;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
