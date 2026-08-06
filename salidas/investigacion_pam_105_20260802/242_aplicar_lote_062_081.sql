-- Ejecutar con sqlcmd -f 65001 para conservar UTF-8.
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
DECLARE @Fuente nvarchar(1000)=N'CENACE: PRODESEN, PAMRNT y diagramas unifilares; coordenadas conciliadas con inventario DGMESNIE/Atlas SEN';

DECLARE @Revisado TABLE(ProyectoId bigint PRIMARY KEY, PEM nvarchar(30));
INSERT @Revisado VALUES
(88,N'D18-BS1'),(91,N'D18-NE3'),(81,N'D18-NT1'),(90,N'D18-OC9'),(74,N'D18-PE4'),
(125,N'D19-NO3'),(127,N'D19-NO5'),(123,N'D19-OC6'),(172,N'I20-SIN1'),(63,N'M18-NO1'),
(70,N'M18-NO2'),(32,N'M18-SIN1'),(106,N'M19-TC1'+NCHAR(13)+NCHAR(10)+N'M19-SEN5'),(160,N'M20-NT1'),(199,N'M21-NT1'),
(249,N'M24-BC1'),(27,N'P16-BS1'),(68,N'P16-BS2'),(41,N'P16-OC4'),(48,N'P17-NE2');

DECLARE @Esperado TABLE(ProyectoId bigint PRIMARY KEY, PEM nvarchar(30), Cantidad int, Lote nvarchar(80));
INSERT @Esperado VALUES
(88,N'D18-BS1',2,N'PAM-UBICACION-INDIVIDUAL-20260803-062'),
(91,N'D18-NE3',2,N'PAM-UBICACION-INDIVIDUAL-20260803-063'),
(90,N'D18-OC9',2,N'PAM-UBICACION-INDIVIDUAL-20260803-065'),
(74,N'D18-PE4',1,N'PAM-UBICACION-INDIVIDUAL-20260803-066'),
(125,N'D19-NO3',2,N'PAM-UBICACION-INDIVIDUAL-20260803-067'),
(127,N'D19-NO5',1,N'PAM-UBICACION-INDIVIDUAL-20260803-068'),
(123,N'D19-OC6',1,N'PAM-UBICACION-INDIVIDUAL-20260803-069'),
(172,N'I20-SIN1',3,N'PAM-UBICACION-INDIVIDUAL-20260803-070'),
(63,N'M18-NO1',3,N'PAM-UBICACION-INDIVIDUAL-20260803-071'),
(70,N'M18-NO2',1,N'PAM-UBICACION-INDIVIDUAL-20260803-072'),
(27,N'P16-BS1',9,N'PAM-UBICACION-INDIVIDUAL-20260803-078'),
(41,N'P16-OC4',3,N'PAM-UBICACION-INDIVIDUAL-20260803-080'),
(48,N'P17-NE2',3,N'PAM-UBICACION-INDIVIDUAL-20260803-081');

