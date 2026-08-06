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
DECLARE @Fuente nvarchar(1000)=N'CENACE/SENER: PRODESEN y diagramas unifilares oficiales; coordenadas conciliadas con inventario territorial DGMESNIE/Atlas SEN';

DECLARE @Esperado TABLE(ProyectoId bigint PRIMARY KEY, PEM nvarchar(30), Cantidad int, Lote nvarchar(80));
INSERT @Esperado VALUES
(227,N'I23-NT1',5,N'PAM-UBICACION-INDIVIDUAL-20260803-012'),
(137,N'P20-OC3',5,N'PAM-UBICACION-INDIVIDUAL-20260803-013'),
(206,N'P22-OC1',4,N'PAM-UBICACION-INDIVIDUAL-20260803-015'),
(223,N'P23-OC1',2,N'PAM-UBICACION-INDIVIDUAL-20260803-016'),
(143,N'P20-NO7',8,N'PAM-UBICACION-INDIVIDUAL-20260803-017'),
(114,N'P19-NO2',5,N'PAM-UBICACION-INDIVIDUAL-20260803-018'),
(184,N'P21-OC8',3,N'PAM-UBICACION-INDIVIDUAL-20260803-019'),
(182,N'P21-OC4',6,N'PAM-UBICACION-INDIVIDUAL-20260803-020');

DECLARE @P TABLE(
 ProyectoId bigint, Etiqueta nvarchar(300), Lat decimal(10,7), Lon decimal(11,7),
 RegistroClave nvarchar(200), Orden int, EsPrincipal bit, Nota nvarchar(500)
);
INSERT @P VALUES
(227,N'SE El Encino',28.4436896,-105.9141626,N'dgmesnie_geojson:se:2e42e75942b5099ecb8a',1,1,N'Componente explícito; no representa la traza de la LT.'),
(227,N'SE Hércules Potencia',28.0480158,-103.8317770,N'atlas_sen:83ad17e37578285266a86429d25567166b6a0c32',2,0,N'Componente explícito 400 kV.'),
(227,N'SE Río Escondido',28.4839207,-100.6925238,N'atlas_sen:ce7ff75e9a2f9ba016e8ab2a6665d7f2a06e86f0',3,0,N'Componente explícito 400 kV; tensión resuelta con PRODESEN.'),
(227,N'SE Torreón Sur',25.4548376,-103.3087805,N'atlas_sen:ca0373f603768ae0feb1eadba0611988c1d55238',4,0,N'Componente explícito 400 kV; tensión resuelta con PRODESEN.'),
(227,N'SE Derramadero',25.2862306,-101.1099750,N'atlas_sen:4e45d4910ce8ea5a24844e50bc58b97df58799f6',5,0,N'Componente explícito; coordenada oficial CFE ya conciliada en caso 011.'),

(137,N'SE Guadalajara II',20.5606209,-103.2869036,N'dgmesnie_geojson:se:baabaff2dc0a6c9a86f6',1,1,N'Sitio del nuevo banco 230/69 kV.'),
(137,N'SE Parque Industrial',20.5699137,-103.3122977,N'dgmesnie_geojson:se:769e2f00e103399dea60',2,0,N'Extremo explícito de LT 69 kV.'),
(137,N'SE El Salto Jalisco',20.5300185,-103.2528530,N'dgmesnie_geojson:se:d29651aeeb9ed4d24f61',3,0,N'Extremo explícito; homónimo resuelto por zona Guadalajara.'),
(137,N'SE Castillo',20.4954633,-103.2468245,N'dgmesnie_geojson:se:8a822323828541e8b6ea',4,0,N'Extremo explícito en corredor Guadalajara.'),
(137,N'SE Atequiza',20.4766302,-103.2343052,N'dgmesnie_geojson:se:24e5fbe18338d5631ada',5,0,N'Sitio explícito de adecuación; no se infiere una línea.'),

