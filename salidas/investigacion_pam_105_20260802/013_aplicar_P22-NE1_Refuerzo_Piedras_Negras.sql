SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @ProyectoId BIGINT=208;
DECLARE @PEM NVARCHAR(30)=N'P22-NE1';
DECLARE @Metodo NVARCHAR(80)=N'investigacion_individual_conciliada_v2';
DECLARE @Lote NVARCHAR(80)=N'PAM-UBICACION-INDIVIDUAL-20260802-005';
DECLARE @Usuario NVARCHAR(300)=N'Codex - autorizado por usuario - 2026-08-02';

BEGIN TRY
  BEGIN TRANSACTION;
  IF NOT EXISTS(SELECT 1 FROM dgmesnie.vw_PAMProyectoVigente WHERE ProyectoId=@ProyectoId AND ClaveProyecto=@PEM AND EstadoVigenciaCartera=N'Vigente')
    THROW 51501,N'Preflight: cambio la identidad o vigencia de P22-NE1.',1;
  IF EXISTS(SELECT 1 FROM dgmesnie.PAMProyectoUbicacion WITH(UPDLOCK,HOLDLOCK) WHERE ProyectoId=@ProyectoId AND Activa=1)
    THROW 51502,N'Preflight: P22-NE1 ya tiene ubicaciones activas.',1;
  IF NOT EXISTS(SELECT 1 FROM dgmesnie.RedElectricaSubestacionInventario WHERE RegistroClave=N'atlas_sen:8d54569a37f8a10b6d7f1aea8151e9aeeb5319b8' AND Activa=1 AND Nombre=N'Los Novillos' AND TensionKv=230)
    THROW 51503,N'Preflight: cambio el nodo Los Novillos.',1;
  IF NOT EXISTS(SELECT 1 FROM dgmesnie.RedElectricaSubestacionInventario WHERE RegistroClave=N'atlas_sen:2736a9d7756f2b5e6ac0095e3ff239e6b1065bc5' AND Activa=1 AND Nombre=N'Piedras Negras Potencia' AND TensionKv=230)
    THROW 51504,N'Preflight: cambio el nodo Piedras Negras Potencia.',1;
  IF NOT EXISTS(SELECT 1 FROM dgmesnie.RedElectricaSubestacionInventario WHERE RegistroClave=N'dgmesnie_geojson:se:818725ba453f38d9a7ca' AND Activa=1 AND Nombre=N'S.E. ACUÑA')
    THROW 51505,N'Preflight: cambio el nodo territorial Acuna.',1;

  CREATE TABLE #G(Etiqueta NVARCHAR(300),Latitud DECIMAL(10,7),Longitud DECIMAL(11,7),Orden INT,EsPrincipal BIT,Observacion NVARCHAR(1000));
  INSERT #G VALUES
    (N'SE Los Novillos',CONVERT(decimal(10,7),29.2814901),CONVERT(decimal(11,7),-100.9719904),1,1,N'Banco nuevo 225 MVA 230/138 kV. OSM way/1138449791 confirma nombre, operador CFE y tensiones 230/138 kV.'),
    (N'SE Piedras Negras Potencia',CONVERT(decimal(10,7),28.6739779),CONVERT(decimal(11,7),-100.6136978),2,0,N'Extremo de TC 138 kV. OSM way/102783493 confirma nombre y tensiones 230/138 kV.'),
    (N'SE Acuna I',CONVERT(decimal(10,7),29.2899352),CONVERT(decimal(11,7),-100.9045342),3,0,N'Extremo de TC 138 kV. CFE RGD confirma ACUNA I 138 kV; OSM way/1139059590 coincide espacialmente. La capa DGMESNIE reporta 115 kV y esa tension no se usa como evidencia.');
  IF (SELECT COUNT(*) FROM #G)<>3 OR (SELECT SUM(CASE WHEN EsPrincipal=1 THEN 1 ELSE 0 END) FROM #G)<>1
    THROW 51506,N'Preflight: seleccion territorial invalida.',1;

  DECLARE @I TABLE(UbicacionId BIGINT,Etiqueta NVARCHAR(300),EsPrincipal BIT);
  INSERT dgmesnie.PAMProyectoUbicacion(ProyectoId,Etiqueta,TipoGeometria,GeometriaJson,Latitud,Longitud,PrecisionUbicacion,MetodoUbicacion,Fuente,FechaCorte,RadioSugeridoKm,Orden,EsPrincipal,Validada,Activa,UsuarioRegistro,Observaciones)
  OUTPUT inserted.UbicacionId,inserted.Etiqueta,inserted.EsPrincipal INTO @I
  SELECT @ProyectoId,g.Etiqueta,N'Point',CONCAT(N'{"type":"Point","coordinates":[',CONVERT(NVARCHAR(50),g.Longitud),N',',CONVERT(NVARCHAR(50),g.Latitud),N']}'),g.Latitud,g.Longitud,N'geocodificada',@Metodo,N'CENACE PRODECEN y diagramas unifilares; CFE Transmision y RGD 2026; Atlas SEN, DGMESNIE y OpenStreetMap ODbL',p.FechaCorte,CONVERT(decimal(8,2),0.25),g.Orden,g.EsPrincipal,1,1,@Usuario,LEFT(CONCAT(g.Observacion,N' Lote=',@Lote,N'.'),1000)
  FROM #G g CROSS JOIN(SELECT TOP(1)FechaCorte FROM dgmesnie.vw_PAMProyectoVigente WHERE ProyectoId=@ProyectoId AND ClaveProyecto=@PEM)p;
  IF(SELECT COUNT(*) FROM @I)<>3 THROW 51507,N'Aplicacion: no se insertaron tres componentes.',1;
  COMMIT TRANSACTION;
  SELECT * FROM @I ORDER BY EsPrincipal DESC,UbicacionId;
END TRY
BEGIN CATCH
  IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
  THROW;
END CATCH;
