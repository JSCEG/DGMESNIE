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
DECLARE @Fuente nvarchar(1000)=N'CENACE/CFE: PAMRNT, PRODESEN, diagramas y contratación; coordenadas conciliadas con inventario DGMESNIE/Atlas SEN y fuente abierta nominada';

DECLARE @Revisado TABLE(ProyectoId bigint PRIMARY KEY, PEM nvarchar(30));
INSERT @Revisado VALUES
(52,N'P17-NT5'),(51,N'P17-OC5'),(30,N'P18-BS6'),(39,N'P18-OC1'),(60,N'P18-OC8'),(38,N'P18-OC9'),
(65,N'P18-OR2'),(105,N'P19-NO1'),(115,N'P19-OC3'),(109,N'P19-OR2'),(151,N'P20-BS1'),(152,N'P20-BS2'),
(141,N'P20-NO4'),(175,N'P21-NO1'),(174,N'P21-NO2'),(185,N'P21-NO3'),(179,N'P21-OC1'),(177,N'P21-OR2'),
(210,N'P22-OR3'),(232,N'P23-BS1'),(239,N'P24-OC3'),(272,N'Sin PEM'),(273,N'Sin PEM'),(274,N'Sin PEM');

DECLARE @Esperado TABLE(ProyectoId bigint PRIMARY KEY, PEM nvarchar(30), Cantidad int, Lote nvarchar(80));
INSERT @Esperado VALUES
(51,N'P17-OC5',5,N'PAM-UBICACION-INDIVIDUAL-20260803-083'),
(30,N'P18-BS6',1,N'PAM-UBICACION-INDIVIDUAL-20260803-084'),
(60,N'P18-OC8',6,N'PAM-UBICACION-INDIVIDUAL-20260803-086'),
(38,N'P18-OC9',4,N'PAM-UBICACION-INDIVIDUAL-20260803-087'),
(65,N'P18-OR2',3,N'PAM-UBICACION-INDIVIDUAL-20260803-088'),
(115,N'P19-OC3',4,N'PAM-UBICACION-INDIVIDUAL-20260803-090'),
(109,N'P19-OR2',3,N'PAM-UBICACION-INDIVIDUAL-20260803-091'),
(152,N'P20-BS2',1,N'PAM-UBICACION-INDIVIDUAL-20260803-093'),
(185,N'P21-NO3',1,N'PAM-UBICACION-INDIVIDUAL-20260803-097'),
(179,N'P21-OC1',4,N'PAM-UBICACION-INDIVIDUAL-20260803-098'),
(177,N'P21-OR2',5,N'PAM-UBICACION-INDIVIDUAL-20260803-099'),
(210,N'P22-OR3',2,N'PAM-UBICACION-INDIVIDUAL-20260803-100'),
(232,N'P23-BS1',1,N'PAM-UBICACION-INDIVIDUAL-20260803-101'),
(239,N'P24-OC3',9,N'PAM-UBICACION-INDIVIDUAL-20260803-102'),
(273,N'Sin PEM',1,N'PAM-UBICACION-INDIVIDUAL-20260803-104');