DECLARE @P TABLE(
 ProyectoId bigint, Etiqueta nvarchar(300), Lat decimal(10,7), Lon decimal(11,7),
 RegistroClave nvarchar(200), Orden int, EsPrincipal bit, Nota nvarchar(500)
);
INSERT @P VALUES
(88,N'SE El Triunfo',23.7854443,-110.1221765,N'dgmesnie_geojson:se:0a749b790abec8648527',1,1,N'Extremo existente explícito del entronque; Buena Vista y la traza quedan pendientes.'),
(88,N'SE Santiago',23.4724980,-109.6947869,N'dgmesnie_geojson:se:3a361229225f71aa98a4',2,0,N'Extremo existente de Baja California Sur; homónimos de otras regiones descartados.'),
(91,N'SE Tampico',22.3178291,-97.8699381,N'dgmesnie_geojson:se:ae41975d3737cb90e203',1,1,N'Extremo existente explícito; Laguna de Miralta y la traza quedan pendientes.'),
(91,N'SE Chairel',22.2371996,-97.8752586,N'dgmesnie_geojson:se:5686069fbdd0b736e113',2,0,N'Extremo existente explícito del entronque Tampico-Chairel.'),
(90,N'SE Cañada',21.9929984,-102.2436750,N'atlas_sen:586593b148c74f9fbe0099dd11fdddc06b7f4fbb',1,1,N'Sitio existente del extremo Cañada en la región Aguascalientes; no representa la nueva SE.'),
(90,N'SE Margaritas',21.9506415,-102.2820860,N'dgmesnie_geojson:se:fb63e09f358732414cbb',2,0,N'Sitio existente del extremo Margaritas en Aguascalientes; Valle de Aguascalientes queda pendiente.'),
(74,N'SE Lerma',19.7944537,-90.6129791,N'dgmesnie_geojson:se:4cca75244a8a99cd59a9',1,1,N'Extremo existente explícito; Hunxectamán, Mérida y la traza quedan pendientes.'),
(125,N'SE Culiacán Poniente',24.8795565,-107.5466761,N'atlas_sen:f7e6bc77d139a9d4c4fd60210068acfaa814915b',1,1,N'Extremo existente explícito en Culiacán; Santa Fe y la traza quedan pendientes.'),
(125,N'SE Culiacán Uno',24.8184617,-107.4171302,N'dgmesnie_geojson:se:0b1091276080c3226094',2,0,N'Extremo existente explícito; los homónimos Santa Fe de otras regiones fueron descartados.'),
(127,N'SE Oasis',29.6887941,-111.0425419,N'dgmesnie_geojson:se:8896b7457160da98967a',1,1,N'Extremo existente de Hermosillo; Terramara, Viñedos y la traza quedan pendientes.'),
(123,N'SE Sayula',19.8882614,-103.5886984,N'dgmesnie_geojson:se:cd6bf161b80f294ce0aa',1,1,N'Extremo existente explícito; Tapalpa, Centro Logístico y la traza quedan pendientes.'),
(172,N'SE Mazatlán Dos',23.1907241,-106.3530586,N'atlas_sen:5008891c4e7ec9b0068056ae2177d37a94950669',1,1,N'Sitio STATCOM explícito del proyecto interregional.'),
(172,N'SE Seri',28.9295848,-110.9512294,N'atlas_sen:e2ccd917b843a5b98e42d7f9394722372f8a210c',2,0,N'Sitio STATCOM explícito; el inventario reconoce el patio de 230 kV.'),
(172,N'SE Nuevo Casas Grandes',30.5252916,-107.8996422,N'atlas_sen:669b64f1ec70349fa2a793466559791fac78590c',3,0,N'Sitio STATCOM explícito; Primero de Mayo queda pendiente.'),
(63,N'SE Hermosillo Nueve',29.0635775,-110.9608474,N'dgmesnie_geojson:se:324a97bc73aad870f3d5',1,1,N'Sitio de sustitución explícito en el expediente vigente.'),
(63,N'SE Hermosillo Ocho',29.0306715,-110.9192459,N'dgmesnie_geojson:se:885bf2887b89f41f936e',2,0,N'Sitio de sustitución explícito en el expediente vigente.'),
(63,N'SE Hermosillo Uno',29.0590946,-110.9550903,N'dgmesnie_geojson:se:2530f040dfd5e11ce645',3,0,N'Sitio de sustitución explícito; Dynatech y Los Mochis Uno quedan pendientes.'),
(70,N'SE Culiacán Uno',24.8184617,-107.4171302,N'dgmesnie_geojson:se:0b1091276080c3226094',1,1,N'Sitio puntual de la nueva bahía de 115 kV.'),
(27,N'SE Coromuel',24.1974518,-110.2538677,N'dgmesnie_geojson:se:a3ef23681e9be3f8999c',1,1,N'Nodo explícito del corredor; no representa una traza inferida.'),
(27,N'SE Central Punta Prieta II',24.2237887,-110.3093885,N'dgmesnie_geojson:se:53200af359c98099810e',2,0,N'Extremo explícito del entronque Coromuel-Punta Prieta II-Palmira.'),
(27,N'SE Palmira',24.1632892,-110.2962285,N'dgmesnie_geojson:se:852ad57eed1c240c460b',3,0,N'Extremo explícito del entronque en La Paz.'),
(27,N'SE Villa Constitución',25.0192167,-111.6665874,N'dgmesnie_geojson:se:80e9ac4aeb22f593a3ac',4,0,N'Nodo y estación convertidora explícitos del corredor.'),
(27,N'SE Olas Altas',24.0349424,-110.3372705,N'atlas_sen:f72fa24b33630e726bb793cc4ae9e137ca7de3ba',5,0,N'Nodo explícito de 230 kV en Baja California Sur.'),
(27,N'SE Mezquital',27.3758863,-112.4088571,N'dgmesnie_geojson:se:76ed294a6b5893edb65b',6,0,N'Extremo explícito de la línea de corriente directa.'),
(27,N'SE Esperanza',28.8244806,-111.4755511,N'atlas_sen:2d239bebed1464c2f3882606f6113ed3ec3c3140',7,0,N'Extremo explícito del tramo Esperanza-Seri en la GCR Noroeste.'),
(27,N'SE Seri',28.9295848,-110.9512294,N'atlas_sen:e2ccd917b843a5b98e42d7f9394722372f8a210c',8,0,N'Extremo explícito de 400 kV; no se infiere la traza.'),
(27,N'SE CD Los Cabos',22.9725454,-110.0205497,N'dgmesnie_geojson:se:7a617df3a900255f0e2a',9,0,N'Extremo explícito del tramo Olas Altas-CD Los Cabos; El Infiernito y Bahía de Kino quedan pendientes.'),
(41,N'SE Conín',20.5799960,-100.2889464,N'dgmesnie_geojson:se:9dd642d8e6e6c2027ed3',1,1,N'Extremo explícito en Querétaro; Marqués Oriente y la traza quedan pendientes.'),
(41,N'SE Tepeyac',20.6162320,-100.2296347,N'dgmesnie_geojson:se:d475359d421a3e53816a',2,0,N'Extremo explícito en Querétaro; el homónimo de otra región fue descartado.'),
(41,N'SE El Sauz',20.4581732,-100.1211283,N'dgmesnie_geojson:se:1bde891d5afaed2d5466',3,0,N'Extremo explícito de la adecuación San Ildefonso-El Sauz.'),
(48,N'SE Las Mesas',21.3101778,-98.7516213,N'dgmesnie_geojson:se:1e49180a7aeb44522fec',1,1,N'Sitio principal explícito del Banco 1.'),
(48,N'SE Huejutla II',21.1177261,-98.4207756,N'dgmesnie_geojson:se:15e60a7aa997b3262642',2,0,N'Extremo explícito de la LT Las Mesas-Huejutla II.'),
(48,N'SE Tamazunchale',21.2627312,-98.7818626,N'dgmesnie_geojson:se:028e90908507f03c9a33',3,0,N'Extremo explícito del entronque; Axtla, Huasteca y las trazas quedan pendientes.');

