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
DECLARE @Fuente nvarchar(1000)=N'CENACE/SENER: PRODESEN y diagramas unifilares; coordenadas conciliadas con inventario territorial DGMESNIE/Atlas SEN/OpenStreetMap';

DECLARE @Esperado TABLE(ProyectoId bigint PRIMARY KEY, PEM nvarchar(30), Cantidad int, Lote nvarchar(80));
INSERT @Esperado VALUES
(162,N'M20-NE1',31,N'PAM-UBICACION-INDIVIDUAL-20260803-042'),
(77,N'D18-OR6',2,N'PAM-UBICACION-INDIVIDUAL-20260803-043'),
(84,N'D18-NO1',2,N'PAM-UBICACION-INDIVIDUAL-20260803-044'),
(94,N'D18-NT6',1,N'PAM-UBICACION-INDIVIDUAL-20260803-045'),
(93,N'D18-NT7',1,N'PAM-UBICACION-INDIVIDUAL-20260803-046'),
(85,N'D18-OR10',1,N'PAM-UBICACION-INDIVIDUAL-20260803-047'),
(87,N'D18-OR12',2,N'PAM-UBICACION-INDIVIDUAL-20260803-048'),
(128,N'D19-NE2',2,N'PAM-UBICACION-INDIVIDUAL-20260803-049'),
(119,N'D19-OC1',2,N'PAM-UBICACION-INDIVIDUAL-20260803-050'),
(121,N'D19-OC4',2,N'PAM-UBICACION-INDIVIDUAL-20260803-051');

