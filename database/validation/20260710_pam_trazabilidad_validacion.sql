SET NOCOUNT ON;

SELECT N'Origen' AS Revision, COUNT(*) AS Total
FROM dgmesnie.InformePormenorizadoModernizacion
UNION ALL
SELECT N'PAMProyecto', COUNT(*) FROM dgmesnie.PAMProyecto
UNION ALL
SELECT N'Versiones vigentes', COUNT(*) FROM dgmesnie.PAMProyectoVersion WHERE EsVersionVigente = 1
UNION ALL
SELECT N'Vista vigente', COUNT(*) FROM dgmesnie.vw_PAMProyectoVigente;

SELECT EstadoVigenciaCartera, COUNT(*) AS Total
FROM dgmesnie.vw_PAMProyectoVigente
GROUP BY EstadoVigenciaCartera
ORDER BY EstadoVigenciaCartera;

SELECT ProyectoId, COUNT(*) AS VersionesVigentes
FROM dgmesnie.PAMProyectoVersion
WHERE EsVersionVigente = 1
GROUP BY ProyectoId
HAVING COUNT(*) <> 1;

SELECT p.ProyectoId, p.ProyectoModernizacionIdOrigen
FROM dgmesnie.PAMProyecto p
LEFT JOIN dgmesnie.PAMProyectoVersion v
    ON v.ProyectoId = p.ProyectoId AND v.EsVersionVigente = 1
WHERE v.ProyectoVersionId IS NULL;

SELECT
    SUM(CASE WHEN o.NombreProyecto = v.NombreProyecto THEN 0 ELSE 1 END) AS DiferenciasNombre,
    SUM(CASE WHEN ISNULL(o.ClavePem, N'') = ISNULL(v.ClaveProyecto, N'') THEN 0 ELSE 1 END) AS DiferenciasClave,
    SUM(CASE WHEN ISNULL(o.MontoProyectoMdp, 0) = ISNULL(v.MontoProyectoMdp, 0) THEN 0 ELSE 1 END) AS DiferenciasMonto
FROM dgmesnie.InformePormenorizadoModernizacion o
INNER JOIN dgmesnie.PAMProyecto p ON p.ProyectoModernizacionIdOrigen = o.ProyectoModernizacionId
INNER JOIN dgmesnie.vw_PAMProyectoVigente v ON v.ProyectoId = p.ProyectoId;