BEGIN TRY
 BEGIN TRANSACTION;

 IF (SELECT COUNT(*) FROM @Revisado)<>20 OR (SELECT COUNT(*) FROM @Esperado)<>13 OR (SELECT COUNT(*) FROM @P)<>33
  THROW 516301,N'Preflight: el lote no conserva 20 expedientes, 13 PEM promovibles y 33 componentes.',1;

 IF EXISTS(
   SELECT 1 FROM @Revisado r
   LEFT JOIN dgmesnie.vw_PAMProyectoVigente v
     ON v.ProyectoId=r.ProyectoId AND v.ClaveProyecto=r.PEM AND v.EstadoVigenciaCartera=N'Vigente'
   WHERE v.ProyectoId IS NULL
 ) THROW 516302,N'Preflight: cambió la identidad o vigencia de uno de los veinte PEM revisados.',1;

 IF EXISTS(
   SELECT 1 FROM dgmesnie.PAMProyectoUbicacion u WITH(UPDLOCK,HOLDLOCK)
   JOIN @Esperado e ON e.ProyectoId=u.ProyectoId
   WHERE u.Activa=1
 ) THROW 516303,N'Preflight: uno de los trece PEM promovibles ya tiene ubicaciones activas.',1;

 IF EXISTS(
   SELECT 1 FROM @P p
   WHERE NOT EXISTS(
     SELECT 1 FROM dgmesnie.RedElectricaSubestacionInventario i
     WHERE i.RegistroClave=p.RegistroClave AND i.Activa=1
   )
 ) THROW 516304,N'Preflight: cambió o desapareció un nodo del inventario conciliado.',1;

 IF EXISTS(
   SELECT 1 FROM @P p
   JOIN dgmesnie.RedElectricaSubestacionInventario i
     ON i.RegistroClave=p.RegistroClave AND i.Activa=1
   WHERE ABS(i.Latitud-p.Lat)>0.0000001 OR ABS(i.Longitud-p.Lon)>0.0000001
 ) THROW 516305,N'Preflight: cambió la coordenada de un nodo conciliado.',1;

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

 IF (SELECT COUNT(*) FROM @I)<>33
  THROW 516306,N'Aplicación: no se insertaron exactamente 33 componentes.',1;

 IF EXISTS(
   SELECT 1 FROM @Esperado e
   LEFT JOIN (SELECT ProyectoId,COUNT(*) Cantidad FROM @I GROUP BY ProyectoId) x ON x.ProyectoId=e.ProyectoId
   WHERE ISNULL(x.Cantidad,0)<>e.Cantidad
 ) THROW 516307,N'Aplicación: el conteo por PEM no coincide con el expediente.',1;

 IF EXISTS(
   SELECT 1 FROM @Esperado e
   OUTER APPLY(
     SELECT COUNT(*) Principales FROM dgmesnie.PAMProyectoUbicacion u
     WHERE u.ProyectoId=e.ProyectoId AND u.Activa=1 AND u.EsPrincipal=1
   ) x WHERE x.Principales<>1
 ) THROW 516308,N'Aplicación: cada PEM debe tener exactamente una ubicación principal.',1;

 COMMIT TRANSACTION;
 SELECT ProyectoId,COUNT(*) UbicacionesInsertadas FROM @I GROUP BY ProyectoId ORDER BY ProyectoId;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