DECLARE @P TABLE(
 ProyectoId bigint, Etiqueta nvarchar(300), Lat decimal(10,7), Lon decimal(11,7),
 RegistroClave nvarchar(200), Orden int, EsPrincipal bit, Nota nvarchar(500)
);
INSERT @P VALUES
(162,N'SE Solidaridad',25.7850725,-100.3933983,N'dgmesnie_geojson:se:99ca642a91c17c40964f',1,1,N'Sitio 87B explícito y territorialmente conciliado; el PEM es multisede.'),
(162,N'SE Modelo',25.7410486,-100.3806148,N'dgmesnie_geojson:se:541faf18a64a343a0dd3',2,0,N'Sitio 87B explícito y territorialmente conciliado.'),
(162,N'SE Prolec',25.7639194,-100.2092987,N'dgmesnie_geojson:se:523248ccaa7feba7f548',3,0,N'Sitio 87B explícito y territorialmente conciliado.'),
(162,N'SE Acero',26.9122195,-101.4716917,N'dgmesnie_geojson:se:c5c51586d60f8c140850',4,0,N'Sitio 87B explícito en Monclova, Coahuila.'),
(162,N'SE Piedras Negras',28.7019435,-100.5449030,N'dgmesnie_geojson:se:ee029cb129784edec197',5,0,N'Sitio 87B explícito en Coahuila; homónimo de Veracruz descartado.'),
(162,N'SE Anzaldúas',26.0937095,-98.2889644,N'dgmesnie_geojson:se:ac1d4149e894b8bdb669',6,0,N'Sitio 87B explícito y territorialmente conciliado.'),
(162,N'SE Jarachina',26.0495987,-98.3550650,N'dgmesnie_geojson:se:3c11bdba4f9c3d065f88',7,0,N'Sitio 87B explícito y territorialmente conciliado.'),
(162,N'SE Nuevo Escobedo',25.8475900,-100.3700100,N'openstreetmap:d9f25a81c3edd548590514a4a1e9601b0f18bcb1',8,0,N'Sitio 87B explícito; coordenada abierta conciliada con el ámbito Monterrey.'),
(162,N'SE Girasoles',25.7966200,-100.3286300,N'openstreetmap:d89c2f00ac7151d9a0925915eddac22f04e6153f',9,0,N'Sitio 87B explícito; coordenada abierta conciliada con el ámbito Monterrey.'),
(162,N'SE Josefa Zozaya',25.7357400,-100.1622200,N'openstreetmap:3f4ec7cd08ea5b06776d312e321c39340c6a09c6',10,0,N'Sitio 87B explícito; coordenada abierta conciliada con el ámbito Monterrey.'),
(162,N'SE México',26.0066195,-98.2761710,N'dgmesnie_geojson:se:78835178e55e4ee618ae',11,0,N'Sitio 87B explícito en Tamaulipas; homónimos fuera de Noreste descartados.'),
(162,N'SE Orizatlán',26.0376658,-98.2187120,N'dgmesnie_geojson:se:3be6657c0a6ac3718c10',12,0,N'Sitio 87B explícito y territorialmente conciliado.'),
(162,N'SE Topochico',25.7390035,-100.3243818,N'dgmesnie_geojson:se:f217976f0804b27058c5',13,0,N'Sitio 87B explícito y territorialmente conciliado.'),
(162,N'SE Félix U. Gómez',25.6723005,-100.2978223,N'dgmesnie_geojson:se:0809f104ed0290124e71',14,0,N'Sitio 87B explícito y territorialmente conciliado.'),
(162,N'SE Mezquital',25.8118500,-100.2238900,N'openstreetmap:27b16b2382eb156c05ad86ad183205a7159ec0e8',15,0,N'Sitio 87B explícito en Monterrey; homónimo de Mulegé descartado.'),
(162,N'SE Finsa Guadalupe',25.6938000,-100.1331600,N'openstreetmap:69b2aa31a61672501b3a78e48ddeeabf8eaa2538',16,0,N'Sitio 87B explícito; coordenada abierta conciliada con Guadalupe, Nuevo León.'),
(162,N'SE Parque Industrial',26.0162402,-98.2179639,N'dgmesnie_geojson:se:04be5fc8221623e7e4b4',17,0,N'Sitio 87B explícito en Reynosa; homónimo de Guadalajara descartado.'),
(162,N'SE Petrolera',26.0666689,-98.2654644,N'dgmesnie_geojson:se:7e3ac48cb94c7ed5f699',18,0,N'Sitio 87B explícito y territorialmente conciliado.'),
(162,N'SE Allende',25.2776928,-100.0233095,N'dgmesnie_geojson:se:c2baffecbda912dc4ce9',19,0,N'Sitio 87B explícito en Nuevo León; homónimos descartados.'),
(162,N'SE Linares',24.8445808,-99.5736015,N'dgmesnie_geojson:se:c9e2cc0a115d90688c0a',20,0,N'Sitio 87B explícito y territorialmente conciliado.'),
(162,N'SE Agua Nueva',25.2174782,-101.0922352,N'dgmesnie_geojson:se:08d1e7b7c42ac1b0c83d',21,0,N'Sitio 87B explícito en Coahuila.'),
(162,N'SE Puente Internacional',28.6850053,-100.5249778,N'dgmesnie_geojson:se:fe3e7d4691d1f7daaae1',22,0,N'Sitio 87B explícito y territorialmente conciliado.'),
(162,N'SE Parque Industrial Colonial',25.9968927,-98.1897268,N'dgmesnie_geojson:se:774e11b2fdae6487b005',23,0,N'Sitio 87B explícito y territorialmente conciliado.'),
(162,N'SE Montemorelos',25.1847062,-99.8411252,N'dgmesnie_geojson:se:ce46aed13767d7f988f5',24,0,N'Sitio 87B explícito y territorialmente conciliado.'),
(162,N'SE Villa de Santiago',25.4295608,-100.1486318,N'dgmesnie_geojson:se:fbfc652baaedff4eb3ad',25,0,N'Sitio 87B explícito y territorialmente conciliado.'),
(162,N'SE Mante',22.7370542,-98.9591729,N'dgmesnie_geojson:se:35d81e46a2f5f7dee669',26,0,N'Sitio 87B explícito en Tamaulipas.'),
(162,N'SE Ramos Arizpe',25.5491087,-100.9394381,N'dgmesnie_geojson:se:ad22b0ba2df2a9cfeda2',27,0,N'Sitio 87B explícito en Coahuila.'),
(162,N'SE Zapaliname',25.4236500,-100.9715079,N'dgmesnie_geojson:se:c2232139b86fa0ff2d6b',28,0,N'Sitio 87B explícito en Coahuila.'),
(162,N'SE Ojo Caliente',27.4836461,-99.5032234,N'dgmesnie_geojson:se:3096a7c6adb34d8c10e2',29,0,N'Sitio 87B explícito en el corredor fronterizo; homónimo de Zacatecas descartado.'),
(162,N'SE Ricsa',26.0470203,-98.2313047,N'dgmesnie_geojson:se:d95a3d33de57eff06e87',30,0,N'Sitio 87B explícito y territorialmente conciliado.'),
(162,N'SE Villa Florida',26.0758430,-98.3627629,N'dgmesnie_geojson:se:7e4fbcdc7f1f6ce3f7cd',31,0,N'Sitio 87B explícito y territorialmente conciliado; 25 sitios permanecen pendientes.'),
(77,N'SE Ocozocoautla',16.7306575,-93.3945373,N'dgmesnie_geojson:se:8046588dc1950f16e969',1,1,N'Extremo existente explícito; no representa la nueva SE Berriozábal ni la traza.'),
(77,N'Complejo SE El Sabino',16.8114068,-93.1910748,N'dgmesnie_geojson:se:08f8415ee108dccd06a5',2,0,N'Ubicación del complejo existente El Sabino; no representa la nueva SE ni la traza 115 kV.'),
(84,N'SE Santa Ana',30.5252114,-111.1315095,N'dgmesnie_geojson:se:4eb9ace0666e3bb0a689',1,1,N'Extremo existente explícito del entronque; no representa la nueva SE El Llano.'),
(84,N'SE Oasis, Sonora',29.6887941,-111.0425419,N'dgmesnie_geojson:se:8896b7457160da98967a',2,0,N'Extremo existente explícito; homónimo de Chihuahua descartado.'),
(94,N'SE Ascensión',31.0708457,-108.0106014,N'dgmesnie_geojson:se:c28e88ad75528fddc26d',1,1,N'Extremo existente explícito; Buenavista y la traza permanecen pendientes.'),
(93,N'SE Galeana',30.1734362,-107.6240514,N'dgmesnie_geojson:se:edda96fbc6c176642c8f',1,1,N'Extremo existente explícito; Lebarón y la traza permanecen pendientes.'),
(85,N'SE Macuspana II',17.8327296,-92.6079737,N'dgmesnie_geojson:se:8616cc3ea3d675e6bc83',1,1,N'Nodo existente inequívoco del corredor; Traconis, Kilómetro Veinte y la traza permanecen pendientes.'),
(87,N'SE Cactus',17.9006863,-93.1961345,N'dgmesnie_geojson:se:cea354f972b8aed4aa0e',1,1,N'Extremo existente explícito; no representa la nueva SE Luis Gil Pérez.'),
(87,N'SE Tamulté',17.9575502,-92.9725535,N'dgmesnie_geojson:se:e78b7f9b46aa9269f59a',2,0,N'Extremo existente explícito; la traza permanece pendiente.'),
(128,N'SE Cruz de Ataque',20.2704000,-98.3416000,N'openstreetmap:4c248aac029219f51c8aecebc87c0ebbc14049d5',1,1,N'Extremo existente explícito en Hidalgo; no representa la nueva SE San Bartolo.'),
(128,N'SE Ixhuatlán',20.7339630,-98.0108545,N'dgmesnie_geojson:se:f5927b8dbde1821bce76',2,0,N'Extremo existente explícito en Hidalgo; la traza permanece pendiente.'),
(119,N'SE Tabacalera',21.5048470,-104.8827983,N'dgmesnie_geojson:se:aee268c6c4dcb07f68b8',1,1,N'Extremo existente explícito; no representa la nueva SE Jauja.'),
(119,N'SE Tepic Industrial',21.4844584,-104.8425416,N'dgmesnie_geojson:se:1817243474087e8763a3',2,0,N'Extremo existente explícito; Jauja y la traza permanecen pendientes.'),
(121,N'SE Tepatitlán',20.8002757,-102.7884267,N'dgmesnie_geojson:se:158c4452e49a6ccee4a6',1,1,N'Extremo existente explícito; no representa la nueva SE Acatic.'),
(121,N'SE Zapotlanejo 115 kV',20.6168480,-103.0822925,N'dgmesnie_geojson:se:963e6bb3f33a78ad3688',2,0,N'Extremo existente de distribución; Acatic y la traza permanecen pendientes.');

