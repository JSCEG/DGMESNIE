SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @ProyectoId BIGINT=258;
DECLARE @PEM NVARCHAR(30)=N'P25-NO2';
DECLARE @Metodo NVARCHAR(80)=N'investigacion_individual_conciliada_v2';
DECLARE @Lote NVARCHAR(80)=N'PAM-UBICACION-INDIVIDUAL-20260802-009';
DECLARE @Usuario NVARCHAR(300)=N'Codex - autorizado por usuario - 2026-08-02';

BEGIN TRY
 BEGIN TRANSACTION;

 IF NOT EXISTS(
   SELECT 1
   FROM dgmesnie.vw_PAMProyectoVigente
   WHERE ProyectoId=@ProyectoId
     AND ClaveProyecto=@PEM
     AND NombreProyecto=N'Eliminar restricción en la capacidad de transmisión de la LT Culiacán Cuatro - Culiacán Sur'
     AND EstadoVigenciaCartera=N'Vigente'
 )
  THROW 51901,N'Preflight: cambio la identidad o vigencia de P25-NO2.',1;

 IF EXISTS(
   SELECT 1
   FROM dgmesnie.PAMProyectoUbicacion WITH(UPDLOCK,HOLDLOCK)
   WHERE ProyectoId=@ProyectoId AND Activa=1
 )
  THROW 51902,N'Preflight: P25-NO2 ya tiene ubicaciones activas.',1;

 CREATE TABLE #G(
   RegistroClave NVARCHAR(100),
   NombreEsperado NVARCHAR(200),
   TensionEsperada decimal(8,3),
   Etiqueta NVARCHAR(300),
   Orden INT,
   EsPrincipal BIT,
   Evidencia NVARCHAR(500)
 );
 INSERT #G VALUES
  (N'dgmesnie_geojson:se:f9428aa24aa47badba66',N'CULIACAN IV',230,N'SE Culiacán Cuatro (CUC)',1,1,N'Extremo CUC; CFE confirma sitio, coordenadas y barras/bancos de 115 kV.'),
  (N'dgmesnie_geojson:se:54fb817c929af8462be5',N'CULIACAN SUR',115,N'SE Culiacán Sur (CUS)',2,0,N'Extremo CUS; CFE confirma sitio, coordenadas y barras/bancos de 115 kV.');

 IF EXISTS(
   SELECT 1
   FROM #G g
   LEFT JOIN dgmesnie.RedElectricaSubestacionInventario i
     ON i.RegistroClave=g.RegistroClave
    AND i.Activa=1
    AND i.NombreNormalizado=g.NombreEsperado
    AND i.TensionKv=g.TensionEsperada
    AND i.EstadoValidacion=N'catalogado'
   WHERE i.RegistroClave IS NULL
 )
  THROW 51903,N'Preflight: cambio uno de los extremos conciliados de Culiacan.',1;

 DECLARE @I TABLE(UbicacionId BIGINT,Etiqueta NVARCHAR(300),EsPrincipal BIT);
 INSERT dgmesnie.PAMProyectoUbicacion(
   ProyectoId,Etiqueta,TipoGeometria,GeometriaJson,Latitud,Longitud,
   PrecisionUbicacion,MetodoUbicacion,Fuente,FechaCorte,RadioSugeridoKm,
   Orden,EsPrincipal,Validada,Activa,UsuarioRegistro,Observaciones
 )
 OUTPUT inserted.UbicacionId,inserted.Etiqueta,inserted.EsPrincipal INTO @I
 SELECT
   @ProyectoId,g.Etiqueta,N'Point',
   CONCAT(N'{"type":"Point","coordinates":[',
          CONVERT(NVARCHAR(50),CONVERT(decimal(11,7),i.Longitud)),N',',
          CONVERT(NVARCHAR(50),CONVERT(decimal(10,7),i.Latitud)),N']}'),
   CONVERT(decimal(10,7),i.Latitud),CONVERT(decimal(11,7),i.Longitud),
   N'exacta',@Metodo,
   N'CENACE Autoevaluacion 2025; CFE Distribucion coordenadas de subestaciones 2023 y RGD 2026; inventario DGMESNIE',
   p.FechaCorte,CONVERT(decimal(8,2),1.00),g.Orden,g.EsPrincipal,1,1,@Usuario,
   LEFT(CONCAT(
     g.Evidencia,N' RegistroClave=',g.RegistroClave,
     N'; CENACE publica la clave P24-NO2 para el mismo titulo, mientras la cartera vigente usa P25-NO2; no se modifica la clave. ',
     N'No se publica la LT hasta comprobar una traza continua. Lote=',@Lote,N'.'
   ),1000)
 FROM #G g
 JOIN dgmesnie.RedElectricaSubestacionInventario i
   ON i.RegistroClave=g.RegistroClave AND i.Activa=1
 CROSS JOIN(
   SELECT TOP(1) FechaCorte
   FROM dgmesnie.vw_PAMProyectoVigente
   WHERE ProyectoId=@ProyectoId AND ClaveProyecto=@PEM
 ) p;

 IF(SELECT COUNT(*) FROM @I)<>2
  THROW 51904,N'Aplicacion: no se insertaron exactamente dos extremos.',1;

 COMMIT TRANSACTION;
 SELECT * FROM @I ORDER BY EsPrincipal DESC,UbicacionId;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
