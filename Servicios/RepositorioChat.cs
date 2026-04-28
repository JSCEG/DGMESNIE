using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using NSIE.Models;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;

namespace NSIE.Servicios
{
    public interface IRepositorioChat
    {
        Task<string> AskGPTAsync(string prompt);
        Task<string> AskVisitasdeVerificaciónAsync(string prompt);
        Task<DataTable> ConsultarBDAsync(string consultaSQL);
        Task<ConsultaNaturalViewModel> GenerarConsultaSQLAsync(string pregunta);
    }

    public class RepositorioChat : IRepositorioChat
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _connectionString;

        public RepositorioChat(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenAI:ApiKey"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<string> AskGPTAsync(string prompt)
        {
            var requestData = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("chat/completions", requestData);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                };
                var result = JsonSerializer.Deserialize<OpenAIChatResponse>(responseContent, options);

                return result?.Choices?[0].Message?.Content.Trim();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al solicitar a OpenAI: {response.ReasonPhrase}, Respuesta: {errorContent}");
            }
        }

        public async Task<string> AskVisitasdeVerificaciónAsync(string prompt)
        {
            var assistantId = "asst_Q6QN48K4WRuw9LptaYTcHhj4"; // ID de tu asistente.
            var modelName = "gpt-4-1106-preview"; // Modelo a utilizar.

            var requestData = new
            {
                model = modelName,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var requestUrl = $"https://api.openai.com/v1/assistants/{assistantId}/messages";

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestData), System.Text.Encoding.UTF8, "application/json")
            };

            requestMessage.Headers.Add("OpenAI-Beta", "assistants=v1");

            var response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                };
                var result = JsonSerializer.Deserialize<OpenAIChatResponse>(responseContent, options);

                return result?.Choices?[0].Message?.Content.Trim();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al solicitar a OpenAI: {response.ReasonPhrase}, Respuesta: {errorContent}");
            }
        }

        public async Task<DataTable> ConsultarBDAsync(string consultaSQL)
        {
            var dataTable = new DataTable();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(consultaSQL, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        dataTable.Load(reader);
                    }
                }
            }

            return dataTable;
        }

        public async Task<(Dictionary<string, List<string>> Tablas, List<Relacion> Relaciones)> ObtenerEstructuraBDAsync()
        {
            var estructuraBD = new Dictionary<string, List<string>>();
            var relacionesBD = new List<Relacion>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var queryEstructura = @"
                    SELECT 
                        TABLE_NAME AS NombreTabla,
                        COLUMN_NAME AS NombreColumna
                    FROM 
                        INFORMATION_SCHEMA.COLUMNS
                    WHERE 
                        TABLE_NAME IN (
                            SELECT TABLE_NAME 
                            FROM INFORMATION_SCHEMA.TABLES 
                            WHERE TABLE_TYPE = 'BASE TABLE'
                        )
                    UNION ALL
                    SELECT 
                        TABLE_NAME AS NombreTabla,
                        COLUMN_NAME AS NombreColumna
                    FROM 
                        INFORMATION_SCHEMA.COLUMNS
                    WHERE 
                        TABLE_NAME IN (
                            SELECT TABLE_NAME 
                            FROM INFORMATION_SCHEMA.VIEWS
                        )
                    ORDER BY 
                        NombreTabla, 
                        NombreColumna;";

                using (var command = new SqlCommand(queryEstructura, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var nombreTabla = reader["NombreTabla"].ToString();
                            var nombreColumna = reader["NombreColumna"].ToString();

                            if (!estructuraBD.ContainsKey(nombreTabla))
                            {
                                estructuraBD[nombreTabla] = new List<string>();
                            }

                            estructuraBD[nombreTabla].Add(nombreColumna);
                        }
                    }
                }

                var queryRelaciones = @"
                    SELECT
                        FK.TABLE_NAME AS TablaHija,
                        CU.COLUMN_NAME AS ColumnaHija,
                        PK.TABLE_NAME AS TablaPadre,
                        PT.COLUMN_NAME AS ColumnaPadre
                    FROM 
                        INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC
                        JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON RC.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
                        JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON RC.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
                        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON CU.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
                        JOIN (
                            SELECT 
                                i1.TABLE_NAME, 
                                i2.COLUMN_NAME 
                            FROM 
                                INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
                                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
                            WHERE 
                                i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
                        ) PT ON PT.TABLE_NAME = PK.TABLE_NAME
                    ORDER BY 
                        FK.TABLE_NAME, 
                        CU.COLUMN_NAME;";

                using (var command = new SqlCommand(queryRelaciones, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var relacion = new Relacion
                            {
                                TablaHija = reader["TablaHija"].ToString(),
                                ColumnaHija = reader["ColumnaHija"].ToString(),
                                TablaPadre = reader["TablaPadre"].ToString(),
                                ColumnaPadre = reader["ColumnaPadre"].ToString()
                            };
                            relacionesBD.Add(relacion);
                        }
                    }
                }
            }

            return (estructuraBD, relacionesBD);
        }

        public async Task<ConsultaNaturalViewModel> GenerarConsultaSQLAsync(string pregunta)
        {
            var (estructuraBD, relacionesBD) = await ObtenerEstructuraBDAsync();

            var catalogo = new CatalogoBD(estructuraBD, relacionesBD);

            var estructuraFiltrada = catalogo.ObtenerEstructuraFiltrada(pregunta);

            var prompt = $"{estructuraFiltrada}\n\nConvierte la siguiente pregunta en una consulta SQL para SQL Server: '{pregunta}'";

            var consultaSQL = await AskGPTAsync(prompt);

            consultaSQL = consultaSQL.Replace("`", ""); // Eliminar comillas invertidas
            consultaSQL = consultaSQL.Replace("CURDATE()", "GETDATE()"); // Reemplazar CURDATE() por GETDATE()
            System.Diagnostics.Debug.WriteLine($"Consulta SQL Generada: {consultaSQL}");

            var resultados = await ConsultarBDAsync(consultaSQL);

            return new ConsultaNaturalViewModel
            {
                Pregunta = pregunta,
                ConsultaSQL = consultaSQL,
                Resultados = resultados
            };
        }
    }

    public class OpenAIChatResponse
    {
        public Choice[] Choices { get; set; }
    }

    public class Choice
    {
        public Message Message { get; set; }
    }

    public class Message
    {
        public string Content { get; set; }
    }
}