BEGIN TRY
 BEGIN TRANSACTION;

 IF (SELECT COUNT(*) FROM @P)<>46
  THROW 515101,N'Preflight: el lote no contiene exactamente 46 componentes.',1;

 IF EXISTS(
   SELECT 1 FROM @Esperado e
   LEFT JOIN dgmesnie.vw_PAMProyectoVigente v
     ON v.ProyectoId=e.ProyectoId AND v.ClaveProyecto=e.PEM AND v.EstadoVigenciaCartera=N'Vigente'
   WHERE v.ProyectoId IS NULL
 ) THROW 515102,N'Preflight: cambió la identidad o vigencia de uno de los diez PEM.',1;

 IF EXISTS(
   SELECT 1 FROM dgmesnie.PAMProyectoUbicacion u WITH(UPDLOCK,HOLDLOCK)
   JOIN @Esperado e ON e.ProyectoId=u.ProyectoId
   WHERE u.Activa=1
 ) THROW 515103,N'Preflight: uno de los diez PEM ya tiene ubicaciones activas.',1;

 IF EXISTS(
   SELECT 1 FROM @P p
   WHERE NOT EXISTS(
     SELECT 1 FROM dgmesnie.RedElectricaSubestacionInventario i
     WHERE i.RegistroClave=p.RegistroClave AND i.Activa=1
   )
 ) THROW 515104,N'Preflight: cambió o desapareció un nodo del inventario conciliado.',1;

 IF EXISTS(
   SELECT 1 FROM @P p
   JOIN dgmesnie.RedElectricaSubestacionInventario i
     ON i.RegistroClave=p.RegistroClave AND i.Activa=1
   WHERE ABS(i.Latitud-p.Lat)>0.0000001 OR ABS(i.Longitud-p.Lon)>0.0000001
 ) THROW 515105,N'Preflight: cambió la coordenada de un nodo conciliado.',1;

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

 IF (SELECT COUNT(*) FROM @I)<>46
  THROW 515106,N'Aplicación: no se insertaron exactamente 46 componentes.',1;

 IF EXISTS(
   SELECT 1 FROM @Esperado e
   LEFT JOIN (SELECT ProyectoId,COUNT(*) Cantidad FROM @I GROUP BY ProyectoId) x ON x.ProyectoId=e.ProyectoId
   WHERE ISNULL(x.Cantidad,0)<>e.Cantidad
 ) THROW 515107,N'Aplicación: el conteo por PEM no coincide con el expediente.',1;

 IF EXISTS(
   SELECT 1 FROM @Esperado e
   OUTER APPLY(
     SELECT COUNT(*) Principales FROM dgmesnie.PAMProyectoUbicacion u
     WHERE u.ProyectoId=e.ProyectoId AND u.Activa=1 AND u.EsPrincipal=1
   ) x WHERE x.Principales<>1
 ) THROW 515108,N'Aplicación: cada PEM debe tener exactamente una ubicación principal.',1;

 COMMIT TRANSACTION;
 SELECT ProyectoId,COUNT(*) UbicacionesInsertadas FROM @I GROUP BY ProyectoId ORDER BY ProyectoId;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