DECLARE @P TABLE(
 ProyectoId bigint, Etiqueta nvarchar(300), Lat decimal(10,7), Lon decimal(11,7),
 RegistroClave nvarchar(200), Orden int, EsPrincipal bit, Nota nvarchar(500)
);
INSERT @P VALUES
(51,N'SE Zimapán P.H.',20.8480791,-99.4575617,N'dgmesnie_geojson:se:3b0eefb1f91dc43229e5',1,1,N'CH Zimapán es extremo explícito; Valle del Mezquital y las trazas quedan pendientes.'),
(51,N'SE Zimapán',20.6861095,-99.3344060,N'dgmesnie_geojson:se:f24688039afa7490c0d0',2,0,N'Extremo explícito del entronque en 115 kV; no representa Tap Zimapán.'),
(51,N'SE Dañú',20.2124890,-99.7177912,N'dgmesnie_geojson:se:b0235ee386b2235c631e',3,0,N'Extremo explícito de la LT CH Zimapán-Dañú.'),
(51,N'SE Huichapan',20.3497537,-99.6565384,N'dgmesnie_geojson:se:6af246e93fd2456c0fef',4,0,N'Sitio explícito del banco de compensación.'),
(51,N'SE Humedades',20.4532555,-99.1838213,N'dgmesnie_geojson:se:ec907a4adc3fe6288295',5,0,N'Sitio explícito del banco de compensación.'),
(30,N'SE Recreo MVAr',24.1048900,-110.3388900,N'openstreetmap:c6edf9ba449bdfafc0b2a5b0c28c17cb659b7001',1,1,N'Sitio exacto conciliado con expediente y documentación CFE en La Paz, BCS.'),
(60,N'SE Castillo',20.4954633,-103.2468245,N'dgmesnie_geojson:se:8a822323828541e8b6ea',1,1,N'Sitio explícito en la zona Guadalajara; el inventario conserva el emplazamiento.'),
(60,N'SE Chapala',20.3130386,-103.2006055,N'dgmesnie_geojson:se:4ca9d32afcd096134b45',2,0,N'Sitio explícito del traslado desde Mojonera.'),
(60,N'SE Miravalle',20.6152564,-103.3614008,N'dgmesnie_geojson:se:259cb2f6a16d8fd2cc56',3,0,N'Sitio de Guadalajara; el homónimo de Monterrey fue descartado.'),
(60,N'SE Mojonera',20.7189793,-103.4544639,N'dgmesnie_geojson:se:e9e8398c34d8ef4f95e4',4,0,N'Sitio explícito de compensación en Guadalajara.'),
(60,N'SE El Sol',20.6412546,-103.4297362,N'dgmesnie_geojson:se:5c82c93994ed2c391385',5,0,N'Sitio de Guadalajara conciliado con el registro abierto de 69 kV; homónimo descartado.'),
(60,N'SE San Agustín',20.5591322,-103.4646282,N'dgmesnie_geojson:se:89a64b161cff652c4e60',6,0,N'Sitio de Guadalajara; el homónimo de Monterrey fue descartado.'),
(38,N'SE Conín',20.5799960,-100.2889464,N'dgmesnie_geojson:se:9dd642d8e6e6c2027ed3',1,1,N'Sitio explícito de compensación en la zona Querétaro.'),
(38,N'SE Cimatario',20.5829216,-100.3562067,N'dgmesnie_geojson:se:abe47a0005659a15ecb9',2,0,N'Sitio explícito de compensación.'),
(38,N'SE Querétaro I',20.6428813,-100.4382466,N'dgmesnie_geojson:se:a2e8aae20012fe46f1cf',3,0,N'Sitio explícito denominado Querétaro en el expediente.'),
(38,N'SE Querétaro Potencia',20.5204727,-100.4871176,N'dgmesnie_geojson:se:43639d8f0dda9720f99a',4,0,N'Sitio explícito; Parque Innovación y Antea quedan pendientes.'),
(65,N'SE La Malinche',19.3869020,-98.0933465,N'atlas_sen:3e07721179c6950f737b0a9234a7fa64958dddeb',1,1,N'Sitio explícito del proyecto de Tlaxcala.'),
(65,N'SE Zocac',19.4982835,-98.0463474,N'dgmesnie_geojson:se:2ffe443413e2f202fd31',2,0,N'Extremo explícito; no se infiere la traza a La Malinche.'),
(65,N'SE Apizaco II',19.4467238,-98.1521053,N'dgmesnie_geojson:se:68ae35bb27d5d5e1b372',3,0,N'Extremo explícito; no se infiere la traza a La Malinche.'),
(115,N'SE Las Delicias',21.2474813,-100.6125144,N'dgmesnie_geojson:se:91d84f12968e347e2370',1,1,N'Extremo y sitio explícito de la obra de 230 kV.'),
(115,N'SE Querétaro I',20.6428813,-100.4382466,N'dgmesnie_geojson:se:a2e8aae20012fe46f1cf',2,0,N'Extremo explícito denominado Querétaro.'),
(115,N'SE Conín',20.5799960,-100.2889464,N'dgmesnie_geojson:se:9dd642d8e6e6c2027ed3',3,0,N'Sitio explícito de sustitución de transformadores de corriente.'),
(115,N'SE Querétaro Potencia',20.5204727,-100.4871176,N'dgmesnie_geojson:se:43639d8f0dda9720f99a',4,0,N'Extremo explícito; la línea queda pendiente de traza oficial.'),
(109,N'SE Puebla 2000',19.0644781,-98.1514105,N'dgmesnie_geojson:se:697408de620746b19be1',1,1,N'Sitio del entronque explícito Puebla Dos Mil.'),
(109,N'SE Puebla II',19.0819710,-98.1376489,N'dgmesnie_geojson:se:3b510e879adf3cbdedf1',2,0,N'Extremo explícito de la línea existente.'),
(109,N'SE Guadalupe Analco',19.0006065,-98.1788495,N'dgmesnie_geojson:se:e66939e91b7dda72e65d',3,0,N'Extremo explícito; el tramo de 0.2 km no se infiere.'),
(152,N'SE El Palmar',23.0201603,-109.8179180,N'dgmesnie_geojson:se:fa1ce72494331b34219d',1,1,N'Sitio y extremo explícito en BCS; Monte Real y la traza quedan pendientes.'),
(185,N'SE Los Mochis Industrial',25.8185652,-108.9019011,N'atlas_sen:9d664f908532f4b990d2b2520d6fea636fc026fa',1,1,N'Extremo explícito; Ruiz Cortines y el tramo quedan pendientes.'),
(179,N'SE Querétaro Potencia',20.5204727,-100.4871176,N'dgmesnie_geojson:se:43639d8f0dda9720f99a',1,1,N'Extremo explícito de la red de 115 kV.'),
(179,N'SE Querétaro Sur',20.5353649,-100.4537071,N'dgmesnie_geojson:se:74ed3b65d7e228a5d777',2,0,N'Extremo explícito; no se infiere la traza.'),
(179,N'SE Satélite',20.6163991,-100.4466502,N'dgmesnie_geojson:se:dea367e432bf4f435056',3,0,N'Nodo explícito de dos segmentos.'),
(179,N'SE Querétaro I',20.6428813,-100.4382466,N'dgmesnie_geojson:se:a2e8aae20012fe46f1cf',4,0,N'Extremo explícito denominado Querétaro Uno; La Loma queda pendiente.'),
(177,N'SE Cuautla II',18.8460605,-98.9318028,N'dgmesnie_geojson:se:a2a85bcaa857186bc3da',1,1,N'Extremo común explícito de los entronques.'),
(177,N'SE Cuautla Industrial',18.7416500,-98.9118454,N'dgmesnie_geojson:se:99fcb7ad9825b33fee55',2,0,N'Extremo explícito del primer entronque.'),
(177,N'SE Tepalcingo',18.6025528,-98.8406502,N'dgmesnie_geojson:se:04f3172b4e3919814b28',3,0,N'Extremo explícito del segundo entronque.'),
(177,N'SE Yautepec Potencia',18.8474518,-98.9982265,N'dgmesnie_geojson:se:308ddb4f70c9bd5b9019',4,0,N'Sitio explícito de sustitución.'),
(177,N'SE Jiutepec',18.8897524,-99.1486773,N'dgmesnie_geojson:se:25acb731cbcb728359b7',5,0,N'Sitio explícito; Ciclo Combinado Centro queda pendiente.'),
(210,N'SE Coapan',18.4401865,-97.3792968,N'dgmesnie_geojson:se:71fa57f8d9689093edf8',1,1,N'Sitio explícito de compensación.'),
(210,N'SE Tehuacán',18.4988825,-97.4045655,N'dgmesnie_geojson:se:90d3edd7a1c2bc84b680',2,0,N'Sitio explícito de sustitución; Zinacantepec queda pendiente.'),
(232,N'SE Olas Altas',24.0349424,-110.3372705,N'atlas_sen:f72fa24b33630e726bb793cc4ae9e137ca7de3ba',1,1,N'Extremo explícito; Turbogás Los Cabos y la traza quedan pendientes.'),
(239,N'SE Querétaro Potencia',20.5204727,-100.4871176,N'dgmesnie_geojson:se:43639d8f0dda9720f99a',1,1,N'Nodo troncal explícito del proyecto regional.'),
(239,N'SE Celaya II',20.5738971,-100.8202428,N'dgmesnie_geojson:se:7194d2cabf03b9ac4f82',2,0,N'Extremo explícito de la LT hacia Toyota Maniobras.'),
(239,N'SE La Fragua',20.9724449,-100.4322451,N'dgmesnie_geojson:se:8669cb09414bb5045a20',3,0,N'Sitio explícito de adecuación en 115 kV.'),
(239,N'SE Las Delicias',21.2474813,-100.6125144,N'dgmesnie_geojson:se:91d84f12968e347e2370',4,0,N'Extremo explícito del entronque Montenegro.'),
(239,N'SE Querétaro I',20.6428813,-100.4382466,N'dgmesnie_geojson:se:a2e8aae20012fe46f1cf',5,0,N'Extremo explícito del entronque Montenegro.'),
(239,N'SE Querétaro Maniobras',20.5700207,-100.4023757,N'dgmesnie_geojson:se:6c8abcaf6484e9ea4429',6,0,N'Extremo explícito de la LT Tejeda-Querétaro Maniobras.'),
(239,N'SE San Ildefonso',20.5712728,-100.1694391,N'dgmesnie_geojson:se:0695e9bec32210ee27ca',7,0,N'Sitio explícito; se conserva la corrección ortográfica del inventario.'),
(239,N'SE San José Iturbide',21.1055206,-100.4572223,N'dgmesnie_geojson:se:ae1eba8bda5fd1ac38fc',8,0,N'Sitio explícito de adecuación.'),
(239,N'SE Los Nogales',21.0616840,-100.4700053,N'dgmesnie_geojson:se:70615a807c4cb14fa41c',9,0,N'Extremo explícito San José Iturbide-Los Nogales; otros homónimos fueron descartados.'),
(273,N'SE El Rosario',23.0050009,-105.8718664,N'dgmesnie_geojson:se:41145eef913d592c656f',1,1,N'CFE confirma ampliación SE Rosario y LT Santa María-Rosario en Rosario, Sinaloa; Santa María queda pendiente.');

