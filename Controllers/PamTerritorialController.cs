using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using NSIE.Models;
using NSIE.Servicios;

namespace NSIE.Controllers;

[ApiController]
[Route("DashboardProyectos/PamTerritorial")]
public sealed class PamTerritorialController : ControllerBase
{
    private readonly IPamTerritorialService _service;
    private readonly IPamRedAssociationService _redAssociationService;
    private readonly IPamConvocatoriaEvidenceService _convocatoriaEvidenceService;
    private readonly IPamConvocatoriaValidationService _convocatoriaValidationService;
    private readonly IPoliticaAccesoPermisosEnergeticos _accessPolicy;
    private readonly ILogger<PamTerritorialController> _logger;

    public PamTerritorialController(
        IPamTerritorialService service,
        IPamRedAssociationService redAssociationService,
        IPamConvocatoriaEvidenceService convocatoriaEvidenceService,
        IPamConvocatoriaValidationService convocatoriaValidationService,
        IPoliticaAccesoPermisosEnergeticos accessPolicy,
        ILogger<PamTerritorialController> logger)
    {
        _service = service;
        _redAssociationService = redAssociationService;
        _convocatoriaEvidenceService = convocatoriaEvidenceService;
        _convocatoriaValidationService = convocatoriaValidationService;
        _accessPolicy = accessPolicy;
        _logger = logger;
    }

