SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @Metodo nvarchar(80)=N'investigacion_individual_conciliada_v2';
DECLARE @E TABLE(ProyectoId bigint PRIMARY KEY,Cantidad int,Lote nvarchar(80));
INSERT @E VALUES
(227,5,N'PAM-UBICACION-INDIVIDUAL-20260803-012'),
(137,5,N'PAM-UBICACION-INDIVIDUAL-20260803-013'),
(206,4,N'PAM-UBICACION-INDIVIDUAL-20260803-015'),
(223,2,N'PAM-UBICACION-INDIVIDUAL-20260803-016'),
(143,8,N'PAM-UBICACION-INDIVIDUAL-20260803-017'),
(114,5,N'PAM-UBICACION-INDIVIDUAL-20260803-018'),
(184,3,N'PAM-UBICACION-INDIVIDUAL-20260803-019'),
(182,6,N'PAM-UBICACION-INDIVIDUAL-20260803-020');

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
 ) THROW 512201,N'Reversión cancelada: el lote ya no coincide con los 38 registros esperados.',1;

 DELETE u
 FROM dgmesnie.PAMProyectoUbicacion u
 JOIN @E e ON e.ProyectoId=u.ProyectoId
 WHERE u.MetodoUbicacion=@Metodo AND u.Observaciones LIKE N'%' + e.Lote + N'.%';

 IF @@ROWCOUNT<>38
  THROW 512202,N'Reversión cancelada: no se eliminaron exactamente 38 registros.',1;
 COMMIT TRANSACTION;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
