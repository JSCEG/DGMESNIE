using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NSIE.Models;

namespace NSIE.Controllers
{
    [Route("GraphProbe")]
    public class GraphProbeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public GraphProbeController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet("SharePointMinimo")]
        public async Task<IActionResult> SharePointMinimo()
        {
            var settings = _configuration.GetSection("GraphProbe").Get<GraphProbeSettings>() ?? new GraphProbeSettings();

            if (string.IsNullOrWhiteSpace(settings.AccessToken) ||
                string.IsNullOrWhiteSpace(settings.Hostname) ||
                string.IsNullOrWhiteSpace(settings.SitePath) ||
                string.IsNullOrWhiteSpace(settings.DriveFolderPath))
            {
                return BadRequest(new SharePointGraphProbeResult
                {
                    Success = false,
                    Message = "Falta configurar GraphProbe: AccessToken, Hostname, SitePath y DriveFolderPath.",
                    RequestUrl = "GET /GraphProbe/SharePointMinimo"
                });
            }

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.AccessToken);

            try
            {
                var siteRequest = $"sites/{settings.Hostname}:/{settings.SitePath}";
                var siteResponse = await client.GetAsync(siteRequest);
                var siteBody = await siteResponse.Content.ReadAsStringAsync();

                if (!siteResponse.IsSuccessStatusCode)
                {
                    return StatusCode((int)siteResponse.StatusCode, new SharePointGraphProbeResult
                    {
                        Success = false,
                        Message = "Graph no pudo resolver el sitio de SharePoint.",
                        RequestUrl = siteRequest,
                        RawError = siteBody
                    });
                }

                using var siteJson = JsonDocument.Parse(siteBody);
                var siteId = siteJson.RootElement.GetProperty("id").GetString() ?? string.Empty;
                var siteName = siteJson.RootElement.TryGetProperty("displayName", out var siteNameNode)
                    ? siteNameNode.GetString()
                    : string.Empty;
                var webUrl = siteJson.RootElement.TryGetProperty("webUrl", out var webUrlNode)
                    ? webUrlNode.GetString()
                    : string.Empty;

                var folderRequest = $"sites/{siteId}/drive/root:/{settings.DriveFolderPath}:/children";
                var folderResponse = await client.GetAsync(folderRequest);
                var folderBody = await folderResponse.Content.ReadAsStringAsync();

                if (!folderResponse.IsSuccessStatusCode)
                {
                    return StatusCode((int)folderResponse.StatusCode, new SharePointGraphProbeResult
                    {
                        Success = false,
                        Message = "Graph resolvió el sitio, pero no pudo listar la carpeta objetivo.",
                        SiteId = siteId,
                        SiteName = siteName,
                        WebUrl = webUrl,
                        RequestUrl = folderRequest,
                        RawError = folderBody
                    });
                }

                using var folderJson = JsonDocument.Parse(folderBody);
                var result = new SharePointGraphProbeResult
                {
                    Success = true,
                    Message = "Graph devolvió la lista de elementos de la carpeta objetivo.",
                    SiteId = siteId,
                    SiteName = siteName,
                    WebUrl = webUrl,
                    RequestUrl = folderRequest
                };

                if (folderJson.RootElement.TryGetProperty("value", out var valueNode) && valueNode.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in valueNode.EnumerateArray())
                    {
                        result.Items.Add(new SharePointGraphProbeItem
                        {
                            Name = item.TryGetProperty("name", out var nameNode) ? nameNode.GetString() : string.Empty,
                            WebUrl = item.TryGetProperty("webUrl", out var itemUrlNode) ? itemUrlNode.GetString() : string.Empty,
                            IsFolder = item.TryGetProperty("folder", out _),
                            LastModifiedDateTime = item.TryGetProperty("lastModifiedDateTime", out var modifiedNode)
                                ? modifiedNode.GetString()
                                : string.Empty
                        });
                    }
                }

                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new SharePointGraphProbeResult
                {
                    Success = false,
                    Message = "Ocurrió un error al ejecutar la prueba mínima de Graph.",
                    RawError = ex.Message,
                    RequestUrl = "GET /GraphProbe/SharePointMinimo"
                });
            }
        }
    }
}