SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @ProyectoId BIGINT=116;
DECLARE @PEM NVARCHAR(30)=N'I19-CE1';
DECLARE @Metodo NVARCHAR(80)=N'investigacion_individual_conciliada_v2';
DECLARE @Lote NVARCHAR(80)=N'PAM-UBICACION-INDIVIDUAL-20260802-007';
DECLARE @Usuario NVARCHAR(300)=N'Codex - autorizado por usuario - 2026-08-02';
BEGIN TRY
 BEGIN TRANSACTION;
 IF NOT EXISTS(SELECT 1 FROM dgmesnie.vw_PAMProyectoVigente WHERE ProyectoId=@ProyectoId AND ClaveProyecto=@PEM AND EstadoVigenciaCartera=N'Vigente')
  THROW 51701,N'Preflight: cambio la identidad o vigencia de I19-CE1.',1;
 IF EXISTS(SELECT 1 FROM dgmesnie.PAMProyectoUbicacion WITH(UPDLOCK,HOLDLOCK) WHERE ProyectoId=@ProyectoId AND Activa=1)
  THROW 51702,N'Preflight: I19-CE1 ya tiene ubicaciones activas.',1;
 CREATE TABLE #G(RegistroClave NVARCHAR(100),Etiqueta NVARCHAR(300),Orden INT,EsPrincipal BIT,Evidencia NVARCHAR(500));
 INSERT #G VALUES
  (N'dgmesnie_geojson:se:c15e1166b7c5627665c7',N'SE Jilotepec Potencia',1,1,N'Bancos, reactores y ampliacion de 400 kV; extremo del corredor oficial.'),
  (N'dgmesnie_geojson:se:1e49180a7aeb44522fec',N'SE Las Mesas',2,0,N'Dos alimentadores de 400 kV; extremo Las Mesas (Tamazunchale) del corredor oficial.');
 IF EXISTS(SELECT 1 FROM #G g LEFT JOIN dgmesnie.RedElectricaSubestacionInventario i ON i.RegistroClave=g.RegistroClave AND i.Activa=1 AND i.TensionKv=400 AND i.EstadoValidacion=N'catalogado' WHERE i.RegistroClave IS NULL)
  THROW 51703,N'Preflight: cambio un nodo de 400 kV seleccionado.',1;
 DECLARE @I TABLE(UbicacionId BIGINT,Etiqueta NVARCHAR(300),EsPrincipal BIT);
 INSERT dgmesnie.PAMProyectoUbicacion(ProyectoId,Etiqueta,TipoGeometria,GeometriaJson,Latitud,Longitud,PrecisionUbicacion,MetodoUbicacion,Fuente,FechaCorte,RadioSugeridoKm,Orden,EsPrincipal,Validada,Activa,UsuarioRegistro,Observaciones)
 OUTPUT inserted.UbicacionId,inserted.Etiqueta,inserted.EsPrincipal INTO @I
 SELECT @ProyectoId,g.Etiqueta,N'Point',CONCAT(N'{"type":"Point","coordinates":[',CONVERT(NVARCHAR(50),CONVERT(decimal(11,7),i.Longitud)),N',',CONVERT(NVARCHAR(50),CONVERT(decimal(10,7),i.Latitud)),N']}'),CONVERT(decimal(10,7),i.Latitud),CONVERT(decimal(11,7),i.Longitud),N'exacta',@Metodo,N'CENACE PRODESEN y diagramas unifilares; concurso CFE 358 SLT; inventario DGMESNIE',p.FechaCorte,CONVERT(decimal(8,2),1.00),g.Orden,g.EsPrincipal,1,1,@Usuario,LEFT(CONCAT(g.Evidencia,N' RegistroClave=',g.RegistroClave,N'; no se publica la linea hasta normalizar Etapas 1/2/3. Lote=',@Lote,N'.'),1000)
 FROM #G g JOIN dgmesnie.RedElectricaSubestacionInventario i ON i.RegistroClave=g.RegistroClave AND i.Activa=1 CROSS JOIN(SELECT TOP(1)FechaCorte FROM dgmesnie.vw_PAMProyectoVigente WHERE ProyectoId=@ProyectoId AND ClaveProyecto=@PEM)p;
 IF(SELECT COUNT(*) FROM @I)<>2 THROW 51704,N'Aplicacion: no se insertaron dos componentes.',1;
 COMMIT TRANSACTION;
 SELECT * FROM @I ORDER BY EsPrincipal DESC,UbicacionId;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
