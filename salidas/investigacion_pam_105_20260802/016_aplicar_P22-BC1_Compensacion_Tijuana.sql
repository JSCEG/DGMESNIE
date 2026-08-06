SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @ProyectoId BIGINT=218;
DECLARE @PEM NVARCHAR(30)=N'P22-BC1';
DECLARE @Metodo NVARCHAR(80)=N'investigacion_individual_conciliada_v2';
DECLARE @Lote NVARCHAR(80)=N'PAM-UBICACION-INDIVIDUAL-20260802-006';
DECLARE @Usuario NVARCHAR(300)=N'Codex - autorizado por usuario - 2026-08-02';

BEGIN TRY
  BEGIN TRANSACTION;
  IF NOT EXISTS(SELECT 1 FROM dgmesnie.vw_PAMProyectoVigente WHERE ProyectoId=@ProyectoId AND ClaveProyecto=@PEM AND EstadoVigenciaCartera=N'Vigente')
    THROW 51601,N'Preflight: cambio la identidad o vigencia de P22-BC1.',1;
  IF EXISTS(SELECT 1 FROM dgmesnie.PAMProyectoUbicacion WITH(UPDLOCK,HOLDLOCK) WHERE ProyectoId=@ProyectoId AND Activa=1)
    THROW 51602,N'Preflight: P22-BC1 ya tiene ubicaciones activas.',1;

  CREATE TABLE #G(RegistroClave NVARCHAR(100),Etiqueta NVARCHAR(300),Latitud DECIMAL(10,7),Longitud DECIMAL(11,7),Orden INT,EsPrincipal BIT,Evidencia NVARCHAR(500));
  INSERT #G VALUES
   (N'dgmesnie_geojson:se:4a1ed6f83da2d5803bef',N'SE Tijuana I',CONVERT(decimal(10,7),32.5154594),CONVERT(decimal(11,7),-116.8834640),1,1,N'Capacitor 24.3 MVAr/69 kV; CFE RGD TIJUANA 1 y diagrama CENACE.'),
   (N'dgmesnie_geojson:se:7d2b68cc601a063f1ad0',N'SE Francisco Villa',CONVERT(decimal(10,7),32.4872849),CONVERT(decimal(11,7),-116.8411590),2,0,N'Capacitor 24.3 MVAr/69 kV; CFE RGD y diagrama CENACE.'),
   (N'dgmesnie_geojson:se:c8fe132814e8ef53bd24',N'SE Lago',CONVERT(decimal(10,7),32.5037959),CONVERT(decimal(11,7),-116.9268717),3,0,N'Capacitor 24.3 MVAr/69 kV; CFE RGD y diagrama CENACE.'),
   (N'dgmesnie_geojson:se:7f4b7e982b5e38045582',N'SE Seminario',CONVERT(decimal(10,7),32.4714524),CONVERT(decimal(11,7),-116.9253337),4,0,N'Capacitor 16.2 MVAr/69 kV; CFE RGD y diagrama CENACE.'),
   (N'dgmesnie_geojson:se:efeac5b196d6109d2825',N'SE Durazno',CONVERT(decimal(10,7),32.4270293),CONVERT(decimal(11,7),-116.9412616),5,0,N'Capacitor 16.2 MVAr/69 kV; CFE RGD y diagrama CENACE.');
  IF (SELECT COUNT(*) FROM #G)<>5 OR (SELECT SUM(CASE WHEN EsPrincipal=1 THEN 1 ELSE 0 END) FROM #G)<>1
    THROW 51603,N'Preflight: seleccion territorial invalida.',1;
  IF EXISTS(SELECT 1 FROM #G g LEFT JOIN dgmesnie.RedElectricaSubestacionInventario i ON i.RegistroClave=g.RegistroClave AND i.Activa=1 WHERE i.RegistroClave IS NULL)
    THROW 51604,N'Preflight: falta un nodo DGMESNIE seleccionado.',1;

  DECLARE @I TABLE(UbicacionId BIGINT,Etiqueta NVARCHAR(300),EsPrincipal BIT);
  INSERT dgmesnie.PAMProyectoUbicacion(ProyectoId,Etiqueta,TipoGeometria,GeometriaJson,Latitud,Longitud,PrecisionUbicacion,MetodoUbicacion,Fuente,FechaCorte,RadioSugeridoKm,Orden,EsPrincipal,Validada,Activa,UsuarioRegistro,Observaciones)
  OUTPUT inserted.UbicacionId,inserted.Etiqueta,inserted.EsPrincipal INTO @I
  SELECT @ProyectoId,g.Etiqueta,N'Point',CONCAT(N'{"type":"Point","coordinates":[',CONVERT(NVARCHAR(50),g.Longitud),N',',CONVERT(NVARCHAR(50),g.Latitud),N']}'),g.Latitud,g.Longitud,N'geocodificada',@Metodo,N'CENACE PRODECEN, PAMRNT y diagramas unifilares; CFE RGD 2026; inventario DGMESNIE',p.FechaCorte,CONVERT(decimal(8,2),0.20),g.Orden,g.EsPrincipal,1,1,@Usuario,LEFT(CONCAT(g.Evidencia,N' RegistroClave=',g.RegistroClave,N'; la tension del capacitor se valida con CFE/CENACE, no con la clasificacion de la capa. Lote=',@Lote,N'.'),1000)
  FROM #G g CROSS JOIN(SELECT TOP(1)FechaCorte FROM dgmesnie.vw_PAMProyectoVigente WHERE ProyectoId=@ProyectoId AND ClaveProyecto=@PEM)p;
  IF(SELECT COUNT(*) FROM @I)<>5 THROW 51605,N'Aplicacion: no se insertaron cinco componentes.',1;
  COMMIT TRANSACTION;
  SELECT * FROM @I ORDER BY EsPrincipal DESC,UbicacionId;
END TRY
BEGIN CATCH
  IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
  THROW;
END CATCH;
