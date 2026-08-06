using System.Text.Json;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models;

namespace NSIE.Servicios
{
    public interface IPamrntProyectosIdentificadosService
    {
        Task<PamrntProyectosIdentificadosViewModel> ObtenerProyectosAsync(PamDashboardFiltro filtro = null);
        Task<PamProyectoDetalleViewModel> ObtenerDetalleAsync(long proyectoId);
        Task<PamrntFichaProyectoViewModel> ObtenerFichaAsync(string clavePem);
        Task<List<PamUsuarioDestinatario>> ObtenerDestinatariosAsync();
    }

    public class PamrntProyectosIdentificadosService : IPamrntProyectosIdentificadosService
    {
        private const string RutaFichaI26 = "data/pamrnt/ficha-i26-pe1.json";
        private readonly IWebHostEnvironment _environment;
        private readonly string _connectionString;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public PamrntProyectosIdentificadosService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<PamrntProyectosIdentificadosViewModel> ObtenerProyectosAsync(PamDashboardFiltro filtro = null)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException("La conexión SQL del repositorio PAM no está configurada.");

            filtro ??= new PamDashboardFiltro();
            filtro.Normalizar();

            const string sql = @"
SELECT v.* INTO #PamFiltrado
FROM dgmesnie.vw_PAMProyectoVigente v
WHERE
      (@Busqueda IS NULL OR v.ClaveProyecto LIKE @BusquedaLike OR v.NombreProyecto LIKE @BusquedaLike
       OR v.GRT LIKE @BusquedaLike OR v.FuenteDocumento LIKE @BusquedaLike
       OR EXISTS (SELECT 1 FROM dgmesnie.PAMProyectoClaveVersion clave
                  WHERE clave.ProyectoId = v.ProyectoId AND clave.EsVigente = 1
                    AND clave.ClaveProyecto LIKE @BusquedaLike))
      AND (@Origen IS NULL OR v.OrigenPrograma = @Origen)
      AND (@Etapa IS NULL OR v.EtapaProyecto = @Etapa)
      AND (@Region IS NULL OR v.GRT = @Region)
      AND (@Tipo IS NULL OR v.TipoProyecto = @Tipo)
      AND (@Fuente IS NULL OR v.FuenteDocumento = @Fuente)
      AND (@Estatus IS NULL
           OR v.EstatusLicitacion = @Estatus
           OR (@Estatus = N'Por clasificar' AND NULLIF(LTRIM(RTRIM(v.EstatusLicitacion)), N'') IS NULL))
      AND (@Equipo IS NULL
           OR (@Equipo = N'lineas' AND v.KmC > 0)
           OR (@Equipo = N'transformacion' AND v.Mva > 0)
           OR (@Equipo = N'compensacion' AND v.Mvar > 0))
      AND (@FiltrarEmpalme = 0 OR v.ProyectoId IN @EmpalmeIds)
      AND
      (
          @Universo = N'todos'
          OR (@Universo = N'vigentes' AND v.EstadoVigenciaCartera = N'Vigente')
          OR (@Universo = N'pam' AND v.EstadoVigenciaCartera = N'Vigente' AND v.OrigenPrograma = N'PAM')
          OR (@Universo = N'pamrnt' AND v.EstadoVigenciaCartera = N'Vigente' AND v.OrigenPrograma = N'PAMRNT')
          OR (@Universo = N'cancelados' AND v.EstadoVigenciaCartera = N'Cancelado')
          OR (@Universo = N'recientes' AND (v.NumeroVersion > 1 OR v.OrigenPrograma = N'PAMRNT'))
      )
;

SELECT
    v.ProyectoId,
    v.PrioridadPrograma AS Prioridad,
    v.GRT AS Gcr,
    v.ClaveProyecto AS ClavePem,
    v.NombreProyecto AS Proyecto,
    COALESCE(v.FechaNecesaria, v.FeoFactible, v.FeoIndicadaOficioSener) AS FechaNecesaria,
    v.AnioPrograma AS EjercicioPlaneacion,
    v.ZonaAtendida,
    v.MontoProyectoMdp AS InversionMdp,
    v.OrigenPrograma,
    v.TipoProyecto,
    v.EtapaProyecto,
    v.EstadoVigenciaCartera,
    v.EstatusLicitacion,
    v.NumeroVersion,
    v.FuenteDocumento,
    v.FechaCorte,
    v.FuenteUbicacion,
    claves.ClavesAlternas,
    cambio.UltimoCambioUtc,
    relacion.ClavePadre,
    relacion.NombrePadre
FROM #PamFiltrado v
OUTER APPLY
(
    SELECT MAX(c.FechaRegistroUtc) AS UltimoCambioUtc
    FROM dgmesnie.PAMCambio c
    WHERE c.ProyectoId = v.ProyectoId
) cambio
OUTER APPLY
(
    SELECT STRING_AGG(CONVERT(NVARCHAR(MAX), clave.ClaveProyecto), NCHAR(10)) AS ClavesAlternas
    FROM dgmesnie.PAMProyectoClaveVersion clave
    WHERE clave.ProyectoId = v.ProyectoId AND clave.EsVigente = 1
) claves
OUTER APPLY
(
    SELECT TOP (1) j.ClavePadre, j.NombrePadre
    FROM dgmesnie.vw_PAMProyectoJerarquiaVigente j
    WHERE j.ProyectoHijoId = v.ProyectoId AND j.EstadoValidacion = N'Validada'
    ORDER BY j.VigenteDesde DESC
) relacion
ORDER BY
    CASE WHEN v.EstadoVigenciaCartera = N'Vigente' THEN 0 ELSE 1 END,
    CASE WHEN v.OrigenPrograma = N'PAMRNT' THEN 0 ELSE 1 END,
    v.PrioridadPrograma, v.Numero, v.ClaveProyecto
OFFSET @Offset ROWS FETCH NEXT @TamanoPagina ROWS ONLY;

SELECT COUNT(*) FROM #PamFiltrado;

SELECT
    SUM(CASE WHEN OrigenPrograma = N'PAM' THEN 1 ELSE 0 END) AS TotalPam,
    SUM(CASE WHEN OrigenPrograma = N'PAM' AND EstadoVigenciaCartera = N'Vigente' THEN 1 ELSE 0 END) AS TotalPamVigentes,
    SUM(CASE WHEN OrigenPrograma = N'PAMRNT' THEN 1 ELSE 0 END) AS TotalPamrnt,
    SUM(CASE WHEN EstadoVigenciaCartera = N'Vigente' THEN 1 ELSE 0 END) AS TotalVigentes,
    SUM(CASE WHEN EstadoVigenciaCartera = N'Cancelado' THEN 1 ELSE 0 END) AS TotalCancelados,
    SUM(CASE WHEN NumeroVersion > 1 OR OrigenPrograma = N'PAMRNT' THEN 1 ELSE 0 END) AS TotalRecientes,
    MAX(FechaCorte) AS FechaCorteMaxima,
    CAST(SUM(CASE WHEN EstadoVigenciaCartera = N'Vigente' THEN ISNULL(KmC, 0) ELSE 0 END) AS DECIMAL(18,2)) AS TotalKmC,
    CAST(SUM(CASE WHEN EstadoVigenciaCartera = N'Vigente' THEN ISNULL(Mva, 0) ELSE 0 END) AS DECIMAL(18,2)) AS TotalMva,
    CAST(SUM(CASE WHEN EstadoVigenciaCartera = N'Vigente' THEN ISNULL(Mvar, 0) ELSE 0 END) AS DECIMAL(18,2)) AS TotalMvar,
    CAST(SUM(CASE WHEN EstadoVigenciaCartera = N'Vigente' THEN ISNULL(MontoProyectoMdp, 0) ELSE 0 END) AS DECIMAL(18,2)) AS TotalInversionVigente,
    CAST(SUM(CASE WHEN EstadoVigenciaCartera = N'Vigente' AND OrigenPrograma = N'PAMRNT' THEN ISNULL(MontoProyectoMdp, 0) ELSE 0 END) AS DECIMAL(18,2)) AS TotalInversionPamrnt,
    SUM(CASE WHEN EstadoVigenciaCartera = N'Vigente' AND (KmC IS NOT NULL OR Mva IS NOT NULL OR Mvar IS NOT NULL) THEN 1 ELSE 0 END) AS ProyectosConMetricas
FROM #PamFiltrado;

-- Los contadores de navegación describen siempre el repositorio completo.
-- Los indicadores anteriores sí se calculan sobre el universo y filtros activos.
SELECT
    SUM(CASE WHEN OrigenPrograma = N'PAM' THEN 1 ELSE 0 END) AS TotalPam,
    SUM(CASE WHEN OrigenPrograma = N'PAM' AND EstadoVigenciaCartera = N'Vigente' THEN 1 ELSE 0 END) AS TotalPamVigentes,
    SUM(CASE WHEN OrigenPrograma = N'PAMRNT' THEN 1 ELSE 0 END) AS TotalPamrnt,
    SUM(CASE WHEN EstadoVigenciaCartera = N'Vigente' THEN 1 ELSE 0 END) AS TotalVigentes,
    SUM(CASE WHEN EstadoVigenciaCartera = N'Cancelado' THEN 1 ELSE 0 END) AS TotalCancelados,
    SUM(CASE WHEN NumeroVersion > 1 OR OrigenPrograma = N'PAMRNT' THEN 1 ELSE 0 END) AS TotalRecientes
FROM dgmesnie.vw_PAMProyectoVigente;

SELECT ISNULL(NULLIF(LTRIM(RTRIM(EstatusLicitacion)), N''), N'Por clasificar') AS Estatus, COUNT(*) AS Total
FROM #PamFiltrado
WHERE EstadoVigenciaCartera = N'Vigente'
GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(EstatusLicitacion)), N''), N'Por clasificar');

