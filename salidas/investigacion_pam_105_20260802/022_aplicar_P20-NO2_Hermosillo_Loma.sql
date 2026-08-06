SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @ProyectoId BIGINT=140;
DECLARE @PEM NVARCHAR(30)=N'P20-NO2';
DECLARE @Metodo NVARCHAR(80)=N'investigacion_individual_conciliada_v2';
DECLARE @Lote NVARCHAR(80)=N'PAM-UBICACION-INDIVIDUAL-20260802-008';
DECLARE @Usuario NVARCHAR(300)=N'Codex - autorizado por usuario - 2026-08-02';
DECLARE @RegistroClave NVARCHAR(100)=N'atlas_sen:8cbd3f300c0c9c45bea725976d180fe20a46afa7';

BEGIN TRY
 BEGIN TRANSACTION;

 IF NOT EXISTS(
   SELECT 1
   FROM dgmesnie.vw_PAMProyectoVigente
   WHERE ProyectoId=@ProyectoId
     AND ClaveProyecto=@PEM
     AND EstadoVigenciaCartera=N'Vigente'
 )
  THROW 51801,N'Preflight: cambio la identidad o vigencia de P20-NO2.',1;

 IF EXISTS(
   SELECT 1
   FROM dgmesnie.PAMProyectoUbicacion WITH(UPDLOCK,HOLDLOCK)
   WHERE ProyectoId=@ProyectoId AND Activa=1
 )
  THROW 51802,N'Preflight: P20-NO2 ya tiene ubicaciones activas.',1;

 IF NOT EXISTS(
   SELECT 1
   FROM dgmesnie.RedElectricaSubestacionInventario
   WHERE RegistroClave=@RegistroClave
     AND Activa=1
     AND NombreNormalizado=N'HERMOSILLO LOMA'
     AND TensionKv=230
     AND EstadoValidacion=N'validada_automatica_fuente_abierta'
     AND ABS(Latitud-CONVERT(decimal(10,7),29.2012889))<0.0000001
     AND ABS(Longitud-CONVERT(decimal(11,7),-111.0045410))<0.0000001
 )
  THROW 51803,N'Preflight: cambio el nodo conciliado Hermosillo Loma.',1;

 DECLARE @I TABLE(UbicacionId BIGINT,Etiqueta NVARCHAR(300),EsPrincipal BIT);
 INSERT dgmesnie.PAMProyectoUbicacion(
   ProyectoId,Etiqueta,TipoGeometria,GeometriaJson,Latitud,Longitud,
   PrecisionUbicacion,MetodoUbicacion,Fuente,FechaCorte,RadioSugeridoKm,
   Orden,EsPrincipal,Validada,Activa,UsuarioRegistro,Observaciones
 )
 OUTPUT inserted.UbicacionId,inserted.Etiqueta,inserted.EsPrincipal INTO @I
 SELECT
   @ProyectoId,N'SE Hermosillo Loma',N'Point',
   CONCAT(N'{"type":"Point","coordinates":[',
          CONVERT(NVARCHAR(50),CONVERT(decimal(11,7),i.Longitud)),N',',
          CONVERT(NVARCHAR(50),CONVERT(decimal(10,7),i.Latitud)),N']}'),
   CONVERT(decimal(10,7),i.Latitud),CONVERT(decimal(11,7),i.Longitud),
   N'exacta',@Metodo,
   N'SENER PRODESEN 2020-2034; CENACE diagramas unifilares; Atlas SEN/OpenStreetMap',
   p.FechaCorte,CONVERT(decimal(8,2),1.00),1,1,1,1,@Usuario,
   LEFT(CONCAT(
     N'Componente comprobado: banco 2 de 225 MVA 230/115 kV y alimentador en Hermosillo Loma. ',
     N'Quiroga y la LT quedan pendientes: CFE confirma identidad 04-QRG-115-1, pero no coordenada ni traza inequívoca. ',
     N'RegistroClave=',@RegistroClave,N'; Lote=',@Lote,N'.'
   ),1000)
 FROM dgmesnie.RedElectricaSubestacionInventario i
 CROSS JOIN(
   SELECT TOP(1) FechaCorte
   FROM dgmesnie.vw_PAMProyectoVigente
   WHERE ProyectoId=@ProyectoId AND ClaveProyecto=@PEM
 ) p
 WHERE i.RegistroClave=@RegistroClave AND i.Activa=1;

 IF(SELECT COUNT(*) FROM @I)<>1
  THROW 51804,N'Aplicacion: no se inserto exactamente Hermosillo Loma.',1;

 COMMIT TRANSACTION;
 SELECT * FROM @I;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
