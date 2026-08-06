SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @Metodo nvarchar(80)=N'investigacion_individual_conciliada_v2';
DECLARE @Usuario nvarchar(300)=N'Codex - autorizado por usuario - 2026-08-03';
DECLARE @Fuente nvarchar(1000)=N'CENACE/SENER: PRODESEN, diagramas unifilares y PAMRNT 2026-2040; coordenadas conciliadas con inventario territorial DGMESNIE/Atlas SEN/OpenStreetMap';

DECLARE @Esperado TABLE(ProyectoId bigint PRIMARY KEY, PEM nvarchar(30), Cantidad int, Lote nvarchar(80));
INSERT @Esperado VALUES
(242,N'P24-NO3',3,N'PAM-UBICACION-INDIVIDUAL-20260803-032'),
(86,N'D18-PE3',1,N'PAM-UBICACION-INDIVIDUAL-20260803-033'),
(266,N'M25-CE1',1,N'PAM-UBICACION-INDIVIDUAL-20260803-034'),
(186,N'P21-NO4',2,N'PAM-UBICACION-INDIVIDUAL-20260803-035'),
(277,N'P26-CE1',4,N'PAM-UBICACION-INDIVIDUAL-20260803-040'),
(281,N'P26-NO2',3,N'PAM-UBICACION-INDIVIDUAL-20260803-041');

DECLARE @P TABLE(
 ProyectoId bigint, Etiqueta nvarchar(300), Lat decimal(10,7), Lon decimal(11,7),
 RegistroClave nvarchar(200), Orden int, EsPrincipal bit, Nota nvarchar(500)
);
INSERT @P VALUES
(242,N'SE Nogales Aeropuerto',31.1619385,-110.9708455,N'dgmesnie_geojson:se:8c576653ed0cec8a68dd',1,1,N'Nodo existente explícito 230 kV; SE Chimeneas y trazas permanecen pendientes.'),
(242,N'SE Nogales Norte',31.3113582,-110.9601362,N'dgmesnie_geojson:se:36e94cdfc657f94002cb',2,0,N'Nodo existente explícito 115 kV del enlace documentado.'),
(242,N'SE Nuevo Nogales',31.2574922,-110.9709674,N'dgmesnie_geojson:se:07dfa5cf81b568a75b4c',3,0,N'Nodo existente explícito 115 kV con adecuaciones documentadas.'),
(86,N'SE Chetumal Norte',18.5326739,-88.2972548,N'dgmesnie_geojson:se:1b7af072b958794fb40a',1,1,N'Extremo existente explícito 115 kV; SE Oxtankah y traza permanecen pendientes.'),
(266,N'SE San Bernabé 400 kV',19.3071525,-99.3433403,N'atlas_sen:2bf3b75104f978d5366cb1ffa9c67cc8d262dcad',1,1,N'Sitio titular exacto en 400 kV; homónimos de 115 kV fueron descartados.'),
(186,N'SE Mazatlán II 115 kV',23.1904581,-106.3532078,N'dgmesnie_geojson:se:f8616df0e14cfb0d4369',1,1,N'Extremo existente explícito del tramo subterráneo; no representa la traza.'),
(186,N'SE Mazatlán Norte',23.2493395,-106.4251850,N'dgmesnie_geojson:se:f01a477917f34c605378',2,0,N'Extremo existente explícito del corte vigente; Mazatlán Tecnológico permanece pendiente.'),
(277,N'SE Pachuca',20.1171990,-98.7601546,N'dgmesnie_geojson:se:7aa07b7ebe12a320471b',1,1,N'Nodo existente del área de influencia común; no implica alternativa seleccionada.'),
(277,N'SE Cubitos',20.1130000,-98.7333500,N'openstreetmap:a5820bd51ebb057cc925e6030d30cede45249f60',2,0,N'Nodo existente explícito en ambas alternativas; coordenada abierta conciliada con diagrama institucional.'),
(277,N'SE Dos Carlos',20.1146800,-98.6944100,N'openstreetmap:8e99c555e30b50bc4f88b7c8adc534b7c227c7d3',3,0,N'Nodo existente explícito en ambas alternativas; coordenada abierta conciliada con diagrama institucional.'),
(277,N'SE Pachuca Potencia',20.0611599,-98.7318653,N'dgmesnie_geojson:se:eb092a282c228e3dc5a8',4,0,N'Nodo fuente existente del área de influencia; no implica selección de obra.'),
(281,N'SE Culiacán Tres',24.8296920,-107.3617586,N'dgmesnie_geojson:se:5d6d2e96074cfa36f1cc',1,1,N'Nodo existente explícito en ambas alternativas; no se registra obra ejecutada.'),
(281,N'SE Culiacán Poniente',24.8795565,-107.5466761,N'atlas_sen:f7e6bc77d139a9d4c4fd60210068acfaa814915b',2,0,N'Nodo existente del área de estudio; alternativa no seleccionada.'),
(281,N'SE Culiacán Uno',24.8184617,-107.4171302,N'dgmesnie_geojson:se:0b1091276080c3226094',3,0,N'Nodo existente explícito del área de estudio; no se usa la genérica SE Norte.');

