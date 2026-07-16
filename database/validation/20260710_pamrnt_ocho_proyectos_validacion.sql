SET NOCOUNT ON;

SELECT OrigenPrograma, COUNT(*) AS Total
FROM dgmesnie.vw_PAMProyectoVigente
GROUP BY OrigenPrograma
ORDER BY OrigenPrograma;

SELECT
    COUNT(*) AS ProyectosPAMRNT,
    COUNT(DISTINCT ClaveProyecto) AS ClavesDistintas,
    SUM(CASE WHEN FuenteDocumento = N'PAMRNT 2026-2040 - Proyectos identificados' THEN 1 ELSE 0 END) AS ConFuente,
    SUM(CASE WHEN PrioridadPrograma BETWEEN 1 AND 8 THEN 1 ELSE 0 END) AS ConPrioridad,
    SUM(CASE WHEN FechaNecesaria IS NOT NULL THEN 1 ELSE 0 END) AS ConFechaNecesaria
FROM dgmesnie.vw_PAMProyectoVigente
WHERE OrigenPrograma = N'PAMRNT';

SELECT PrioridadPrograma, ClaveProyecto, NombreProyecto, GRT, TipoProyecto,
       MontoProyectoMdp, FechaNecesaria, ZonaAtendida, FuenteDocumento, FuenteUbicacion
FROM dgmesnie.vw_PAMProyectoVigente
WHERE OrigenPrograma = N'PAMRNT'
ORDER BY PrioridadPrograma;

SELECT ProyectoId, COUNT(*) AS VersionesVigentes
FROM dgmesnie.PAMProyectoVersion
WHERE EsVersionVigente = 1
GROUP BY ProyectoId
HAVING COUNT(*) <> 1;

SELECT COUNT(*) AS RelacionesCreadasSinValidacion
FROM dgmesnie.PAMProyectoRelacionVersion
WHERE CargaId IN
(
    SELECT c.CargaId
    FROM dgmesnie.PAMCarga c
    INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = c.FuenteId
    WHERE f.HashSha256 = 'C433CEE3F92AC30D120C742CD8B6BBFFD325AA8C21A2F6CBE4710D94205B9230'
);