(206,N'SE Nuevo Vallarta',20.7614565,-105.2886756,N'atlas_sen:a3bfaa88777394cd258a70c22a5bf3fe61bf9d7c',1,1,N'Sitio del nuevo banco 230/115 kV.'),
(206,N'SE Vallarta Potencia',20.7225478,-105.1956747,N'atlas_sen:0d3920bd47a2bd22804669310dc604558559337a',2,0,N'Componente explícito.'),
(206,N'SE Vallarta I',20.6295950,-105.2260040,N'dgmesnie_geojson:se:09b52504401dab4e6e8a',3,0,N'Componente explícito; denominado Vallarta Uno en la cartera.'),
(206,N'SE Compostela',21.2491052,-104.9031423,N'dgmesnie_geojson:se:8ba85d7070c3e9412be0',4,0,N'Extremo explícito de enlace operativo.'),

(223,N'SE Manzanillo',19.0270254,-104.3173738,N'atlas_sen:8a4c3e86c1c71fda350bf04d3afad990249d3f16',1,1,N'Sitio explícito del reactor 400 kV.'),
(223,N'SE Tapeixtles Potencia',19.0533992,-104.2323324,N'atlas_sen:946345fc9af7047ed591d1fd100756eb8be677e0',2,0,N'Sitio explícito del reactor 400 kV.'),

(143,N'SE Hermosillo Cuatro',29.0806509,-111.0247889,N'atlas_sen:27f9a85463fff0a21a1afbcaa3358785fe81f158',1,1,N'Extremo explícito; no representa el cable.'),
(143,N'SE Ladrilleras',29.1456100,-111.0042076,N'dgmesnie_geojson:se:95c1d20b4ec7ef1ce7da',2,0,N'Extremo explícito.'),
(143,N'SE Ciudad Obregón Tres',27.5238948,-109.9251703,N'atlas_sen:cbc7a818a31187439466a3a4fa94d24c2faf023c',3,0,N'Extremo explícito.'),
(143,N'SE Los Mochis Tres',25.8178700,-108.9945100,N'openstreetmap:5a5f09374a65e43f408d9523b838b8fb34738e5e',4,0,N'Nombre confirmado por diagrama CENACE; coordenada fuente abierta.'),
(143,N'SE Culiacán Milenium',24.7983689,-107.4368517,N'dgmesnie_geojson:se:5e478f5cad753a54b12d',5,0,N'Extremo explícito; inventario rotula Milenium.'),
(143,N'SE La Higuera',24.6978911,-107.5072755,N'atlas_sen:e2ae53b03a72ad8e185f9faec911d1069ea41034',6,0,N'Extremo explícito.'),
(143,N'SE Tres Ríos',24.8221551,-107.4056996,N'dgmesnie_geojson:se:1068b9b299aee7944ca9',7,0,N'Extremo explícito.'),
(143,N'SE Mazatlán Centro',23.2023250,-106.4138876,N'dgmesnie_geojson:se:1168833597e76a16bddd',8,0,N'Extremo explícito.'),

(114,N'SE Hermosillo Loma',29.2012889,-111.0045410,N'atlas_sen:8cbd3f300c0c9c45bea725976d180fe20a46afa7',1,1,N'Nodo explícito representativo de Zona Hermosillo.'),
(114,N'SE Bácum',27.5165381,-110.0738967,N'atlas_sen:527b8ffe3927eb67ed696e1396e645b8a4ed2c1a',2,0,N'Nodo explícito representativo de Zona Obregón.'),
(114,N'SE Louisiana',25.7848299,-109.0762864,N'atlas_sen:11ac9ba6b5ec4a0452f1c2f0c54b57840b4ed2b4',3,0,N'Nodo explícito representativo de Zona Los Mochis.'),
(114,N'SE Culiacán Tres',24.8299246,-107.3616339,N'atlas_sen:812896b1867656d0483a6728f179c47ab826dd39',4,0,N'Nodo explícito representativo de Zona Culiacán.'),
(114,N'SE Mazatlán Dos',23.1907241,-106.3530586,N'atlas_sen:5008891c4e7ec9b0068056ae2177d37a94950669',5,0,N'Nodo explícito representativo de Zona Mazatlán.'),