    [HttpGet]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(
        typeof(IReadOnlyList<PamTerritorialProyecto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PamTerritorialProyecto>>> Buscar(
        [FromQuery] string? busqueda = null,
        [FromQuery] int limite = 500,
        CancellationToken cancellationToken = default)
    {
        if (busqueda is { Length: > 160 } || limite is < 1 or > 500)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Parámetros de búsqueda PAM no válidos",
                detail: "La búsqueda admite hasta 160 caracteres y el límite debe estar entre 1 y 500.");
        }

        try
        {
            return Ok(await _service.BuscarAsync(
                busqueda,
                limite,
                cancellationToken));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Falló la consulta del índice territorial PAM.");
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Fuente PAM no disponible",
                detail: "No fue posible consultar el catálogo territorial PAM en este momento.");
        }
    }

    [HttpGet("{proyectoId:long}")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(typeof(PamTerritorialProyecto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PamTerritorialProyecto>> Obtener(
        long proyectoId,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await _service.ObtenerAsync(proyectoId, cancellationToken);
            return project is null
                ? Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Proyecto PAM no encontrado")
                : Ok(project);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (SqlException ex)
        {
            _logger.LogError(
                ex,
                "Falló la consulta territorial del proyecto PAM {ProyectoId}.",
                proyectoId);
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Fuente PAM no disponible",
                detail: "No fue posible consultar el proyecto PAM en este momento.");
        }
    }

    [HttpGet("{proyectoId:long}/AsociacionesRed")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(typeof(PamRedAssociationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PamRedAssociationResult>> AsociacionesRed(
        long proyectoId,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await _service.ObtenerAsync(proyectoId, cancellationToken);
            if (project is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Proyecto PAM no encontrado");
            }

            var result = (await _redAssociationService.ResolverAsync(
                new[] { project },
                cancellationToken)).First();
            return Ok(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (
            ex is SqlException or
            HttpRequestException or
            System.Text.Json.JsonException)
        {
            _logger.LogError(
                ex,
                "Falló el análisis de asociaciones de red del proyecto PAM {ProyectoId}.",
                proyectoId);
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Catálogo eléctrico no disponible",
                detail: "No fue posible contrastar el proyecto PAM con las subestaciones y líneas en este momento.");
        }
    }

    [HttpGet("{proyectoId:long}/EvidenciaConvocatoria")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(
        typeof(PamConvocatoriaEvidenceResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PamConvocatoriaEvidenceResult>> EvidenciaConvocatoria(
        long proyectoId,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await _service.ObtenerAsync(proyectoId, cancellationToken);
            if (project is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Proyecto PAM no encontrado");
            }

            var result = (await _convocatoriaEvidenceService.ResolverPamAsync(
                new[] { project },
                cancellationToken)).First();
            return Ok(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (
            ex is SqlException or
            HttpRequestException or
            InvalidDataException or
            TaskCanceledException or
            System.Text.Json.JsonException)
        {
            _logger.LogError(
                ex,
                "Falló el cruce del proyecto PAM {ProyectoId} con Segunda Convocatoria.",
                proyectoId);
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Segunda Convocatoria no disponible",
                detail: "No fue posible contrastar el proyecto PAM con la fuente publicada en este momento.");
        }
    }

    [HttpGet("EvidenciaConvocatoria/Resumen")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(
        typeof(PamConvocatoriaCoverageReport),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<PamConvocatoriaCoverageReport>>
        ResumenEvidenciaConvocatoria(
            CancellationToken cancellationToken,
            [FromQuery] bool refrescar = false)
    {
        try
        {
            return Ok(await _convocatoriaEvidenceService.ObtenerCoberturaAsync(
                cancellationToken,
                refrescar));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (
            ex is HttpRequestException or
            InvalidDataException or
            TaskCanceledException or
            System.Text.Json.JsonException)
        {
            _logger.LogError(
                ex,
                "Falló el diagnóstico de cobertura de red con Segunda Convocatoria.");
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Segunda Convocatoria no disponible",
                detail: "No fue posible generar el diagnóstico de subestaciones y líneas en este momento.");
        }
    }

    [HttpGet("EvidenciaConvocatoria/Validaciones")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(
        typeof(PamConvocatoriaValidationSnapshot),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<PamConvocatoriaValidationSnapshot>>
        ValidacionesEvidenciaConvocatoria(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _convocatoriaValidationService.ObtenerActualesAsync(
                cancellationToken));
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (SqlException ex)
        {
            _logger.LogError(
                ex,
                "Falló la consulta de validaciones de Segunda Convocatoria.");
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Registro de validaciones no disponible",
                detail: "No fue posible consultar las decisiones humanas en este momento.");
        }
    }

    [HttpPost("EvidenciaConvocatoria/Validaciones")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(
        typeof(PamConvocatoriaValidationRecord),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PamConvocatoriaValidationRecord>>
        GuardarValidacionEvidenciaConvocatoria(
            [FromBody] PamConvocatoriaValidationCommand command,
            CancellationToken cancellationToken)
    {
        var profile = ObtenerPerfilUsuario();
        if (profile is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Sesión requerida",
                detail: "Inicia sesión para registrar una decisión de validación.");
        }
        if (!PuedeValidarEvidencia(profile))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Validación no autorizada",
                detail: "La validación de red está reservada a cuentas institucionales autorizadas.");
        }

        var candidateId = command?.CandidatoId?.Trim() ?? string.Empty;
        var type = command?.TipoElemento?.Trim().ToLowerInvariant() ??
            string.Empty;
        var decision = command?.Decision?.Trim().ToLowerInvariant() ??
            string.Empty;
        var observation = command?.Observacion?.Trim() ?? string.Empty;
        if (candidateId.Length is < 10 or > 80 ||
            !candidateId.StartsWith("C2-", StringComparison.Ordinal) ||
            !string.Equals(type, "subestacion", StringComparison.Ordinal) ||
            !PamConvocatoriaValidationDecisions.Permitidas.Contains(decision) ||
            observation.Length is < 5 or > 2000)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Decisión de validación no válida",
                detail: "Selecciona una subestación, una decisión permitida y escribe una observación de 5 a 2000 caracteres.");
        }

        try
        {
            var coverage =
                await _convocatoriaEvidenceService.ObtenerCoberturaAsync(
                    cancellationToken);
            var candidate = coverage.Candidatos.FirstOrDefault(item =>
                string.Equals(
                    item.CandidatoId,
                    candidateId,
                    StringComparison.Ordinal));
            if (candidate is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Candidato no encontrado",
                    detail: "La evidencia cambió o el candidato ya no forma parte del diagnóstico actual.");
            }
            if (!string.Equals(
                    candidate.TipoElemento,
                    type,
                    StringComparison.Ordinal))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Tipo de candidato inconsistente");
            }

            var actor = CrearActorValidacion(profile);
            return Ok(await _convocatoriaValidationService.GuardarAsync(
                candidate,
                decision,
                observation,
                actor,
                cancellationToken));
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "La decisión no corresponde al candidato",
                detail: ex.Message);
        }
        catch (SqlException ex)
        {
            _logger.LogError(
                ex,
                "Falló el registro de validación {CandidateId}.",
                candidateId);
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "No fue posible guardar la validación",
                detail: "La decisión no se registró. Inténtalo nuevamente.");
        }
    }

    [HttpPost(
        "EvidenciaConvocatoria/Validaciones/ConfirmarCoincidenciasAutomaticas")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(
        typeof(PamConvocatoriaBulkValidationResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PamConvocatoriaBulkValidationResult>>
        ConfirmarCoincidenciasAutomaticas(
            CancellationToken cancellationToken,
            [FromQuery] string tipoElemento = "subestacion")
    {
        var normalizedType = (tipoElemento ?? string.Empty)
            .Trim()
            .ToLowerInvariant();
        if (normalizedType is not ("subestacion" or "linea_transmision"))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Fase de validación no válida",
                detail: "Usa tipoElemento=subestacion o tipoElemento=linea_transmision.");
        }

        var profile = ObtenerPerfilUsuario();
        if (profile is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Sesión requerida",
                detail: "Inicia sesión para confirmar coincidencias automáticas.");
        }
        if (!PuedeValidarEvidencia(profile))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Validación no autorizada",
                detail: "La validación de red está reservada a cuentas institucionales autorizadas.");
        }

        try
        {
            var coverage =
                await _convocatoriaEvidenceService.ObtenerCoberturaAsync(
                    cancellationToken);
            return Ok(await _convocatoriaValidationService
                .ConfirmarCoincidenciasAutomaticasAsync(
                    coverage.Candidatos,
                    normalizedType,
                    CrearActorValidacion(profile),
                    cancellationToken));
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (
            ex is SqlException or
            HttpRequestException or
            InvalidDataException or
            TaskCanceledException or
            System.Text.Json.JsonException)
        {
            _logger.LogError(
                ex,
                "Falló la confirmación automática en lote de Segunda Convocatoria.");
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "No fue posible confirmar las coincidencias",
                detail: "El lote no se registró. Verifica las fuentes e inténtalo nuevamente.");
        }
    }

    [HttpGet("EvidenciaConvocatoria/GeoJson")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [Produces("application/geo+json", "application/json")]
    [ProducesResponseType(
        typeof(PamConvocatoriaGeoJson),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<PamConvocatoriaGeoJson>>
        GeoJsonEvidenciaConvocatoria(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _convocatoriaEvidenceService.ObtenerCoberturaGeoJsonAsync(
                cancellationToken));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (
            ex is HttpRequestException or
            InvalidDataException or
            TaskCanceledException or
            System.Text.Json.JsonException)
        {
            _logger.LogError(
                ex,
                "Falló la capa de evidencia de Segunda Convocatoria.");
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Segunda Convocatoria no disponible",
                detail: "No fue posible construir la capa de evidencia en este momento.");
        }
    }

    private PerfilUsuario? ObtenerPerfilUsuario()
    {
        var json = HttpContext.Session.GetString("PerfilUsuario");
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<PerfilUsuario>(json);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex,
                "No fue posible interpretar el perfil para validar evidencia Conv2.");
            return null;
        }
    }

    private bool PuedeValidarEvidencia(PerfilUsuario profile)
    {
        if (string.Equals(
                profile.Rol,
                "Administrador",
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                profile.Rol_Nombre,
                "Administrador",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return _accessPolicy.Resolver(profile).EsInstitucional;
    }

    private static PamConvocatoriaValidationActor CrearActorValidacion(
        PerfilUsuario profile) =>
        new()
        {
            UsuarioId = int.TryParse(profile.IdUsuario, out var userId)
                ? userId
                : null,
            Nombre = string.IsNullOrWhiteSpace(profile.Nombre)
                ? profile.Correo
                : profile.Nombre.Trim(),
            Unidad = profile.Unidad_de_Adscripcion?.Trim() ?? string.Empty
        };

    [HttpGet("GeoJson")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [Produces("application/geo+json", "application/json")]
    [ProducesResponseType(typeof(PamTerritorialGeoJson), StatusCodes.Status200OK)]
    public async Task<ActionResult<PamTerritorialGeoJson>> GeoJson(
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.ObtenerGeoJsonAsync(cancellationToken));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Falló la consulta GeoJSON de proyectos PAM.");
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Fuente PAM no disponible",
                detail: "No fue posible consultar las ubicaciones PAM en este momento.");
        }
    }
}
