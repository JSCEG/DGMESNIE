using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace NSIE.Servicios;

/// <summary>
/// Ligas oficiales de un permiso en el registro público de la CNE: documento del permiso,
/// resolución de otorgamiento y demás resoluciones.
/// Fuente principal: dbo.Ligas_Permisos_CNE (cargada desde el registro CNE). Si el permiso
/// no está ahí, se busca su PermisoId en la API pública de la CNE y se arma la liga del
/// visor de documentos (drive.cne.gob.mx).
/// </summary>
public interface IPermisoCneDocumentoServicio
{
    Task<LigasPermisoCne?> ObtenerLigasAsync(string numeroPermiso, CancellationToken cancellationToken);

    Task<string?> ObtenerUrlDocumentoAsync(string numeroPermiso, CancellationToken cancellationToken);
}

public sealed class LigasPermisoCne
{
    public string NumeroPermiso { get; init; } = string.Empty;
    public string? LigaPermiso { get; init; }
    public string? LigaResolucion { get; init; }
    public IReadOnlyList<ResolucionPermisoCne> Resoluciones { get; init; } = Array.Empty<ResolucionPermisoCne>();
    public DateTime? FechaActualizacion { get; init; }
    public string Fuente { get; init; } = "CNE";
}

public sealed class ResolucionPermisoCne
{
    public string Numero { get; init; } = string.Empty;
    public string Liga { get; init; } = string.Empty;
}

public sealed partial class PermisoCneDocumentoServicio : IPermisoCneDocumentoServicio
{
    public const string UrlBusquedaCne = "https://www.cne.gob.mx/Permisos/index.html";
    private const string UrlApi = "https://api-creweb.cne.gob.mx/api/Permisos/ObtenerPermisosPaginados";
    private const string UrlDocumento = "https://drive.cne.gob.mx/Drive/ObtenerPermiso/?id=";
    private const string CacheLigas = "cne:ligas-permisos";
    private static readonly string[] Columnas = { "Numero", "Estado", "Persona", "AliasProyecto", "ResolucionesAsociadas", "AnexosAsociados" };
    private static readonly SemaphoreSlim CandadoLigas = new(1, 1);

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PermisoCneDocumentoServicio> _logger;
    private readonly string _connectionString;

    public PermisoCneDocumentoServicio(HttpClient http, IMemoryCache cache, IConfiguration configuration, ILogger<PermisoCneDocumentoServicio> logger)
    {
        _http = http;
        _cache = cache;
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    }

    /// <summary>
    /// Normaliza un número de permiso para cruzarlo con Ligas_Permisos_CNE.NumeroPermisoNorm:
    /// mayúsculas, sin espacios, sin prefijo "CNE/" y sin ceros a la izquierda en cada segmento
    /// numérico (E/046/GEN/2016 → E/46/GEN/2016). Comparar sólo con UPPER(TRIM()) pierde
    /// entre 3 % y 12 % de los permisos.
    /// </summary>
    public static string NormalizarNumeroPermiso(string? numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
        {
            return string.Empty;
        }

        var s = new string(numero.ToUpperInvariant().Where(c => !char.IsWhiteSpace(c)).ToArray());
        if (s.StartsWith("CNE/", StringComparison.Ordinal))
        {
            s = s[4..];
        }

        return string.Join('/', s.Split('/').Select(seg =>
            seg.Length > 0 && seg.All(char.IsDigit) ? (seg.TrimStart('0') is { Length: > 0 } t ? t : "0") : seg));
    }

    public async Task<LigasPermisoCne?> ObtenerLigasAsync(string numeroPermiso, CancellationToken cancellationToken)
    {
        var norm = NormalizarNumeroPermiso(numeroPermiso);
        if (norm.Length == 0)
        {
            return null;
        }

        var indice = await ObtenerIndiceLigasAsync(cancellationToken);
        if (indice.TryGetValue(norm, out var ligas))
        {
            return ligas;
        }

        // Gasolineras con el número corto: PL/n/ES/aaaa → PL/n/EXP/ES/aaaa.
        var corto = PermisoPetroliferoCorto().Match(norm);
        if (corto.Success && indice.TryGetValue($"PL/{corto.Groups[1].Value}/EXP/ES/{corto.Groups[2].Value}", out ligas))
        {
            return ligas;
        }

        return null;
    }