BEGIN TRY
 BEGIN TRANSACTION;

 IF (SELECT COUNT(*) FROM @Revisado)<>24 OR (SELECT COUNT(*) FROM @Esperado)<>15 OR (SELECT COUNT(*) FROM @P)<>50
  THROW 516501,N'Preflight: el lote no conserva 24 expedientes, 15 PEM promovibles y 50 componentes.',1;

 IF EXISTS(
   SELECT 1 FROM @Revisado r
   LEFT JOIN dgmesnie.vw_PAMProyectoVigente v
     ON v.ProyectoId=r.ProyectoId AND v.ClaveProyecto=r.PEM AND v.EstadoVigenciaCartera=N'Vigente'
   WHERE v.ProyectoId IS NULL
 ) THROW 516502,N'Preflight: cambió la identidad o vigencia de uno de los veinticuatro expedientes.',1;

 IF NOT EXISTS(SELECT 1 FROM dgmesnie.PAMProyectoClaveVersion WHERE ProyectoId=272 AND ClaveProyecto=N'CFE25-ZMD' AND EsVigente=1)
    OR NOT EXISTS(SELECT 1 FROM dgmesnie.PAMProyectoClaveVersion WHERE ProyectoId=273 AND ClaveProyecto=N'CFE25-STM' AND EsVigente=1)
  THROW 516503,N'Preflight: cambió una clave alterna de los expedientes sin PEM principal.',1;

 IF EXISTS(
   SELECT 1 FROM dgmesnie.PAMProyectoUbicacion u WITH(UPDLOCK,HOLDLOCK)
   JOIN @Esperado e ON e.ProyectoId=u.ProyectoId
   WHERE u.Activa=1
 ) THROW 516504,N'Preflight: uno de los quince proyectos promovibles ya tiene ubicaciones activas.',1;

 IF EXISTS(
   SELECT 1 FROM @P p WHERE NOT EXISTS(
     SELECT 1 FROM dgmesnie.RedElectricaSubestacionInventario i
     WHERE i.RegistroClave=p.RegistroClave AND i.Activa=1
   )
 ) THROW 516505,N'Preflight: cambió o desapareció un nodo del inventario conciliado.',1;

 IF EXISTS(
   SELECT 1 FROM @P p
   JOIN dgmesnie.RedElectricaSubestacionInventario i ON i.RegistroClave=p.RegistroClave AND i.Activa=1
   WHERE ABS(i.Latitud-p.Lat)>0.0000001 OR ABS(i.Longitud-p.Lon)>0.0000001
 ) THROW 516506,N'Preflight: cambió la coordenada de un nodo conciliado.',1;

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

 IF (SELECT COUNT(*) FROM @I)<>50
  THROW 516507,N'Aplicación: no se insertaron exactamente 50 componentes.',1;

 IF EXISTS(
   SELECT 1 FROM @Esperado e
   LEFT JOIN (SELECT ProyectoId,COUNT(*) Cantidad FROM @I GROUP BY ProyectoId) x ON x.ProyectoId=e.ProyectoId
     WHERE ISNULL(x.Cantidad,0)<>e.Cantidad
 ) THROW 516508,N'Aplicación: el conteo por proyecto no coincide con el expediente.',1;

 IF EXISTS(
   SELECT 1 FROM @Esperado e
   OUTER APPLY(
     SELECT COUNT(*) Principales FROM dgmesnie.PAMProyectoUbicacion u
     WHERE u.ProyectoId=e.ProyectoId AND u.Activa=1 AND u.EsPrincipal=1
   ) x WHERE x.Principales<>1
 ) THROW 516509,N'Aplicación: cada proyecto debe tener exactamente una ubicación principal.',1;

 COMMIT TRANSACTION;
 SELECT ProyectoId,COUNT(*) UbicacionesInsertadas FROM @I GROUP BY ProyectoId ORDER BY ProyectoId;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