SELECT FechaNecesaria, FeoFactible
FROM #PamFiltrado
WHERE EstadoVigenciaCartera = N'Vigente';

SELECT DISTINCT EtapaProyecto FROM dgmesnie.vw_PAMProyectoVigente WHERE NULLIF(LTRIM(RTRIM(EtapaProyecto)), N'') IS NOT NULL ORDER BY EtapaProyecto;
SELECT DISTINCT GRT FROM dgmesnie.vw_PAMProyectoVigente WHERE NULLIF(LTRIM(RTRIM(GRT)), N'') IS NOT NULL ORDER BY GRT;
SELECT DISTINCT TipoProyecto FROM dgmesnie.vw_PAMProyectoVigente WHERE NULLIF(LTRIM(RTRIM(TipoProyecto)), N'') IS NOT NULL ORDER BY TipoProyecto;
SELECT DISTINCT FuenteDocumento FROM dgmesnie.vw_PAMProyectoVigente WHERE NULLIF(LTRIM(RTRIM(FuenteDocumento)), N'') IS NOT NULL ORDER BY FuenteDocumento;";

            await using var connection = new SqlConnection(_connectionString);

            // Pre-cálculo del filtro de empalme: el nivel se deriva en C# con el parser
            // de fechas, así que se resuelven aquí los IDs del nivel pedido y el SQL filtra por ellos.
            IEnumerable<long> empalmeIds = new List<long> { -1 };
            var filtrarEmpalme = false;
            if (!string.IsNullOrWhiteSpace(filtro.Empalme))
            {
                var fechasFiltro = await connection.QueryAsync<(long ProyectoId, string FechaNecesaria, string FeoFactible)>(
                    "SELECT ProyectoId, FechaNecesaria, FeoFactible FROM dgmesnie.vw_PAMProyectoVigente WHERE EstadoVigenciaCartera = N'Vigente';");
                var ids = fechasFiltro
                    .Where(f => new PamEmpalme
                    {
                        FechaNecesaria = PamFechaParser.Parsear(f.FechaNecesaria),
                        FeoFactible = PamFechaParser.Parsear(f.FeoFactible)
                    }.Nivel == filtro.Empalme)
                    .Select(f => f.ProyectoId)
                    .ToList();
                empalmeIds = ids.Count > 0 ? ids : new List<long> { -1 };
                filtrarEmpalme = true;
            }

            var parameters = new
            {
                Busqueda = Normalizar(filtro.Busqueda),
                BusquedaLike = string.IsNullOrWhiteSpace(filtro.Busqueda) ? null : $"%{filtro.Busqueda.Trim()}%",
                Origen = Normalizar(filtro.Origen),
                Etapa = Normalizar(filtro.Etapa),
                Region = Normalizar(filtro.Region),
                Tipo = Normalizar(filtro.Tipo),
                Fuente = Normalizar(filtro.Fuente),
                Estatus = Normalizar(filtro.Estatus),
                Equipo = Normalizar(filtro.Equipo),
                EmpalmeIds = empalmeIds,
                FiltrarEmpalme = filtrarEmpalme,
                filtro.Universo,
                Offset = (filtro.Pagina - 1) * filtro.TamanoPagina,
                filtro.TamanoPagina
            };

            using var multi = await connection.QueryMultipleAsync(sql, parameters);
            var proyectos = (await multi.ReadAsync<PamrntProyectoIdentificado>()).ToList();
            var totalFiltrado = await multi.ReadSingleAsync<int>();
            var resumen = await multi.ReadSingleAsync<PamRepositorioResumen>();
            var navegacion = await multi.ReadSingleAsync<PamRepositorioResumen>();
            var conteosEstatus = (await multi.ReadAsync<(string Estatus, int Total)>())
                .ToDictionary(x => x.Estatus, x => x.Total, StringComparer.OrdinalIgnoreCase);
            var fechasEmpalme = (await multi.ReadAsync<(string FechaNecesaria, string FeoFactible)>()).ToList();
            var conteosEmpalme = fechasEmpalme
                .Select(f => new PamEmpalme
                {
                    FechaNecesaria = PamFechaParser.Parsear(f.FechaNecesaria),
                    FeoFactible = PamFechaParser.Parsear(f.FeoFactible)
                })
                .GroupBy(e => e.Nivel)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
            var etapas = (await multi.ReadAsync<string>()).ToList();
            var regiones = (await multi.ReadAsync<string>()).ToList();
            var tipos = (await multi.ReadAsync<string>()).ToList();
            var fuentes = (await multi.ReadAsync<string>()).ToList();

            foreach (var proyecto in proyectos)
                proyecto.FichaDisponible = proyecto.ProyectoId > 0;

            return new PamrntProyectosIdentificadosViewModel
            {
                Fuente = "Repositorio histórico PAM/PAMRNT",
                VersionDatos = resumen.FechaCorteMaxima?.ToString("yyyy-MM-dd") ?? "Sin fecha de corte",
                CruceBaseDisponible = true,
                AvisoCruce = "Datos vigentes consultados desde dgmesnie.vw_PAMProyectoVigente.",
                Proyectos = proyectos,
                TotalPam = resumen.TotalPam,
                TotalPamVigentes = resumen.TotalPamVigentes,
                TotalPamrnt = resumen.TotalPamrnt,
                TotalVigentes = resumen.TotalVigentes,
                TotalCancelados = resumen.TotalCancelados,
                TotalRecientes = resumen.TotalRecientes,
                TotalVigentesRepositorio = navegacion.TotalVigentes,
                TotalPamVigentesRepositorio = navegacion.TotalPamVigentes,
                TotalPamrntRepositorio = navegacion.TotalPamrnt,
                TotalCanceladosRepositorio = navegacion.TotalCancelados,
                TotalRecientesRepositorio = navegacion.TotalRecientes,
                TotalKmC = resumen.TotalKmC,
                TotalMva = resumen.TotalMva,
                TotalMvar = resumen.TotalMvar,
                TotalInversionVigente = resumen.TotalInversionVigente,
                TotalInversionPamrnt = resumen.TotalInversionPamrnt,
                ProyectosConMetricas = resumen.ProyectosConMetricas,
                TotalFiltrado = totalFiltrado,
                Filtro = filtro,
                ConteosEstatus = conteosEstatus,
                ConteosEmpalme = conteosEmpalme,
                EtapasDisponibles = etapas,
                RegionesDisponibles = regiones,
                TiposDisponibles = tipos,
                FuentesDisponibles = fuentes
            };
        }

        public async Task<PamProyectoDetalleViewModel> ObtenerDetalleAsync(long proyectoId)
        {
            if (proyectoId <= 0) return null;

            const string sql = @"
SELECT
    p.ProyectoId, p.ProyectoUid, p.ProyectoModernizacionIdOrigen,
    v.ProyectoVersionId, v.NumeroVersion, v.OrigenPrograma, v.Programa, v.AnioPrograma,
    v.TipoProyecto, v.EstadoVigenciaCartera, v.ClaveProyecto, v.NombreProyecto, v.GRT,
    v.EtapaProyecto,
    COALESCE(NULLIF(LTRIM(RTRIM(v.EstatusLicitacion)), N''), mapeo.Estatus) AS EstatusLicitacion,
    v.TipoFinanciamiento, v.AnioInstruccion, v.MontoProyectoMdp,
    v.PorcentajeAvanceEjecucion, v.ElementosEquiposAsociados, v.FechaEstimadaInicio,
    v.FeoIndicadaOficioSener, v.FeoFactible, v.FechaNecesaria, v.ZonaAtendida,
    v.PrioridadPrograma, v.EstadoRealProyecto, v.CircunstanciasAtrasos,
    v.AccionesMitigacionCorreccion, v.ComentariosNivelPriorizacion, v.ClasificacionSener,
    v.Mva, v.Mvar, v.KmC, v.VigenteDesde,
    claves.ClavesAlternas,
    f.NombreDocumento AS FuenteDocumento, f.FechaCorte, f.HojaPaginaSeccion AS FuenteUbicacion
FROM dgmesnie.PAMProyecto p
LEFT JOIN dgmesnie.PAMProyectoVersion v
    ON v.ProyectoId = p.ProyectoId AND v.EsVersionVigente = 1
LEFT JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
LEFT JOIN dgmesnie.PAMEstatusMapeo mapeo
    ON mapeo.EtapaProyecto = LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
           v.EtapaProyecto, NCHAR(13), N' '), NCHAR(10), N' '), NCHAR(9), N' '), N'  ', N' '), N'  ', N' ')))
