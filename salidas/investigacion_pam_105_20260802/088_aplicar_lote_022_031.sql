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
DECLARE @Fuente nvarchar(1000)=N'CENACE/SENER: PRODESEN, PAMRNT y diagramas unifilares oficiales; coordenadas conciliadas con inventario territorial DGMESNIE/Atlas SEN';

DECLARE @Esperado TABLE(ProyectoId bigint PRIMARY KEY, PEM nvarchar(30), Cantidad int, Lote nvarchar(80));
INSERT @Esperado VALUES
(120,N'D19-OC2',1,N'PAM-UBICACION-INDIVIDUAL-20260803-022'),
(138,N'P20-OC4',1,N'PAM-UBICACION-INDIVIDUAL-20260803-023'),
(161,N'M20-NT2',1,N'PAM-UBICACION-INDIVIDUAL-20260803-024'),
(122,N'D19-OC5',1,N'PAM-UBICACION-INDIVIDUAL-20260803-025'),
(126,N'D19-NO4',1,N'PAM-UBICACION-INDIVIDUAL-20260803-026'),
(83,N'D18-OC7',1,N'PAM-UBICACION-INDIVIDUAL-20260803-027'),
(129,N'D19-BC1',1,N'PAM-UBICACION-INDIVIDUAL-20260803-029'),
(75,N'D18-OC5',2,N'PAM-UBICACION-INDIVIDUAL-20260803-030'),
(238,N'P24-OC2',2,N'PAM-UBICACION-INDIVIDUAL-20260803-031');

DECLARE @P TABLE(
 ProyectoId bigint, Etiqueta nvarchar(300), Lat decimal(10,7), Lon decimal(11,7),
 RegistroClave nvarchar(200), Orden int, EsPrincipal bit, Nota nvarchar(500)
);
INSERT @P VALUES
(120,N'SE Vallarta I',20.6295950,-105.2260040,N'dgmesnie_geojson:se:09b52504401dab4e6e8a',1,1,N'Extremo existente explícito; SE Centro, Nogalito y traza permanecen pendientes.'),
(138,N'SE La Pila',22.0314785,-100.8475132,N'atlas_sen:6d71fd2c9664d4ee6e4ecc05f85fc0a5bd10c465',1,1,N'Componente existente explícito 230 kV; Laguna San Vicente II y trazas permanecen pendientes.'),
(161,N'SE Vicente Guerrero',23.7149111,-103.9947516,N'dgmesnie_geojson:se:904e2fc3fa9094977058',1,1,N'Extremo existente explícito 115 kV; no se equipara CM Centauro con Celulósicos Centauro.'),
(122,N'SE Juan Rulfo',19.6804076,-103.7966277,N'dgmesnie_geojson:se:c688e668c5bfbeb0642f',1,1,N'Extremo existente explícito 115 kV; SE Tolimán y traza permanecen pendientes.'),
(126,N'SE San Rafael',25.4969584,-108.3068710,N'dgmesnie_geojson:se:7332d64fb065492ccf21',1,1,N'Extremo existente explícito en Sinaloa; Tamazula Jalisco fue descartada por homonimia.'),
(83,N'SE Jesús del Monte',20.9311445,-101.7150952,N'dgmesnie_geojson:se:872837fd2a79219b9b23',1,1,N'Extremo existente explícito 115 kV en Guanajuato; homónimos San Cristóbal fueron descartados.'),
(129,N'SE Parque Industrial San Luis',32.4360295,-114.7114600,N'atlas_sen:3e2222338d8f74ea6a7c64f3df03d32930f527da',1,1,N'Extremo existente explícito 230 kV en San Luis Río Colorado; Libramiento Culiacán fue descartada.'),
(75,N'SE Tesistán 230',20.7889085,-103.5030726,N'atlas_sen:7fe9c3bf0f1996629b1f8f2cd774c626ac87d48c',1,1,N'Extremo existente explícito 230 kV; no representa la traza.'),
(75,N'SE Niños Héroes',20.6655326,-103.3737011,N'dgmesnie_geojson:se:ca7526ce00466e28b73c',2,0,N'Extremo existente explícito 230 kV; SE Bajío permanece pendiente.'),
(238,N'SE Silao Potencia',20.9065890,-101.4661024,N'atlas_sen:492461ca0cb91b86d0328696536eab4f43f64640',1,1,N'Extremo existente explícito 230 kV; no representa la traza.'),
(238,N'SE León IV',21.1360385,-101.6058468,N'dgmesnie_geojson:se:55e86f7f148cb1307033',2,0,N'Extremo existente explícito 230 kV; SE Puerto Interior permanece pendiente.');

BEGIN TRY
 BEGIN TRANSACTION;

 IF (SELECT COUNT(*) FROM @P)<>11
  THROW 513101,N'Preflight: el lote no contiene exactamente 11 componentes.',1;

 IF EXISTS(
   SELECT 1 FROM @Esperado e
   LEFT JOIN dgmesnie.vw_PAMProyectoVigente v
     ON v.ProyectoId=e.ProyectoId AND v.ClaveProyecto=e.PEM AND v.EstadoVigenciaCartera=N'Vigente'
   WHERE v.ProyectoId IS NULL
 ) THROW 513102,N'Preflight: cambió la identidad o vigencia de uno de los nueve PEM.',1;

 IF EXISTS(
   SELECT 1 FROM dgmesnie.PAMProyectoUbicacion u WITH(UPDLOCK,HOLDLOCK)
   JOIN @Esperado e ON e.ProyectoId=u.ProyectoId
   WHERE u.Activa=1
 ) THROW 513103,N'Preflight: uno de los nueve PEM ya tiene ubicaciones activas.',1;

 IF EXISTS(
   SELECT 1 FROM @P p
   JOIN dgmesnie.RedElectricaSubestacionInventario i
     ON i.RegistroClave=p.RegistroClave AND i.Activa=1
   WHERE ABS(i.Latitud-p.Lat)>0.0000001 OR ABS(i.Longitud-p.Lon)>0.0000001
 ) THROW 513104,N'Preflight: cambió la coordenada de un nodo conciliado.',1;

 IF EXISTS(
   SELECT 1 FROM @P p
   WHERE NOT EXISTS(
     SELECT 1 FROM dgmesnie.RedElectricaSubestacionInventario i
     WHERE i.RegistroClave=p.RegistroClave AND i.Activa=1
   )
 ) THROW 513105,N'Preflight: cambió o desapareció un nodo del inventario conciliado.',1;

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

 IF (SELECT COUNT(*) FROM @I)<>11
  THROW 513106,N'Aplicación: no se insertaron exactamente 11 componentes.',1;

 IF EXISTS(
   SELECT 1 FROM @Esperado e
   LEFT JOIN (SELECT ProyectoId,COUNT(*) Cantidad FROM @I GROUP BY ProyectoId) x ON x.ProyectoId=e.ProyectoId
   WHERE ISNULL(x.Cantidad,0)<>e.Cantidad
 ) THROW 513107,N'Aplicación: el conteo por PEM no coincide con el expediente.',1;

 COMMIT TRANSACTION;
 SELECT ProyectoId,COUNT(*) UbicacionesInsertadas FROM @I GROUP BY ProyectoId ORDER BY ProyectoId;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