    public async Task<string?> ObtenerUrlDocumentoAsync(string numeroPermiso, CancellationToken cancellationToken)
    {
        var numero = (numeroPermiso ?? string.Empty).Trim();
        if (numero.Length == 0)
        {
            return null;
        }

        var ligas = await ObtenerLigasAsync(numero, cancellationToken);
        if (!string.IsNullOrWhiteSpace(ligas?.LigaPermiso))
        {
            return ligas.LigaPermiso;
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

    private async Task<IReadOnlyDictionary<string, LigasPermisoCne>> ObtenerIndiceLigasAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheLigas, out IReadOnlyDictionary<string, LigasPermisoCne>? indice) && indice is not null)
        {
            return indice;
        }

        await CandadoLigas.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(CacheLigas, out indice) && indice is not null)
            {
                return indice;
            }

            var resultado = new Dictionary<string, LigasPermisoCne>(StringComparer.Ordinal);
            try
            {
                const string sql = @"
IF OBJECT_ID(N'dbo.Ligas_Permisos_CNE', N'U') IS NOT NULL
    EXEC(N'SELECT NumeroPermiso, NumeroPermisoNorm, LigaPermiso, LigaResolucionOtorgamiento, LigasResoluciones, NumerosResolucion, TRY_CONVERT(datetime2, FechaActualizacion) AS FechaActualizacion FROM dbo.Ligas_Permisos_CNE');";
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync(CancellationToken.None);
                var filas = await connection.QueryAsync<FilaLigas>(new CommandDefinition(sql, commandTimeout: 120));
                foreach (var f in filas)
                {
                    var norm = NormalizarNumeroPermiso(string.IsNullOrWhiteSpace(f.NumeroPermisoNorm) ? f.NumeroPermiso : f.NumeroPermisoNorm);
                    if (norm.Length == 0 || resultado.ContainsKey(norm))
                    {
                        continue;
                    }

                    var ligas = Partir(f.LigasResoluciones);
                    var numeros = Partir(f.NumerosResolucion);
                    var resoluciones = ligas
                        .Select((liga, i) => new ResolucionPermisoCne { Liga = liga, Numero = i < numeros.Count ? numeros[i] : $"Resolución {i + 1}" })
                        .ToList();
                    resultado[norm] = new LigasPermisoCne
                    {
                        NumeroPermiso = (f.NumeroPermiso ?? string.Empty).Trim(),
                        LigaPermiso = Vacio(f.LigaPermiso),
                        LigaResolucion = Vacio(f.LigaResolucionOtorgamiento) ?? resoluciones.FirstOrDefault()?.Liga,
                        Resoluciones = resoluciones,
                        FechaActualizacion = HoraMexico.DesdeUtc(f.FechaActualizacion)
                    };
                }
            }
            catch (SqlException ex)
            {
                _logger.LogWarning(ex, "No se pudo leer dbo.Ligas_Permisos_CNE.");
            }

            indice = resultado;
            _cache.Set(CacheLigas, indice, TimeSpan.FromMinutes(resultado.Count > 0 ? 30 : 5));
            return indice;
        }
        finally
        {
            CandadoLigas.Release();
        }
    }

    private static List<string> Partir(string? valor) =>
        (valor ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static string? Vacio(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

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

    [GeneratedRegex(@"^PL/(\d+)/ES/(\d{4})$")]
    private static partial Regex PermisoPetroliferoCorto();

    private sealed class FilaLigas
    {
        public string? NumeroPermiso { get; init; }
        public string? NumeroPermisoNorm { get; init; }
        public string? LigaPermiso { get; init; }
        public string? LigaResolucionOtorgamiento { get; init; }
        public string? LigasResoluciones { get; init; }
        public string? NumerosResolucion { get; init; }
        public DateTime? FechaActualizacion { get; init; }
    }
}

/// <summary>Las fechas de las tablas de carga CNE vienen en UTC; se muestran en hora de la CDMX.</summary>
public static class HoraMexico
{
    private static readonly TimeZoneInfo Zona = Buscar();

    private static TimeZoneInfo Buscar()
    {
        foreach (var id in new[] { "America/Mexico_City", "Central Standard Time (Mexico)" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); } catch (TimeZoneNotFoundException) { } catch (InvalidTimeZoneException) { }
        }

        // México no usa horario de verano desde 2022.
        return TimeZoneInfo.CreateCustomTimeZone("CDMX", TimeSpan.FromHours(-6), "Ciudad de México", "Ciudad de México");
    }

    public static DateTime? DesdeUtc(DateTime? utc) =>
        utc is null ? null : TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc.Value, DateTimeKind.Utc), Zona);
}
