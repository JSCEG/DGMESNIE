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
DECLARE @Fuente nvarchar(1000)=N'CENACE/SENER/CFE: PRODESEN, PAMRNT, diagramas unifilares y documentación contractual; coordenadas conciliadas con inventario DGMESNIE/Atlas SEN';

DECLARE @Revisado TABLE(ProyectoId bigint PRIMARY KEY, PEM nvarchar(30));
INSERT @Revisado VALUES
(306,N'M01-GCRN'),(101,N'P19-NT1'),(230,N'P23-BC1'),(253,N'P24-OR2'),(262,N'P25-NE1'),
(293,N'CFE25-FPP'),(294,N'CFE25-PHC'),(269,N'CFE25-SLM'),(89,N'D18-BC3'),(80,N'D18-BC4');

DECLARE @Esperado TABLE(ProyectoId bigint PRIMARY KEY, PEM nvarchar(30), Cantidad int, Lote nvarchar(80));
INSERT @Esperado VALUES
(101,N'P19-NT1',1,N'PAM-UBICACION-INDIVIDUAL-20260803-053'),
(230,N'P23-BC1',2,N'PAM-UBICACION-INDIVIDUAL-20260803-054'),
(293,N'CFE25-FPP',1,N'PAM-UBICACION-INDIVIDUAL-20260803-057'),
(269,N'CFE25-SLM',8,N'PAM-UBICACION-INDIVIDUAL-20260803-059'),
(89,N'D18-BC3',1,N'PAM-UBICACION-INDIVIDUAL-20260803-060'),
(80,N'D18-BC4',2,N'PAM-UBICACION-INDIVIDUAL-20260803-061');

DECLARE @P TABLE(
 ProyectoId bigint, Etiqueta nvarchar(300), Lat decimal(10,7), Lon decimal(11,7),
 RegistroClave nvarchar(200), Orden int, EsPrincipal bit, Nota nvarchar(500)
);
INSERT @P VALUES
(101,N'SE Terranova 230 kV',31.5548139,-106.4061325,N'atlas_sen:c2afa96f5a728c4c0095c91252e0b6a9dfc78c78',1,1,N'Sitio exacto del Banco 2; no representa un segundo emplazamiento.'),
(230,N'SE Cerro Prieto II',32.3919746,-115.2254666,N'dgmesnie_geojson:se:1a794fff861ce1fb8527',1,1,N'Extremo existente explícito del anillo 230 kV; no representa Victoria Potencia.'),
(230,N'SE Chapultepec 230 kV',32.3619798,-115.0606601,N'dgmesnie_geojson:se:105250ff21f8f7ed9a6a',2,0,N'Extremo existente explícito; Victoria Potencia y la traza permanecen pendientes.'),
(293,N'SE Puerto Peñasco 230 kV',31.3113014,-113.5207574,N'dgmesnie_geojson:se:8f64ae18643bcfb7a401',1,1,N'Punto de interconexión identificado en documentación contractual CFE; no representa la central.'),
(269,N'SE Salamanca',20.5713389,-101.1692227,N'atlas_sen:1c0b3513b91e07cb36b7cf2f777310c5c3c36290',1,1,N'Instalación intervenida explícitamente en el expediente vigente.'),
(269,N'SE Salamanca Cogeneración',20.5798651,-101.1625797,N'atlas_sen:21f12b361690507838004167205e0dbb090fd876',2,0,N'Instalación intervenida explícitamente en el expediente vigente.'),
(269,N'SE Salamanca Norte',20.5947958,-101.1807786,N'dgmesnie_geojson:se:a6c71d9b2a3e5202a347',3,0,N'Instalación intervenida explícitamente en el expediente vigente.'),
(269,N'SE Salamanca Sur',20.5432548,-101.1916785,N'dgmesnie_geojson:se:ade018b864b61618e82e',4,0,N'Instalación intervenida explícitamente en el expediente vigente.'),
(269,N'SE Salamanca II',20.5317631,-101.2298243,N'dgmesnie_geojson:se:95867a76a8e2d81bcbc8',5,0,N'Instalación intervenida explícitamente en el expediente vigente.'),
(269,N'SE Irapuato II',20.6994321,-101.2982347,N'dgmesnie_geojson:se:e4d563d577514d77834b',6,0,N'Instalación intervenida explícitamente en el expediente vigente.'),
(269,N'SE Querétaro Potencia',20.5204727,-100.4871176,N'dgmesnie_geojson:se:43639d8f0dda9720f99a',7,0,N'Instalación intervenida explícitamente en el expediente vigente.'),
(269,N'SE Villa de Reyes',21.8321832,-100.9317222,N'dgmesnie_geojson:se:d234fea988581060421f',8,0,N'Extremo existente explícito de la LT a Maniobras WTC II; la traza y WTC II quedan pendientes.'),
(89,N'SE Chapultepec 230 kV',32.3619798,-115.0606601,N'dgmesnie_geojson:se:105250ff21f8f7ed9a6a',1,1,N'Extremo existente explícito; Victoria Potencia y la traza permanecen pendientes.'),
(80,N'SE Metrópoli Potencia',32.4451030,-116.8782506,N'dgmesnie_geojson:se:9bec5a2b0da198be9e11',1,1,N'Extremo existente explícito del entronque; no representa la nueva SE La Encantada.'),
(80,N'SE Tijuana I',32.5154594,-116.8834640,N'dgmesnie_geojson:se:4a1ed6f83da2d5803bef',2,0,N'Extremo Tijuana identificado en el diagrama oficial; La Encantada y la traza quedan pendientes.');

