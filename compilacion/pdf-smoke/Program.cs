using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using NSIE.Models.ProyectosPrivados;
using NSIE.Servicios;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var jsonPath = Path.Combine(root, "wwwroot", "data", "cartera-convocatoria-20260805.json");
var json = await File.ReadAllTextAsync(jsonPath);
var seed = JsonConvert.DeserializeObject<CarteraConvocatoriaSeed>(json)
    ?? throw new InvalidOperationException("No se pudo leer la semilla.");
var data = new CarteraConvocatoriaDatos
{
    SourceVersion = seed.SourceVersion,
    Source = seed.Source,
    Projects = seed.Projects,
    Notes = seed.Notes,
};
var outputDirectory = Path.Combine(root, "output", "pdf");
Directory.CreateDirectory(outputDirectory);
var output = Path.Combine(outputDirectory, "SENER_Cartera_Estrategicos_Segunda_Convocatoria_20260805.pdf");
var bytes = CarteraConvocatoriaPdfService.Generar(data, Path.Combine(root, "wwwroot"), new DateTime(2026, 8, 5));
await File.WriteAllBytesAsync(output, bytes);
Console.WriteLine(output);
Console.WriteLine($"bytes={bytes.Length}");

string? connectionString = null;
foreach (var settingsName in new[] { "appsettings.Development.json", "appsettings.json" })
{
    var settingsPath = Path.Combine(root, settingsName);
    if (!File.Exists(settingsPath)) continue;
    var settings = JObject.Parse(await File.ReadAllTextAsync(settingsPath));
    connectionString ??= settings.SelectToken("ConnectionStrings.DefaultConnection")?.Value<string>();
}
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("No se encontró la conexión de la aplicación.");

var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["ConnectionStrings:DefaultConnection"] = connectionString,
    })
    .Build();
var repository = new RepositorioProyectosPrivados(configuration);
await repository.SincronizarCarteraConvocatoriaAsync(seed, "Actualización PDF 2026-08-05");
var synchronized = await repository.ObtenerCarteraConvocatoriaAsync();
var classes = synchronized.Projects
    .GroupBy(project => project.AnalysisClassification)
    .OrderBy(group => group.Key)
    .ToDictionary(group => group.Key ?? "Sin clasificación", group => group.Count());
Console.WriteLine(JsonConvert.SerializeObject(new
{
    sourceVersion = synchronized.SourceVersion,
    projects = synchronized.Projects.Count,
    mw = synchronized.Projects.Sum(project => project.Mw),
    classifications = classes,
    comments = synchronized.Notes.Count,
    manualStates = synchronized.Projects.Count(project => project.Decision is "continua" or "no-continua"),
}, Formatting.Indented));
