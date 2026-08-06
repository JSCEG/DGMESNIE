SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @ProyectoId BIGINT=102;
DECLARE @PEM NVARCHAR(30)=N'P18-NE2';
DECLARE @Metodo NVARCHAR(80)=N'investigacion_individual_conciliada_v2';
DECLARE @Lote NVARCHAR(80)=N'PAM-UBICACION-INDIVIDUAL-20260802-011';
DECLARE @Usuario NVARCHAR(300)=N'Codex - autorizado por usuario - 2026-08-02';
DECLARE @Lat decimal(10,7)=CONVERT(decimal(10,7),25.2862306);
DECLARE @Lon decimal(11,7)=CONVERT(decimal(11,7),-101.1099750);

BEGIN TRY
 BEGIN TRANSACTION;

 IF NOT EXISTS(
   SELECT 1
   FROM dgmesnie.vw_PAMProyectoVigente
   WHERE ProyectoId=@ProyectoId
     AND ClaveProyecto=@PEM
     AND NombreProyecto=N'Derramadero entronque Ramos Arizpe Potencia - Salero'
     AND EstadoVigenciaCartera=N'Vigente'
 )
  THROW 511101,N'Preflight: cambio la identidad o vigencia de P18-NE2.',1;

 IF EXISTS(
   SELECT 1
   FROM dgmesnie.PAMProyectoUbicacion WITH(UPDLOCK,HOLDLOCK)
   WHERE ProyectoId=@ProyectoId AND Activa=1
 )
  THROW 511102,N'Preflight: P18-NE2 ya tiene ubicaciones activas.',1;

 IF NOT EXISTS(
   SELECT 1
   FROM dgmesnie.RedElectricaSubestacionInventario
   WHERE RegistroClave=N'atlas_sen:4e45d4910ce8ea5a24844e50bc58b97df58799f6'
     AND Activa=1
     AND NombreNormalizado=N'DERRAMADERO'
     AND TensionKv=400
 )
  THROW 511103,N'Preflight: cambio la identidad del nodo Derramadero en el inventario.',1;

 DECLARE @I TABLE(UbicacionId BIGINT,Etiqueta NVARCHAR(300),EsPrincipal BIT);
 INSERT dgmesnie.PAMProyectoUbicacion(
   ProyectoId,Etiqueta,TipoGeometria,GeometriaJson,Latitud,Longitud,
   PrecisionUbicacion,MetodoUbicacion,Fuente,FechaCorte,RadioSugeridoKm,
   Orden,EsPrincipal,Validada,Activa,UsuarioRegistro,Observaciones
 )
 OUTPUT inserted.UbicacionId,inserted.Etiqueta,inserted.EsPrincipal INTO @I
 SELECT
   @ProyectoId,N'SE Derramadero',N'Point',
   CONCAT(N'{"type":"Point","coordinates":[',CONVERT(NVARCHAR(50),@Lon),N',',CONVERT(NVARCHAR(50),@Lat),N']}'),
   @Lat,@Lon,N'exacta',@Metodo,
   N'CENACE PRODESEN 2019-2033; concurso CFE y SIDOF; coordenada oficial CFE',
   FechaCorte,CONVERT(decimal(8,2),1.00),1,1,1,1,@Usuario,
   LEFT(CONCAT(
     N'SE Derramadero 400 kV; dos alimentadores y reactor 75 MVAr; coordenada CFE 25 17 10.43 N, 101 06 35.91 O. ',
     N'No se publica la LT hasta disponer de traza as built; se conserva discrepancia 3.2 km fisicos/6.4 km-c versus 4.46 km CFE. ',
     N'RegistroClave=atlas_sen:4e45d4910ce8ea5a24844e50bc58b97df58799f6; Lote=',@Lote,N'.'
   ),1000)
 FROM dgmesnie.vw_PAMProyectoVigente
 WHERE ProyectoId=@ProyectoId AND ClaveProyecto=@PEM;

 IF(SELECT COUNT(*) FROM @I)<>1
  THROW 511104,N'Aplicacion: no se inserto exactamente SE Derramadero.',1;

 COMMIT TRANSACTION;
 SELECT * FROM @I;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