BEGIN TRY
 BEGIN TRANSACTION;

 IF (SELECT COUNT(*) FROM @P)<>14
  THROW 514101,N'Preflight: el lote no contiene exactamente 14 componentes.',1;

 IF EXISTS(
   SELECT 1 FROM @Esperado e
   LEFT JOIN dgmesnie.vw_PAMProyectoVigente v
     ON v.ProyectoId=e.ProyectoId AND v.ClaveProyecto=e.PEM AND v.EstadoVigenciaCartera=N'Vigente'
   WHERE v.ProyectoId IS NULL
 ) THROW 514102,N'Preflight: cambió la identidad o vigencia de uno de los seis PEM.',1;

 IF EXISTS(
   SELECT 1 FROM dgmesnie.PAMProyectoUbicacion u WITH(UPDLOCK,HOLDLOCK)
   JOIN @Esperado e ON e.ProyectoId=u.ProyectoId
   WHERE u.Activa=1
 ) THROW 514103,N'Preflight: uno de los seis PEM ya tiene ubicaciones activas.',1;

 IF EXISTS(
   SELECT 1 FROM @P p
   WHERE NOT EXISTS(
     SELECT 1 FROM dgmesnie.RedElectricaSubestacionInventario i
     WHERE i.RegistroClave=p.RegistroClave AND i.Activa=1
   )
 ) THROW 514104,N'Preflight: cambió o desapareció un nodo del inventario conciliado.',1;

 IF EXISTS(
   SELECT 1 FROM @P p
   JOIN dgmesnie.RedElectricaSubestacionInventario i
     ON i.RegistroClave=p.RegistroClave AND i.Activa=1
   WHERE ABS(i.Latitud-p.Lat)>0.0000001 OR ABS(i.Longitud-p.Lon)>0.0000001
 ) THROW 514105,N'Preflight: cambió la coordenada de un nodo conciliado.',1;

 DECLARE @I TABLE(ProyectoId bigint, UbicacionId bigint, Etiqueta nvarchar(300));
 INSERT dgmesnie.PAMProyectoUbicacion(
   ProyectoId,Etiqueta,TipoGeometria,GeometriaJson,Latitud,Longitud,
   PrecisionUbicacion,MetodoUbicacion,Fuente,FechaCorte,RadioSugeridoKm,
   Orden,EsPrincipal,Validada,Activa,UsuarioRegistro,Observaciones
 )
 OUTPUT inserted.ProyectoId,inserted.UbicacionId,inserted.Etiqueta INTO @I
 SELECT p.ProyectoId,p.Etiqueta,N'Point',
   CONCAT(N'{"type":"Point","coordinates":[',CONVERT(nvarchar(50),p.Lon),N',',CONVERT(nvarchar(50),p.Lat),N']}'),
   p.Lat,p.Lon,N'exacta',@Metodo,@Fuente,v.FechaCorte,CONVERT(decimal(8,2),0.50),
   p.Orden,p.EsPrincipal,1,1,@Usuario,
   LEFT(CONCAT(p.Nota,N' RegistroClave=',p.RegistroClave,N'; Lote=',e.Lote,N'.'),1000)
 FROM @P p
 JOIN @Esperado e ON e.ProyectoId=p.ProyectoId
 JOIN dgmesnie.vw_PAMProyectoVigente v ON v.ProyectoId=p.ProyectoId AND v.ClaveProyecto=e.PEM;

 IF (SELECT COUNT(*) FROM @I)<>14
  THROW 514106,N'Aplicación: no se insertaron exactamente 14 componentes.',1;

 IF EXISTS(
   SELECT 1 FROM @Esperado e
   LEFT JOIN (SELECT ProyectoId,COUNT(*) Cantidad FROM @I GROUP BY ProyectoId) x ON x.ProyectoId=e.ProyectoId
   WHERE ISNULL(x.Cantidad,0)<>e.Cantidad
 ) THROW 514107,N'Aplicación: el conteo por PEM no coincide con el expediente.',1;

 COMMIT TRANSACTION;
 SELECT ProyectoId,COUNT(*) UbicacionesInsertadas FROM @I GROUP BY ProyectoId ORDER BY ProyectoId;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