OUTER APPLY
(
    SELECT STRING_AGG(CONVERT(NVARCHAR(MAX), clave.ClaveProyecto), NCHAR(10)) AS ClavesAlternas
    FROM dgmesnie.PAMProyectoClaveVersion clave
    WHERE clave.ProyectoId = p.ProyectoId AND clave.EsVigente = 1
) claves
WHERE p.ProyectoId = @ProyectoId;

SELECT
    ProyectoVersionId, NumeroVersion, EsVersionVigente, VigenteDesde, VigenteHasta,
    ClaveProyecto, NombreProyecto, EtapaProyecto, EstadoVigenciaCartera, MontoProyectoMdp,
    v.MotivoCambio, f.NombreDocumento AS FuenteDocumento, f.FechaCorte,
    v.FechaRegistroUtc, v.UsuarioRegistro
FROM dgmesnie.PAMProyectoVersion v
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = v.FuenteId
WHERE v.ProyectoId = @ProyectoId
ORDER BY v.NumeroVersion DESC;

SELECT
    c.CambioId, c.TipoCambio, c.Campo, c.ValorAnterior, c.ValorNuevo,
    c.FechaRegistroUtc, c.UsuarioRegistro, carga.TipoCarga,
    fuente.NombreDocumento AS FuenteDocumento, fuente.FechaCorte
FROM dgmesnie.PAMCambio c
INNER JOIN dgmesnie.PAMCarga carga ON carga.CargaId = c.CargaId
INNER JOIN dgmesnie.PAMFuente fuente ON fuente.FuenteId = carga.FuenteId
WHERE c.ProyectoId = @ProyectoId
ORDER BY c.FechaRegistroUtc DESC, c.CambioId DESC;

SELECT
    f.FuenteId, f.NombreDocumento, f.TipoDocumento, f.FechaDocumento, f.FechaCorte,
    f.RutaArchivo, f.HojaPaginaSeccion, f.VersionDocumento, f.HashSha256, f.Observaciones
FROM dgmesnie.PAMFuente f
WHERE EXISTS
(
    SELECT 1 FROM dgmesnie.PAMProyectoVersion v
    WHERE v.ProyectoId = @ProyectoId AND v.FuenteId = f.FuenteId
)
OR EXISTS
(
    SELECT 1 FROM dgmesnie.PAMProyectoRelacionVersion r
    WHERE (r.ProyectoPadreId = @ProyectoId OR r.ProyectoHijoId = @ProyectoId)
      AND r.FuenteId = f.FuenteId
)
OR EXISTS
(
    SELECT 1
    FROM dgmesnie.PAMCambio c
    INNER JOIN dgmesnie.PAMCarga carga ON carga.CargaId = c.CargaId
    WHERE c.ProyectoId = @ProyectoId AND carga.FuenteId = f.FuenteId
)
ORDER BY f.FechaCorte DESC, f.FuenteId DESC;

