using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models;

namespace NSIE.Servicios
{
    public interface IPamrntProyectosIdentificadosService
    {
        Task<PamrntProyectosIdentificadosViewModel> ObtenerProyectosAsync();
        Task<PamrntFichaProyectoViewModel> ObtenerFichaAsync(string clavePem);
    }

    public class PamrntProyectosIdentificadosService : IPamrntProyectosIdentificadosService
    {
        private const string RutaProyectos = "data/pamrnt/proyectos-identificados-2026-2040.json";
        private const string RutaFichaI26 = "data/pamrnt/ficha-i26-pe1.json";

        private readonly IWebHostEnvironment _environment;
        private readonly string _connectionString;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PamrntProyectosIdentificadosService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<PamrntProyectosIdentificadosViewModel> ObtenerProyectosAsync()
        {
            var model = await LeerJsonAsync<PamrntProyectosIdentificadosViewModel>(RutaProyectos);
            var claves = model.Proyectos.Select(x => x.ClavePem).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                model.CruceBaseDisponible = false;
                model.AvisoCruce = "La conexión del Informe Pormenorizado no está configurada. Se muestran únicamente los datos del PAMRNT.";
                return model;
            }

            try
            {
                const string sql = @"
SELECT ProyectoModernizacionId, ClavePem, EtapaProyecto, MontoProyectoMdp, FechaCarga
FROM dgmesnie.InformePormenorizadoModernizacion
WHERE Activo = 1 AND ClavePem IN @Claves;";

                await using var connection = new SqlConnection(_connectionString);
                var registros = (await connection.QueryAsync<PamrntCruceBase>(sql, new { Claves = claves }))
                    .Where(x => !string.IsNullOrWhiteSpace(x.ClavePem))
                    .GroupBy(x => x.ClavePem, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

                foreach (var proyecto in model.Proyectos)
                {
                    if (registros.TryGetValue(proyecto.ClavePem, out var registro))
                    {
                        proyecto.EnBaseInformePormenorizado = true;
                        proyecto.ProyectoModernizacionId = registro.ProyectoModernizacionId;
                        proyecto.EtapaBase = registro.EtapaProyecto;
                        proyecto.MontoBaseMdp = registro.MontoProyectoMdp;
                        proyecto.FechaCargaBase = registro.FechaCarga;
                    }
                    else
                    {
                        proyecto.EnBaseInformePormenorizado = false;
                    }
                }

                model.CruceBaseDisponible = true;
                model.AvisoCruce = "Cruce por clave PEM contra dgmesnie.InformePormenorizadoModernizacion.";
            }
            catch (SqlException)
            {
                model.CruceBaseDisponible = false;
                model.AvisoCruce = "No fue posible consultar el Informe Pormenorizado. La tabla conserva los datos oficiales del PAMRNT.";
            }

            return model;
        }

        public async Task<PamrntFichaProyectoViewModel> ObtenerFichaAsync(string clavePem)
        {
            if (!string.Equals(clavePem, "I26-PE1", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var listado = await ObtenerProyectosAsync();
            var proyecto = listado.Proyectos.FirstOrDefault(x => string.Equals(x.ClavePem, clavePem, StringComparison.OrdinalIgnoreCase));
            if (proyecto == null)
            {
                return null;
            }

            return new PamrntFichaProyectoViewModel
            {
                Proyecto = proyecto,
                Ficha = await LeerJsonAsync<PamrntFichaProyecto>(RutaFichaI26)
            };
        }

        private async Task<T> LeerJsonAsync<T>(string relativePath)
        {
            var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                : _environment.WebRootPath;
            var path = Path.Combine(webRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("No se encontró la fuente de datos PAMRNT.", path);
            }

            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions)
                ?? throw new InvalidOperationException("La fuente de datos PAMRNT no contiene información utilizable.");
        }

        private class PamrntCruceBase
        {
            public int ProyectoModernizacionId { get; set; }
            public string ClavePem { get; set; }
            public string EtapaProyecto { get; set; }
            public decimal? MontoProyectoMdp { get; set; }
            public DateTime? FechaCarga { get; set; }
        }
    }
}