(184,N'SE Cerro Blanco',21.3942643,-104.6024362,N'dgmesnie_geojson:se:bc070391e92bd88e6c93',1,1,N'Extremo existente explícito; SE Vallejo permanece pendiente.'),
(184,N'SE Vallarta Potencia',20.7225478,-105.1956747,N'atlas_sen:0d3920bd47a2bd22804669310dc604558559337a',2,0,N'Extremo existente del entronque documentado.'),
(184,N'SE Nuevo Vallarta',20.7614565,-105.2886756,N'atlas_sen:a3bfaa88777394cd258a70c22a5bf3fe61bf9d7c',3,0,N'Extremo existente del entronque documentado.'),

(182,N'SE Querétaro Potencia',20.5204727,-100.4871176,N'atlas_sen:73727232566dba7f59bedc91fcfb3c8521b1ed2c',1,1,N'Extremo existente explícito; SE Otomí permanece pendiente.'),
(182,N'SE Las Delicias',21.2474813,-100.6125144,N'dgmesnie_geojson:se:91d84f12968e347e2370',2,0,N'Extremo existente explícito.'),
(182,N'SE La Loma',20.6501100,-100.4781400,N'openstreetmap:075b23031261a664220b3855d0b73c54f1936e98',3,0,N'Extremo existente explícito; homónimo resuelto por zona Querétaro.'),
(182,N'SE Querétaro Poniente',20.5921302,-100.4582169,N'dgmesnie_geojson:se:af89dd027ac9e447a5f1',4,0,N'Extremo existente explícito.'),
(182,N'SE Querétaro Industrial',20.5972675,-100.4111104,N'dgmesnie_geojson:se:5488d3b6d1ffbd88c419',5,0,N'Componente existente explícito.'),
(182,N'SE Querétaro Maniobras',20.5700207,-100.4023757,N'dgmesnie_geojson:se:6c8abcaf6484e9ea4429',6,0,N'Componente existente explícito.');

BEGIN TRY
 BEGIN TRANSACTION;

 IF (SELECT COUNT(*) FROM @P)<>38
  THROW 512101,N'Preflight: el lote no contiene exactamente 38 componentes.',1;

 IF EXISTS(
   SELECT 1 FROM @Esperado e
   LEFT JOIN dgmesnie.vw_PAMProyectoVigente v
     ON v.ProyectoId=e.ProyectoId AND v.ClaveProyecto=e.PEM AND v.EstadoVigenciaCartera=N'Vigente'
   WHERE v.ProyectoId IS NULL
 ) THROW 512102,N'Preflight: cambió la identidad o vigencia de uno de los ocho PEM.',1;

 IF EXISTS(
   SELECT 1 FROM dgmesnie.PAMProyectoUbicacion u WITH(UPDLOCK,HOLDLOCK)
   JOIN @Esperado e ON e.ProyectoId=u.ProyectoId
   WHERE u.Activa=1
 ) THROW 512103,N'Preflight: uno de los ocho PEM ya tiene ubicaciones activas.',1;

 IF EXISTS(
   SELECT 1 FROM @P p
   WHERE NOT EXISTS(
     SELECT 1 FROM dgmesnie.RedElectricaSubestacionInventario i
     WHERE i.RegistroClave=p.RegistroClave AND i.Activa=1
   )
 ) THROW 512104,N'Preflight: cambió o desapareció un nodo del inventario conciliado.',1;

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

 IF (SELECT COUNT(*) FROM @I)<>38
  THROW 512105,N'Aplicación: no se insertaron exactamente 38 componentes.',1;

 IF EXISTS(
   SELECT 1 FROM @Esperado e
   LEFT JOIN (SELECT ProyectoId,COUNT(*) Cantidad FROM @I GROUP BY ProyectoId) x ON x.ProyectoId=e.ProyectoId
   WHERE ISNULL(x.Cantidad,0)<>e.Cantidad
 ) THROW 512106,N'Aplicación: el conteo por PEM no coincide con el expediente.',1;

 COMMIT TRANSACTION;
 SELECT ProyectoId,COUNT(*) UbicacionesInsertadas FROM @I GROUP BY ProyectoId ORDER BY ProyectoId;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