SELECT
    r.ProyectoRelacionVersionId,
    CASE WHEN r.ProyectoPadreId = @ProyectoId THEN N'Hijo' ELSE N'Padre' END AS Direccion,
    CASE WHEN r.ProyectoPadreId = @ProyectoId THEN r.ProyectoHijoId ELSE r.ProyectoPadreId END AS ProyectoRelacionadoId,
    CASE WHEN r.ProyectoPadreId = @ProyectoId THEN hijo.ClaveProyecto ELSE padre.ClaveProyecto END AS ClaveRelacionada,
    CASE WHEN r.ProyectoPadreId = @ProyectoId THEN hijo.NombreProyecto ELSE padre.NombreProyecto END AS NombreRelacionado,
    r.TipoRelacion, r.EstadoValidacion, r.VigenteDesde, r.VigenteHasta, r.EsRelacionVigente,
    f.NombreDocumento AS FuenteDocumento, r.HojaPaginaSeccion, r.Observaciones
FROM dgmesnie.PAMProyectoRelacionVersion r
INNER JOIN dgmesnie.PAMFuente f ON f.FuenteId = r.FuenteId
LEFT JOIN dgmesnie.vw_PAMProyectoVigente padre ON padre.ProyectoId = r.ProyectoPadreId
LEFT JOIN dgmesnie.vw_PAMProyectoVigente hijo ON hijo.ProyectoId = r.ProyectoHijoId
WHERE r.ProyectoPadreId = @ProyectoId OR r.ProyectoHijoId = @ProyectoId
ORDER BY r.EsRelacionVigente DESC, r.VigenteDesde DESC;";

            await using var connection = new SqlConnection(_connectionString);
            using var multi = await connection.QueryMultipleAsync(sql, new { ProyectoId = proyectoId });
            var actual = await multi.ReadSingleOrDefaultAsync<PamProyectoDetalleActual>();
            if (actual == null) return null;

            return new PamProyectoDetalleViewModel
            {
                Actual = actual,
                Historial = (await multi.ReadAsync<PamProyectoVersionResumen>()).ToList(),
                Cambios = (await multi.ReadAsync<PamCambioDetalle>()).ToList(),
                Fuentes = (await multi.ReadAsync<PamFuenteDetalle>()).ToList(),
                Relaciones = (await multi.ReadAsync<PamRelacionDetalle>()).ToList()
            };
        }

        public async Task<PamrntFichaProyectoViewModel> ObtenerFichaAsync(string clavePem)
        {
            if (string.IsNullOrWhiteSpace(clavePem)) return null;

            var proyectoId = await ObtenerProyectoIdPorClaveAsync(clavePem.Trim());
            if (!proyectoId.HasValue) return null;

            var detalle = await ObtenerDetalleAsync(proyectoId.Value);
            if (detalle?.Actual == null) return null;

            var expedienteOrigen = await ObtenerExpedientePormenorizadoOrigenAsync(detalle.Actual);

            var proyecto = ConstruirProyectoIdentificado(detalle.Actual);
            var ficha = await ObtenerFichaDesdeBaseAsync(proyecto.ProyectoId);

            // Enriquecida = viene de ficha validada (BD/JSON) con alternativas, comparativa
            // y evaluación económica reales; las dinámicas ocultan esas láminas para no
            // dejar huecos de alternativas no documentadas.
            var enriquecida = ficha != null;
            if (ficha == null && string.Equals(proyecto.ClavePem, "I26-PE1", StringComparison.OrdinalIgnoreCase))
            {
                ficha = await IntentarLeerJsonAsync<PamrntFichaProyecto>(RutaFichaI26);
                enriquecida = ficha != null;
            }

            if (ficha == null)
            {
                ficha = ConstruirFichaDinamica(proyecto, detalle);
                enriquecida = false;
            }

            // Adjunta siempre los recursos CDN (imágenes/figuras) al modelo de la ficha
            var recursos = await ObtenerRecursosDesdeBaseAsync(proyecto.ProyectoId);
            if (recursos != null && recursos.Count > 0)
            {
                ficha.Recursos = recursos;
            }

            NormalizarFicha(ficha, proyecto, detalle);

            var contexto = await ObtenerProyectosAsync(new PamDashboardFiltro
            {
                Universo = string.Equals(proyecto.OrigenPrograma, "PAMRNT", StringComparison.OrdinalIgnoreCase) ? "pamrnt" : "todos",
                Pagina = 1,
                TamanoPagina = 50
            });

            var impacto = await ObtenerImpactoRegionalAsync(proyecto.ProyectoId, detalle.Actual.GRT);

            // Panorama contextual: los PAMRNT identificados muestran los 8; los PAM
            // muestran los proyectos vigentes de su misma GCR (más relevante que los 8).
            var esPamrnt = string.Equals(proyecto.OrigenPrograma, "PAMRNT", StringComparison.OrdinalIgnoreCase);
            var proyectosRegion = new List<PamrntProyectoIdentificado>();
            if (!esPamrnt && !string.IsNullOrWhiteSpace(detalle.Actual.GRT))
            {
                var region = await ObtenerProyectosAsync(new PamDashboardFiltro
                {
                    Universo = "vigentes",
                    Region = detalle.Actual.GRT,
                    Pagina = 1,
                    TamanoPagina = 10
                });
                proyectosRegion = region.Proyectos;
            }

            return new PamrntFichaProyectoViewModel
            {
                Proyecto = proyecto,
                Ficha = ficha,
                Detalle = detalle,
                ExpedientePormenorizadoOrigen = expedienteOrigen,
                ImpactoRegional = impacto,
                ContextoCartera = contexto.Proyectos,
                ProyectosRegion = proyectosRegion,
                FichaEnriquecida = enriquecida
            };
        }

        private async Task<PamExpedientePormenorizadoOrigen> ObtenerExpedientePormenorizadoOrigenAsync(PamProyectoDetalleActual actual)
        {
            var claves = new[] { actual.ClaveProyecto }
                .Concat((actual.ClavesAlternas ?? string.Empty).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(NormalizarClavePormenorizada)
                .Where(clave => !string.IsNullOrWhiteSpace(clave))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            const string sql = @"
IF OBJECT_ID(N'dgmesnie.InformePormenorizadoModernizacion', N'U') IS NULL
BEGIN
    SELECT CAST(NULL AS INT) AS ProyectoModernizacionId,
           CAST(NULL AS INT) AS Numero,
           CAST(NULL AS INT) AS NumeroOriginal,
           CAST(NULL AS NVARCHAR(200)) AS ClavePem,
           CAST(NULL AS NVARCHAR(500)) AS NombreProyecto,
           CAST(NULL AS NVARCHAR(500)) AS FuenteArchivo,
           CAST(NULL AS DATETIME2) AS FechaCarga
    WHERE 1 = 0;
END
ELSE
BEGIN
    SELECT ProyectoModernizacionId, Numero, NumeroOriginal, ClavePem, NombreProyecto, FuenteArchivo, FechaCarga
    FROM dgmesnie.InformePormenorizadoModernizacion
    WHERE ProyectoModernizacionId = @ProyectoModernizacionIdOrigen
       OR UPPER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(COALESCE(ClavePem, N''))), N' ', N''), N'‐', N'-'), N'‑', N'-'), N'–', N'-'), N'—', N'-'), N'−', N'-')) IN @Claves;
