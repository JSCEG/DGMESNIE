using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NSIE.Servicios;

/// <summary>
/// Liga al documento oficial de un permiso en el registro público de la CNE.
/// La CNE no publica una URL por número de permiso: se busca el número en su API
/// (api-creweb) para obtener el PermisoId interno y con él se arma la liga de su
/// visor de documentos (drive.cne.gob.mx). El PermisoId se guarda en caché 24 h.
/// </summary>
public interface IPermisoCneDocumentoServicio
{
    Task<string?> ObtenerUrlDocumentoAsync(string numeroPermiso, CancellationToken cancellationToken);
}

public sealed class PermisoCneDocumentoServicio : IPermisoCneDocumentoServicio
{
    public const string UrlBusquedaCne = "https://www.cne.gob.mx/Permisos/index.html";
    private const string UrlApi = "https://api-creweb.cne.gob.mx/api/Permisos/ObtenerPermisosPaginados";
    private const string UrlDocumento = "https://drive.cne.gob.mx/Drive/ObtenerPermiso/?id=";
    private static readonly string[] Columnas = { "Numero", "Estado", "Persona", "AliasProyecto", "ResolucionesAsociadas", "AnexosAsociados" };

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PermisoCneDocumentoServicio> _logger;

    public PermisoCneDocumentoServicio(HttpClient http, IMemoryCache cache, ILogger<PermisoCneDocumentoServicio> logger)
    {
        _http = http;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string?> ObtenerUrlDocumentoAsync(string numeroPermiso, CancellationToken cancellationToken)
    {
        var numero = (numeroPermiso ?? string.Empty).Trim();
        if (numero.Length == 0)
        {
            return null;
        }

        var clave = "cne:permiso-id:" + numero.ToUpperInvariant();
        if (!_cache.TryGetValue(clave, out long? permisoId))
        {
            permisoId = await BuscarPermisoIdAsync(numero, cancellationToken);
            _cache.Set(clave, permisoId, permisoId is null ? TimeSpan.FromMinutes(30) : TimeSpan.FromHours(24));
        }

        if (permisoId is null)
        {
            return null;
        }

        // El visor sólo lee el PermisoId del cuarto segmento del GUID; los demás son relleno.
        var guid = $"00000000-0000-4000-{permisoId}-000000000000";
        return UrlDocumento + Convert.ToBase64String(Encoding.UTF8.GetBytes(guid));
    }

    private async Task<long?> BuscarPermisoIdAsync(string numero, CancellationToken cancellationToken)
    {
        var columnas = new JsonArray(Columnas.Select(c => (JsonNode)new JsonObject
        {
            ["data"] = c,
            ["name"] = "",
            ["searchable"] = true,
            ["orderable"] = false,
            ["search"] = new JsonObject { ["value"] = c == "Numero" ? numero : "", ["regex"] = false }
        }).ToArray());
        var cuerpo = new JsonObject
        {
            ["parameters"] = new JsonObject
            {
                ["draw"] = 1,
                ["columns"] = columnas,
                ["order"] = new JsonArray(),
                ["start"] = 0,
                ["length"] = 10,
                ["search"] = new JsonObject { ["value"] = "", ["regex"] = false }
            }
        };

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, UrlApi)
            {
                Content = new StringContent(cuerpo.ToJsonString(), Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("Origin", "https://www.cne.gob.mx");
            request.Headers.Referrer = new Uri("https://www.cne.gob.mx/");
            using var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("La CNE respondió {Status} al buscar el permiso {Numero}.", (int)response.StatusCode, numero);
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var json = await JsonNode.ParseAsync(stream, cancellationToken: cancellationToken);
            var fila = (json?["data"] as JsonArray ?? new JsonArray())
                .OfType<JsonObject>()
                .FirstOrDefault(f => string.Equals(f["Numero"]?.GetValue<string>()?.Trim(), numero, StringComparison.OrdinalIgnoreCase));
            var id = fila?["PermisoId"];
            return id is JsonValue v && v.TryGetValue<long>(out var permisoId) ? permisoId : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "No se pudo consultar la CNE para el permiso {Numero}.", numero);
            return null;
        }
    }
}