BEGIN TRY
 BEGIN TRANSACTION;

 IF (SELECT COUNT(*) FROM @Revisado)<>10 OR (SELECT COUNT(*) FROM @P)<>15
  THROW 516101,N'Preflight: el lote no conserva 10 expedientes y 15 componentes.',1;

 IF EXISTS(
   SELECT 1 FROM @Revisado r
   LEFT JOIN dgmesnie.vw_PAMProyectoVigente v
     ON v.ProyectoId=r.ProyectoId AND v.ClaveProyecto=r.PEM AND v.EstadoVigenciaCartera=N'Vigente'
   WHERE v.ProyectoId IS NULL
 ) THROW 516102,N'Preflight: cambió la identidad o vigencia de uno de los diez PEM revisados.',1;

 IF EXISTS(
   SELECT 1 FROM dgmesnie.PAMProyectoUbicacion u WITH(UPDLOCK,HOLDLOCK)
   JOIN @Esperado e ON e.ProyectoId=u.ProyectoId
   WHERE u.Activa=1
 ) THROW 516103,N'Preflight: uno de los seis PEM promovibles ya tiene ubicaciones activas.',1;

 IF EXISTS(
   SELECT 1 FROM @P p
   WHERE NOT EXISTS(
     SELECT 1 FROM dgmesnie.RedElectricaSubestacionInventario i
     WHERE i.RegistroClave=p.RegistroClave AND i.Activa=1
   )
 ) THROW 516104,N'Preflight: cambió o desapareció un nodo del inventario conciliado.',1;

 IF EXISTS(
   SELECT 1 FROM @P p
   JOIN dgmesnie.RedElectricaSubestacionInventario i
     ON i.RegistroClave=p.RegistroClave AND i.Activa=1
   WHERE ABS(i.Latitud-p.Lat)>0.0000001 OR ABS(i.Longitud-p.Lon)>0.0000001
 ) THROW 516105,N'Preflight: cambió la coordenada de un nodo conciliado.',1;

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

 IF (SELECT COUNT(*) FROM @I)<>15
  THROW 516106,N'Aplicación: no se insertaron exactamente 15 componentes.',1;

 IF EXISTS(
   SELECT 1 FROM @Esperado e
   LEFT JOIN (SELECT ProyectoId,COUNT(*) Cantidad FROM @I GROUP BY ProyectoId) x ON x.ProyectoId=e.ProyectoId
   WHERE ISNULL(x.Cantidad,0)<>e.Cantidad
 ) THROW 516107,N'Aplicación: el conteo por PEM no coincide con el expediente.',1;

 IF EXISTS(
   SELECT 1 FROM @Esperado e
   OUTER APPLY(
     SELECT COUNT(*) Principales FROM dgmesnie.PAMProyectoUbicacion u
     WHERE u.ProyectoId=e.ProyectoId AND u.Activa=1 AND u.EsPrincipal=1
   ) x WHERE x.Principales<>1
 ) THROW 516108,N'Aplicación: cada PEM debe tener exactamente una ubicación principal.',1;

 COMMIT TRANSACTION;
 SELECT ProyectoId,COUNT(*) UbicacionesInsertadas FROM @I GROUP BY ProyectoId ORDER BY ProyectoId;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