END";

            await using var connection = new SqlConnection(_connectionString);
            var matches = (await connection.QueryAsync<ExpedientePormenorizadoRow>(sql, new
            {
                actual.ProyectoModernizacionIdOrigen,
                Claves = claves.Length == 0 ? new[] { "__SIN_CLAVE__" } : claves
            })).ToList();

            var legacy = actual.ProyectoModernizacionIdOrigen.HasValue
                ? matches.SingleOrDefault(row => row.ProyectoModernizacionId == actual.ProyectoModernizacionIdOrigen.Value)
                : null;
            if (legacy != null)
            {
                return MapearExpedienteOrigen(legacy, "Registro legado enlazado por identificador interno");
            }

            if (actual.ProyectoModernizacionIdOrigen.HasValue)
            {
                return new PamExpedientePormenorizadoOrigen
                {
                    ProyectoModernizacionId = actual.ProyectoModernizacionIdOrigen,
                    MetodoVinculo = "Identificador legado de cartera",
                    Estado = "Vínculo histórico sin detalle disponible",
                    Mensaje = "La cartera conserva el identificador de origen, pero el registro no está disponible en la tabla vigente del informe. No se sustituyó por una coincidencia de nombre."
                };
            }

            var exactos = matches
                .Where(row => claves.Contains(NormalizarClavePormenorizada(row.ClavePem), StringComparer.Ordinal))
                .GroupBy(row => row.ProyectoModernizacionId)
                .Select(group => group.First())
                .ToList();
            if (exactos.Count == 1)
            {
                return MapearExpedienteOrigen(exactos[0], "Clave PEM principal o alterna única");
            }

            return new PamExpedientePormenorizadoOrigen
            {
                Estado = "Pendiente de vinculación",
                MetodoVinculo = claves.Length == 0 ? "Sin clave PEM disponible" : "Clave PEM sin coincidencia única",
                Mensaje = exactos.Count > 1
                    ? "Más de un registro pormenorizado comparte una clave disponible; requiere resolución manual antes de vincularse."
                    : "No existe un vínculo comprobable al expediente pormenorizado. No se infirió por nombre, región ni por avances posteriores."
            };
        }

        private static PamExpedientePormenorizadoOrigen MapearExpedienteOrigen(ExpedientePormenorizadoRow row, string metodo)
            => new()
            {
                TieneVinculo = true,
                TieneAccesoDetalle = true,
                ProyectoModernizacionId = row.ProyectoModernizacionId,
                Numero = row.Numero,
                NumeroOriginal = row.NumeroOriginal,
                ClavePem = row.ClavePem,
                NombreProyecto = row.NombreProyecto,
                FuenteDocumento = row.FuenteArchivo,
                FechaCarga = row.FechaCarga,
                MetodoVinculo = metodo,
                Estado = "Vinculado y verificable",
                Mensaje = "Expediente de origen independiente de las actualizaciones y cortes de seguimiento posteriores."
            };

        private static string NormalizarClavePormenorizada(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return Regex.Replace(value
                .Replace('‐', '-').Replace('‑', '-').Replace('‒', '-')
                .Replace('–', '-').Replace('—', '-').Replace('−', '-')
                .ToUpperInvariant(), @"\s+", string.Empty).Trim();
        }

        public async Task<List<PamUsuarioDestinatario>> ObtenerDestinatariosAsync()
        {
            const string sql = @"
SELECT IdUsuario, Nombre, Correo, Cargo
FROM dgmesnie.Usuario
WHERE Vigente = 1 AND NULLIF(LTRIM(RTRIM(Correo)), N'') IS NOT NULL
  AND Correo LIKE N'%@%.%'
ORDER BY Nombre;";
            await using var connection = new SqlConnection(_connectionString);
            return (await connection.QueryAsync<PamUsuarioDestinatario>(sql)).ToList();
        }

        private async Task<PamImpactoRegional> ObtenerImpactoRegionalAsync(long proyectoId, string region)
        {
            if (string.IsNullOrWhiteSpace(region)) return null;

            const string sql = @"
SELECT
    @Region AS Region,
    COUNT(*) AS TotalProyectos,
    CAST(SUM(ISNULL(v.MontoProyectoMdp, 0)) AS DECIMAL(18,2)) AS InversionRegion,
    CAST(SUM(ISNULL(v.KmC, 0)) AS DECIMAL(18,2)) AS KmCRegion,
    CAST(SUM(ISNULL(v.Mva, 0)) AS DECIMAL(18,2)) AS MvaRegion,
    CAST(SUM(ISNULL(v.Mvar, 0)) AS DECIMAL(18,2)) AS MvarRegion,
    CAST(SUM(CASE WHEN v.ProyectoId = @ProyectoId THEN ISNULL(v.MontoProyectoMdp, 0) ELSE 0 END) AS DECIMAL(18,2)) AS InversionProyecto,
    CAST(SUM(CASE WHEN v.ProyectoId = @ProyectoId THEN ISNULL(v.KmC, 0) ELSE 0 END) AS DECIMAL(18,2)) AS KmCProyecto,
    CAST(SUM(CASE WHEN v.ProyectoId = @ProyectoId THEN ISNULL(v.Mva, 0) ELSE 0 END) AS DECIMAL(18,2)) AS MvaProyecto,
    CAST(SUM(CASE WHEN v.ProyectoId = @ProyectoId THEN ISNULL(v.Mvar, 0) ELSE 0 END) AS DECIMAL(18,2)) AS MvarProyecto
FROM dgmesnie.vw_PAMProyectoVigente v
WHERE v.EstadoVigenciaCartera = N'Vigente' AND v.GRT = @Region;";

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<PamImpactoRegional>(sql, new { ProyectoId = proyectoId, Region = region });
        }

        private async Task<long?> ObtenerProyectoIdPorClaveAsync(string clavePem)
        {
            const string sql = @"
SELECT TOP (1) v.ProyectoId
FROM dgmesnie.vw_PAMProyectoVigente v
WHERE v.ClaveProyecto = @Clave
   OR EXISTS
   (
       SELECT 1
       FROM dgmesnie.PAMProyectoClaveVersion clave
       WHERE clave.ProyectoId = v.ProyectoId
         AND clave.EsVigente = 1
         AND clave.ClaveProyecto = @Clave
   )
ORDER BY CASE WHEN v.ClaveProyecto = @Clave THEN 0 ELSE 1 END, v.ProyectoId;";

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<long?>(sql, new { Clave = clavePem });
        }

        private async Task<PamrntFichaProyecto> ObtenerFichaDesdeBaseAsync(long proyectoId)
        {
            const string sql = @"
IF OBJECT_ID(N'dgmesnie.PAMProyectoFicha', N'U') IS NOT NULL
BEGIN
    DECLARE @SqlFicha NVARCHAR(MAX) = N'
        SELECT TOP (1) FichaJson
        FROM dgmesnie.PAMProyectoFicha
        WHERE ProyectoId = @ProyectoId AND EsVigente = 1
        ORDER BY FechaRegistroUtc DESC, FichaId DESC;';
    EXEC sys.sp_executesql @SqlFicha, N'@ProyectoId BIGINT', @ProyectoId;
END
ELSE
BEGIN
    SELECT CAST(NULL AS NVARCHAR(MAX)) AS FichaJson;
END;";

            await using var connection = new SqlConnection(_connectionString);
            var json = await connection.QueryFirstOrDefaultAsync<string>(sql, new { ProyectoId = proyectoId });

            if (string.IsNullOrWhiteSpace(json)) return null;

            try { return JsonSerializer.Deserialize<PamrntFichaProyecto>(json, _jsonOptions); }
            catch (JsonException) { return null; }
        }

        private async Task<List<PamrntFichaRecurso>> ObtenerRecursosDesdeBaseAsync(long proyectoId)
        {
            const string sql = @"
IF OBJECT_ID(N'dgmesnie.PAMProyectoImagen', N'U') IS NOT NULL
BEGIN
    DECLARE @SqlImagenes NVARCHAR(MAX) = N'
        SELECT 
            ImagenId AS RecursoId,
            ProyectoId,
            ISNULL(TipoImagenCodigo, N''Diagrama'') AS TipoRecurso,
            ISNULL(FiguraTituloPAM, N''Figura técnica'') AS Titulo,
            DescripcionImagen AS Descripcion,
            UrlCDN AS Url,
            TextoAlternativo AS AltText,
            OrdenImagen AS Orden,
            CAST(1 AS BIT) AS Aplica,
            OrdenImagen,
            FiguraNumeroPAM,
            FiguraTituloPAM,
            DescripcionImagen,
            ContextoPAM,
            TextoAlternativo,
            TipoImagenCodigo,
            TipoImagenDescripcion,
            Variante,
            TipoRelacion,
            EsCompartida,
            AssetId,
            NombreArchivo,
            RutaCDN,
            UrlCDN,
            FuenteDescripcion
        FROM dgmesnie.PAMProyectoImagen
        WHERE (ProyectoId = @ProyectoId OR ClaveProyecto = (SELECT TOP 1 ClaveProyecto FROM dgmesnie.vw_PAMProyectoVigente WHERE ProyectoId = @ProyectoId))
          AND Activo = 1
        ORDER BY OrdenImagen, ImagenId;';
    EXEC sys.sp_executesql @SqlImagenes, N'@ProyectoId BIGINT', @ProyectoId;
END
ELSE IF OBJECT_ID(N'dgmesnie.PAMProyectoFichaRecurso', N'U') IS NOT NULL
BEGIN
    DECLARE @SqlRecursos NVARCHAR(MAX) = N'
        SELECT RecursoId, ProyectoId, TipoRecurso, Titulo, Descripcion, Url, AltText, Orden, Aplica
        FROM dgmesnie.PAMProyectoFichaRecurso
        WHERE ProyectoId = @ProyectoId AND Activo = 1
        ORDER BY Orden, RecursoId;';
    EXEC sys.sp_executesql @SqlRecursos, N'@ProyectoId BIGINT', @ProyectoId;
END;";

            await using var connection = new SqlConnection(_connectionString);
            var recursos = (await connection.QueryAsync<PamrntFichaRecurso>(sql, new { ProyectoId = proyectoId })).Where(x => x.Aplica).ToList();
            return recursos;
        }

        private PamrntProyectoIdentificado ConstruirProyectoIdentificado(PamProyectoDetalleActual actual)
        {
            return new PamrntProyectoIdentificado
            {
                ProyectoId = actual.ProyectoId,
                Prioridad = actual.PrioridadPrograma,
                Gcr = actual.GRT,
                ClavePem = actual.ClaveProyecto,
                ClavesAlternas = actual.ClavesAlternas,
                Proyecto = actual.NombreProyecto,
                FechaNecesaria = PrimerTexto(actual.FechaNecesaria, actual.FeoFactible, actual.FeoIndicadaOficioSener),
                EjercicioPlaneacion = actual.AnioPrograma ?? 0,
                ZonaAtendida = actual.ZonaAtendida,
                InversionMdp = actual.MontoProyectoMdp,
                FichaDisponible = true,
                TipoProyecto = actual.TipoProyecto,
                OrigenPrograma = actual.OrigenPrograma,
                EtapaProyecto = actual.EtapaProyecto,
                EstatusLicitacion = actual.EstatusLicitacion,
                EstadoVigenciaCartera = actual.EstadoVigenciaCartera,
                NumeroVersion = actual.NumeroVersion,
                FuenteDocumento = actual.FuenteDocumento,
                FechaCorte = actual.FechaCorte,
                FuenteUbicacion = actual.FuenteUbicacion
            };
        }

        private PamrntFichaProyecto ConstruirFichaDinamica(PamrntProyectoIdentificado proyecto, PamProyectoDetalleViewModel detalle)
        {
            var actual = detalle.Actual;
            var inversion = actual.MontoProyectoMdp ?? proyecto.InversionMdp ?? 0;
            var fechaNecesaria = PrimerTexto(actual.FechaNecesaria, actual.FeoFactible, actual.FeoIndicadaOficioSener, proyecto.FechaNecesaria, "Por definir");
            var etapa = PrimerTexto(actual.EtapaProyecto, proyecto.EtapaProyecto, "Sin etapa registrada");
            var zona = PrimerTexto(actual.ZonaAtendida, proyecto.ZonaAtendida, "Zona pendiente de integrar");

            return new PamrntFichaProyecto
            {
                ClavePem = PrimerTexto(actual.ClaveProyecto, proyecto.ClavePem, $"ID-{actual.ProyectoId}"),
                Titulo = PrimerTexto(actual.NombreProyecto, proyecto.Proyecto, "Proyecto PAM/PAMRNT"),
                ResumenEjecutivo = $"Proyecto del programa {actual.OrigenPrograma} de tipo {PrimerTexto(actual.TipoProyecto, "Transmisión/Transformación")} para la atención de la zona {zona}, orientado al fortalecimiento de la capacidad y confiabilidad del SEN.",
                AlternativaSeleccionada = PrimerTexto(actual.ElementosEquiposAsociados, "Alcance técnico registrado en cartera"),
                RecomendacionEjecutiva = "Dar seguimiento continuo a las etapas de desarrollo y coordinar la ejecución con la Gerencia Regional de Transmisión.",
                InversionMdp = inversion,
                FechaNecesaria = fechaNecesaria,
                FechaFactible = PrimerTexto(actual.FeoFactible, actual.FechaEstimadaInicio, fechaNecesaria),
                RelacionBeneficioCosto = 0,
                TotalObras = Math.Max(0, detalle.Relaciones.Count),
                CorredorPrincipal = zona,
                Fuente = PrimerTexto(actual.FuenteDocumento, proyecto.FuenteDocumento, "Cartera oficial PAMRNT 2026–2040"),
                Estados = ExtraerAmbito(zona),
                DiagnosticoOperativo = PrimerTexto(actual.EstadoRealProyecto, actual.CircunstanciasAtrasos, actual.ElementosEquiposAsociados, $"El proyecto se encuentra en etapa {etapa} y atiende la zona {zona}."),
                PronosticoDemanda = PrimerTexto(actual.ComentariosNivelPriorizacion, "Pronóstico de demanda integrado en los estudios de planeación del proyecto."),
                DemandaMaxima = "En integración",
                NotaEvaluacionEconomica = inversion > 0
                    ? $"Inversión vigente registrada por {inversion:N3} MDP conforme a los análisis del ejercicio de planeación."
                    : "Evaluación económica en proceso de integración en el modelo de producción.",
                ConclusionEjecutiva = PrimerTexto(actual.AccionesMitigacionCorreccion,
                    $"El proyecto mantiene seguimiento activo en el portafolio del {actual.OrigenPrograma} para garantizar la oportunidad de las obras de infraestructura."),
                MetasFisicas = ConstruirMetas(actual, detalle),
                PendientesValidacion = ConstruirPendientes(actual),
                Riesgos = ConstruirRiesgos(actual),
                Alternativas = ConstruirAlternativas(actual, inversion),
                Comparativa = ConstruirComparativa(actual, inversion, fechaNecesaria),
                ProximosPasos = ConstruirProximosPasos(detalle)
            };
        }

        private void NormalizarFicha(PamrntFichaProyecto ficha, PamrntProyectoIdentificado proyecto, PamProyectoDetalleViewModel detalle)
        {
            var actual = detalle.Actual;
            ficha.ClavePem = PrimerTexto(ficha.ClavePem, actual.ClaveProyecto, proyecto.ClavePem, $"ID-{actual.ProyectoId}");
            ficha.Titulo = PrimerTexto(ficha.Titulo, actual.NombreProyecto, proyecto.Proyecto, "Proyecto PAM/PAMRNT");
            ficha.TipoFicha = PrimerTexto(ficha.TipoFicha, "Ficha ejecutiva dinámica");
            ficha.ResumenEjecutivo = PrimerTexto(ficha.ResumenEjecutivo, $"Ficha ejecutiva generada desde la base SQL para {ficha.Titulo}.");
            ficha.AlternativaSeleccionada = PrimerTexto(ficha.AlternativaSeleccionada, actual.ElementosEquiposAsociados, "Alcance vigente registrado");
            ficha.RecomendacionEjecutiva = PrimerTexto(ficha.RecomendacionEjecutiva, "Validar y completar fuentes técnicas antes del cierre ejecutivo.");
            ficha.InversionMdp = ficha.InversionMdp > 0 ? ficha.InversionMdp : (actual.MontoProyectoMdp ?? proyecto.InversionMdp ?? 0);
            ficha.FechaNecesaria = PrimerTexto(ficha.FechaNecesaria, actual.FechaNecesaria, actual.FeoFactible, proyecto.FechaNecesaria, "Por definir");
            ficha.FechaFactible = PrimerTexto(ficha.FechaFactible, actual.FeoFactible, actual.FechaEstimadaInicio, ficha.FechaNecesaria);
            ficha.CorredorPrincipal = PrimerTexto(ficha.CorredorPrincipal, actual.ZonaAtendida, proyecto.ZonaAtendida, "Ámbito territorial en integración");
            ficha.Fuente = PrimerTexto(ficha.Fuente, actual.FuenteDocumento, proyecto.FuenteDocumento, "Repositorio histórico PAM/PAMRNT");
            ficha.Estados ??= new List<string>();
            if (ficha.Estados.Count == 0) ficha.Estados = ExtraerAmbito(PrimerTexto(actual.ZonaAtendida, proyecto.ZonaAtendida));
            ficha.MetasFisicas ??= new List<PamrntMetricaProyecto>();
            if (ficha.MetasFisicas.Count == 0) ficha.MetasFisicas = ConstruirMetas(actual, detalle);
            ficha.PendientesValidacion ??= new List<string>();
            if (ficha.PendientesValidacion.Count == 0) ficha.PendientesValidacion = ConstruirPendientes(actual);
            ficha.PuntosClave ??= new List<string>();
            if (ficha.PuntosClave.Count == 0)
            {
                ficha.PuntosClave = new List<string>
                {
                    $"Origen: {PrimerTexto(actual.OrigenPrograma, proyecto.OrigenPrograma, "PAM/PAMRNT")}",
                    $"Etapa: {PrimerTexto(actual.EtapaProyecto, proyecto.EtapaProyecto, "sin etapa")}",
                    $"Región: {PrimerTexto(actual.GRT, proyecto.Gcr, "sin región")}",
                    $"Fuente: {PrimerTexto(actual.FuenteDocumento, proyecto.FuenteDocumento, "sin fuente")}."
                };
            }
            ficha.Riesgos ??= new List<PamrntRiesgoProyecto>();
            if (ficha.Riesgos.Count == 0) ficha.Riesgos = ConstruirRiesgos(actual);
            ficha.Alternativas ??= new List<PamrntAlternativaProyecto>();
            if (ficha.Alternativas.Count == 0) ficha.Alternativas = ConstruirAlternativas(actual, ficha.InversionMdp);
            ficha.Comparativa ??= new List<PamrntComparacionProyecto>();
            if (ficha.Comparativa.Count == 0) ficha.Comparativa = ConstruirComparativa(actual, ficha.InversionMdp, ficha.FechaNecesaria);
            ficha.ProximosPasos ??= new List<string>();
            if (ficha.ProximosPasos.Count == 0) ficha.ProximosPasos = ConstruirProximosPasos(detalle);
            ficha.Recursos ??= new List<PamrntFichaRecurso>();
        }

        private static List<PamrntMetricaProyecto> ConstruirMetas(PamProyectoDetalleActual actual, PamProyectoDetalleViewModel detalle)
        {
            var metas = new List<PamrntMetricaProyecto>();
            if (actual.Mva.HasValue) metas.Add(new PamrntMetricaProyecto { Concepto = "Transformación", Valor = actual.Mva.Value.ToString("N2"), Unidad = "MVA" });
            if (actual.Mvar.HasValue) metas.Add(new PamrntMetricaProyecto { Concepto = "Compensación", Valor = actual.Mvar.Value.ToString("N2"), Unidad = "MVAr" });
            if (actual.KmC.HasValue) metas.Add(new PamrntMetricaProyecto { Concepto = "Transmisión", Valor = actual.KmC.Value.ToString("N2"), Unidad = "km-C" });
            metas.Add(new PamrntMetricaProyecto { Concepto = "Versiones", Valor = Math.Max(1, detalle.Historial.Count).ToString("N0"), Unidad = "histórico" });
            return metas;
        }

        private static List<string> ConstruirPendientes(PamProyectoDetalleActual actual)
        {
            var pendientes = new List<string>();
            if (string.IsNullOrWhiteSpace(actual.ElementosEquiposAsociados)) pendientes.Add("Completar elementos y equipos asociados.");
            if (string.IsNullOrWhiteSpace(actual.FeoFactible)) pendientes.Add("Validar fecha factible de entrada en operación.");
            pendientes.Add("Vincular diagrama unifilar, mapa geoespacial y/o lámina C7U cuando estén disponibles en la base de datos.");
            return pendientes;
        }

        private static List<PamrntRiesgoProyecto> ConstruirRiesgos(PamProyectoDetalleActual actual)
        {
            var riesgos = new List<PamrntRiesgoProyecto>();

            // 1. Riesgo real reportado: circunstancias de atraso + acción de mitigación (campos de BD).
            if (!string.IsNullOrWhiteSpace(actual.CircunstanciasAtrasos))
            {
                riesgos.Add(new PamrntRiesgoProyecto
                {
                    Riesgo = "Circunstancias de atraso reportadas",
                    Impacto = Recortar(actual.CircunstanciasAtrasos, 240),
                    Mitigacion = PrimerTexto(actual.AccionesMitigacionCorreccion,
                        "Sin acción de mitigación registrada; dar seguimiento en el próximo corte del Informe.")
                });
            }

            // 2. Riesgo de empalme con la generación, calculado de las fechas vigentes.
            var necesaria = PamFechaParser.Parsear(actual.FechaNecesaria);
            var factible = PamFechaParser.Parsear(actual.FeoFactible);
            if (necesaria.HasValue && factible.HasValue && factible.Value > necesaria.Value)
            {
                var meses = (factible.Value.Year - necesaria.Value.Year) * 12 + factible.Value.Month - necesaria.Value.Month;
                riesgos.Add(new PamrntRiesgoProyecto
                {
                    Riesgo = "Desfasamiento con la fecha necesaria",
                    Impacto = $"La red entraría {meses} mes(es) después de cuando se requiere, exponiendo a la generación asociada de la región a vertimientos.",
                    Mitigacion = "Priorizar el proceso de licitación y sincronizar el cronograma con la generación de la GCR."
                });
            }

            // 3. Riesgo desde el estado real de ejecución (campo de BD).
            if (riesgos.Count < 3 && !string.IsNullOrWhiteSpace(actual.EstadoRealProyecto))
            {
                riesgos.Add(new PamrntRiesgoProyecto
                {
                    Riesgo = "Estado de ejecución reportado",
                    Impacto = Recortar(actual.EstadoRealProyecto, 240),
                    Mitigacion = PrimerTexto(actual.ComentariosNivelPriorizacion,
                        "Mantener el seguimiento del avance en cada corte del Informe Pormenorizado.")
                });
            }

            // Respaldo sólo si el proyecto no tiene ningún campo de riesgo capturado.
            if (riesgos.Count == 0)
            {
                riesgos.Add(new PamrntRiesgoProyecto
                {
                    Riesgo = "Información de riesgos en integración",
                    Impacto = "Aún no se registran circunstancias de atraso ni estado de ejecución para este proyecto.",
                    Mitigacion = "Se poblará automáticamente cuando el Informe Pormenorizado incorpore estos campos."
                });
            }

            return riesgos;
        }

        /// <summary>Trunca respetando palabras y agrega puntos suspensivos si excede el límite.</summary>
        private static string Recortar(string texto, int limite)
        {
            if (string.IsNullOrWhiteSpace(texto)) return texto;
            var t = System.Text.RegularExpressions.Regex.Replace(texto.Trim(), @"\s+", " ");
            if (t.Length <= limite) return t;
            var corte = t.LastIndexOf(' ', Math.Min(limite, t.Length - 1));
            if (corte < limite / 2) corte = limite;
            return t[..corte].TrimEnd(',', ';', '.', ' ') + "…";
        }

        private static List<PamrntAlternativaProyecto> ConstruirAlternativas(PamProyectoDetalleActual actual, decimal inversion)
        {
            return new List<PamrntAlternativaProyecto>
            {
                new()
                {
                    Nombre = "Alcance vigente",
                    Tecnologia = PrimerTexto(actual.TipoProyecto, actual.TipoFinanciamiento, "Proyecto de transmisión"),
                    InversionMdp = inversion,
                    Alcance = PrimerTexto(actual.ElementosEquiposAsociados, actual.ZonaAtendida, "Alcance por integrar desde fuente técnica."),
                    Ventaja = "Se mantiene vinculado a la versión vigente de la base SQL.",
                    Recomendada = true
                },
                new()
                {
                    Nombre = "Pendiente de evaluación",
                    Tecnologia = "Alternativa técnica por documentar",
                    InversionMdp = 0,
                    Alcance = "Reservado para comparar alternativas cuando exista ficha técnica o minuta validada.",
                    Ventaja = "Permite mantener el formato ejecutivo sin inventar información no registrada.",
                    Recomendada = false
                }
            };
        }

        private static List<PamrntComparacionProyecto> ConstruirComparativa(PamProyectoDetalleActual actual, decimal inversion, string fechaNecesaria)
        {
            return new List<PamrntComparacionProyecto>
            {
                new() { Criterio = "Inversión", Alternativa1 = inversion > 0 ? $"{inversion:N3} MDP" : "En integración", Alternativa2 = "Por documentar", Favorable = "Alcance vigente" },
                new() { Criterio = "Fecha necesaria", Alternativa1 = PrimerTexto(fechaNecesaria, "Por definir"), Alternativa2 = "Por documentar", Favorable = "Alcance vigente" },
                new() { Criterio = "Estado", Alternativa1 = PrimerTexto(actual.EstadoVigenciaCartera, "Sin estado"), Alternativa2 = "No aplica", Favorable = "Cartera" },
                new() { Criterio = "Fuente", Alternativa1 = PrimerTexto(actual.FuenteDocumento, "Repositorio SQL"), Alternativa2 = "Por documentar", Favorable = "Trazabilidad" }
            };
        }

        private static List<string> ConstruirProximosPasos(PamProyectoDetalleViewModel detalle)
        {
            return new List<string>
            {
                "Supervisar el avance físico-financiero del proyecto conforme al programa de ejecución aprobado.",
                "Monitorear la fecha de entrada en operación (FEO) para asegurar la interconexión oportuna a la Red Nacional de Transmisión (RNT).",
                "Dar seguimiento a la emisión de oficios e instrucciones por parte de la SENER y el CENACE.",
                "Mantener la trazabilidad documental y validar las actualizaciones en el portafolio oficial del PAMRNT."
            };
        }

        private async Task<T> IntentarLeerJsonAsync<T>(string relativePath) where T : class
        {
            var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                : _environment.WebRootPath;
            var path = Path.Combine(webRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path)) return null;

            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions);
        }

        private static List<string> ExtraerAmbito(string zona)
        {
            if (string.IsNullOrWhiteSpace(zona)) return new List<string> { "Ámbito en integración" };
            var baseTexto = zona.Contains('/') ? zona.Split('/').Last() : zona;
            return baseTexto
                .Split(new[] { ',', ';', '·' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(6)
                .DefaultIfEmpty(baseTexto.Trim())
                .ToList();
        }

        private static string PrimerTexto(params string[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
        }

        private static string Normalizar(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private class PamRepositorioResumen
        {
            public int TotalPam { get; set; }
            public int TotalPamVigentes { get; set; }
            public int TotalPamrnt { get; set; }
            public int TotalVigentes { get; set; }
            public int TotalCancelados { get; set; }
            public int TotalRecientes { get; set; }
            public DateTime? FechaCorteMaxima { get; set; }
            public decimal TotalKmC { get; set; }
            public decimal TotalMva { get; set; }
            public decimal TotalMvar { get; set; }
            public decimal TotalInversionVigente { get; set; }
            public decimal TotalInversionPamrnt { get; set; }
            public int ProyectosConMetricas { get; set; }
        }

        private class FichaJsonRow
        {
            public string FichaJson { get; set; }
        }

        private class ExpedientePormenorizadoRow
        {
            public int ProyectoModernizacionId { get; set; }
            public int? Numero { get; set; }
            public int? NumeroOriginal { get; set; }
            public string ClavePem { get; set; }
            public string NombreProyecto { get; set; }
            public string FuenteArchivo { get; set; }
            public DateTime? FechaCarga { get; set; }
        }
    }
}
