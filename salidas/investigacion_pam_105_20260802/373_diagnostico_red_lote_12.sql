SET NOCOUNT ON;

DECLARE @Patrones TABLE (Patron NVARCHAR(200));
INSERT @Patrones (Patron) VALUES
    (N'ANGOSTURA'), (N'MANUEL MORENO'), (N'SAN CRISTOBAL'),
    (N'OXCHUC'), (N'TRINITARIA'), (N'JUCHITAN'),
    (N'TECNOLOGICO'), (N'TRANSISTMICA'), (N'TAPANATEPEC'),
    (N'CHARCAS'), (N'MATEHUALA'), (N'PUEBLO NUEVO'),
    (N'EL MAYO'), (N'PARQUE INDUSTRIAL BAJIO'),
    (N'HERRADURA'), (N'METROPOLI'), (N'TIJUANA'),
    (N'FLORIDO'), (N'FRANCISCO VILLA'), (N'TECATE'),
    (N'ENCINAL');

SELECT
    N'INVENTARIO' AS Origen,
    i.RegistroClave,
    i.Nombre,
    i.TensionKv,
    i.NivelRed,
    i.Latitud,
    i.Longitud,
    i.Region,
    i.Zona,
    i.FuenteClave,
    i.EstadoConciliacion,
    i.EstadoValidacion,
    i.Fuente
FROM dgmesnie.RedElectricaSubestacionInventario i
WHERE i.Activa = 1
  AND EXISTS
  (
      SELECT 1 FROM @Patrones p
      WHERE i.NombreNormalizado LIKE N'%' + p.Patron + N'%'
  )
ORDER BY i.Nombre, i.FuenteClave, i.TensionKv;

DECLARE @VersionId BIGINT =
(
    SELECT TOP (1) VersionId
    FROM dgmesnie.RedElectricaVersion
    WHERE Activa = 1
    ORDER BY VersionId DESC
);

SELECT
    N'NODO' AS Origen,
    n.NodoClave,
    n.Nombre,
    n.TensionKv,
    n.Latitud,
    n.Longitud,
    n.Grado,
    n.EsVirtual,
    n.Fuente
FROM dgmesnie.RedElectricaNodo n
WHERE n.VersionId = @VersionId
  AND EXISTS
  (
      SELECT 1 FROM @Patrones p
      WHERE n.NombreNormalizado LIKE N'%' + p.Patron + N'%'
  )
ORDER BY n.Nombre, n.TensionKv;

SELECT
    N'ARISTA' AS Origen,
    a.AristaClave,
    a.Nombre,
    a.ExtremoNominalA,
    a.ExtremoNominalB,
    a.TensionKv,
    a.EstadoConexion,
    a.ConfianzaOrigen,
    a.ConfianzaDestino,
    no1.Nombre AS NodoOrigen,
    no1.Latitud AS LatitudOrigen,
    no1.Longitud AS LongitudOrigen,
    nd.Nombre AS NodoDestino,
    nd.Latitud AS LatitudDestino,
    nd.Longitud AS LongitudDestino
FROM dgmesnie.RedElectricaArista a
JOIN dgmesnie.RedElectricaNodo no1
  ON no1.VersionId = a.VersionId
 AND no1.NodoClave = a.NodoOrigenClave
JOIN dgmesnie.RedElectricaNodo nd
  ON nd.VersionId = a.VersionId
 AND nd.NodoClave = a.NodoDestinoClave
WHERE a.VersionId = @VersionId
  AND EXISTS
  (
      SELECT 1 FROM @Patrones p
      WHERE a.NombreNormalizado LIKE N'%' + p.Patron + N'%'
         OR UPPER(COALESCE(a.ExtremoNominalA, N'')) LIKE N'%' + p.Patron + N'%'
         OR UPPER(COALESCE(a.ExtremoNominalB, N'')) LIKE N'%' + p.Patron + N'%'
  )
ORDER BY a.Nombre;
