SET NOCOUNT ON;

DECLARE @VersionId BIGINT =
(
    SELECT TOP (1) VersionId
    FROM dgmesnie.RedElectricaVersion
    WHERE Activa = 1
    ORDER BY VersionId DESC
);

SELECT
    a.AristaClave,
    a.Nombre,
    a.ExtremoNominalA,
    a.ExtremoNominalB,
    a.TensionKv,
    a.EstadoConexion,
    a.ConfianzaOrigen,
    a.ConfianzaDestino,
    a.ResolucionOrigen,
    a.ResolucionDestino,
    a.LongitudCatalogoKm,
    a.LongitudGeometriaKm,
    LEN(a.GeometriaJson) AS LongitudJson,
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
  AND
  (
      UPPER(a.Nombre) LIKE N'%MANUEL MORENO TORRES%SAN CRISTOBAL%'
      OR UPPER(a.Nombre) LIKE N'%SAN CRISTOBAL%MANUEL MORENO TORRES%'
      OR UPPER(a.Nombre) LIKE N'%JUCHITAN II%TECNOLOGICO%'
      OR UPPER(a.Nombre) LIKE N'%TECNOLOGICO%JUCHITAN II%'
      OR UPPER(a.Nombre) LIKE N'%CHARCAS%MATEHUALA%'
      OR UPPER(a.Nombre) LIKE N'%MATEHUALA%CHARCAS%'
      OR UPPER(a.Nombre) LIKE N'%MATEHUALA SUR%MATEHUALA%'
      OR UPPER(a.Nombre) LIKE N'%MATEHUALA%MATEHUALA SUR%'
      OR UPPER(a.Nombre) LIKE N'%METROPOLI%TIJUANA I%'
      OR UPPER(a.Nombre) LIKE N'%TIJUANA I%METROPOLI%'
      OR UPPER(a.Nombre) LIKE N'%LA HERRADURA%TECATE%'
      OR UPPER(a.Nombre) LIKE N'%TECATE%LA HERRADURA%'
  )
ORDER BY a.Nombre, a.AristaClave;

SELECT
    i.RegistroClave,
    i.Nombre,
    i.TensionKv,
    i.Latitud,
    i.Longitud,
    i.FuenteClave,
    i.EstadoConciliacion,
    i.EstadoValidacion
FROM dgmesnie.RedElectricaSubestacionInventario i
WHERE i.Activa = 1
  AND i.Latitud IS NOT NULL
  AND
  (
      UPPER(i.Nombre) LIKE N'%ANGOSTURA%'
      OR UPPER(i.Nombre) LIKE N'%MANUEL MORENO%'
      OR UPPER(i.Nombre) LIKE N'%SAN CRISTOBAL ORIENTE%'
      OR UPPER(i.Nombre) LIKE N'%OXCHUC%'
      OR UPPER(i.Nombre) LIKE N'%TRINITARIA%'
      OR UPPER(i.Nombre) LIKE N'%JUCHITAN II%'
      OR UPPER(i.Nombre) LIKE N'%TRANS%STMICA%'
      OR UPPER(i.Nombre) LIKE N'%TAPANATEPEC%'
      OR UPPER(i.Nombre) LIKE N'%CHARCAS POTENCIA%'
      OR UPPER(i.Nombre) LIKE N'%MATEHUALA%'
      OR UPPER(i.Nombre) LIKE N'%PUEBLO NUEVO%'
      OR UPPER(i.Nombre) LIKE N'%EL MAYO%'
      OR UPPER(i.Nombre) LIKE N'%LA HERRADURA%'
      OR UPPER(i.Nombre) LIKE N'%TECATE%'
      OR UPPER(i.Nombre) LIKE N'%METROPOLI POTENCIA%'
      OR UPPER(i.Nombre) LIKE N'%TIJUANA I%'
      OR UPPER(i.Nombre) LIKE N'%EL FLORIDO%'
      OR UPPER(i.Nombre) LIKE N'%FRANCISCO VILLA%'
      OR UPPER(i.Nombre) LIKE N'%ENCINAL%'
  )
ORDER BY i.Nombre, i.FuenteClave;
