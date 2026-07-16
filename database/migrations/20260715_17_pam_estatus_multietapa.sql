SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

/*
  Fase 17: cierre del semáforo para los 6 proyectos multietapa.
  Sus EtapaProyecto traen saltos de línea/tabuladores internos, por lo que el
  cruce exacto contra PAMEstatusMapeo no coincidía. La vista y el detalle ahora
  normalizan CR/LF/TAB a espacio (y colapsan espacios dobles) antes de cruzar.
  Las llaves del mapeo ya están capturadas con espacios simples.
*/

EXEC(N'
CREATE OR ALTER VIEW dgmesnie.vw_PAMProyectoVigente
AS
SELECT
    p.ProyectoId, p.ProyectoUid, p.ProyectoModernizacionIdOrigen,
    v.ProyectoVersionId, v.NumeroVersion, v.OrigenPrograma, v.Programa, v.AnioPrograma,
    v.TipoProyecto, v.EstadoVigenciaCartera, v.ClaveProyecto, v.Numero, v.NumeroOriginal,
    v.GRT, v.NombreProyecto, v.TipoFinanciamiento, v.AnioInstruccion, v.EtapaProyecto,
    v.MontoProyectoMdp, v.ElementosEquiposAsociados, v.FechaEstimadaInicio,
    v.FeoIndicadaOficioSener, v.FeoFactible, v.PorcentajeAvanceEjecucion,
    v.CircunstanciasAtrasos, v.AccionesMitigacionCorreccion, v.EstadoRealProyecto,
    v.ComentariosNivelPriorizacion, v.ClasificacionSener, v.FechaProgramacionTrimestre,
    v.QuincenaPublicacion, v.UniversoPresentacionPresidencia, v.Mva, v.Mvar, v.KmC,
    v.PrioridadPrograma, v.ZonaAtendida, v.FechaNecesaria,
    COALESCE(NULLIF(LTRIM(RTRIM(v.EstatusLicitacion)), N''''), m.Estatus) AS EstatusLicitacion,
    v.VigenteDesde, v.ActivoEnFuente, v.FuenteId, f.NombreDocumento AS FuenteDocumento,
    f.FechaCorte, f.HojaPaginaSeccion AS FuenteUbicacion
FROM dgmesnie.PAMProyecto p
INNER JOIN dgmesnie.PAMProyectoVersion v ON v.ProyectoId = p.ProyectoId AND v.EsVersionVigente = 1
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
LEFT JOIN dgmesnie.PAMEstatusMapeo m
    ON m.EtapaProyecto = LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
           v.EtapaProyecto, NCHAR(13), N'' ''), NCHAR(10), N'' ''), NCHAR(9), N'' ''), N''  '', N'' ''), N''  '', N'' '')))
WHERE p.Activo = 1;
');
GO

/* Radiografía final: Por clasificar debe quedar en 0. */
SELECT ISNULL(EstatusLicitacion, N'Por clasificar') AS Estatus, COUNT(*) AS Total
FROM dgmesnie.vw_PAMProyectoVigente
WHERE EstadoVigenciaCartera = N'Vigente'
GROUP BY ISNULL(EstatusLicitacion, N'Por clasificar')
ORDER BY Total DESC;
