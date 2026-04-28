using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dapper;
using System.Data.SqlClient;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NSIE.Controllers;
using NSIE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

using System.Threading.Tasks;
using System.Collections.Generic;


namespace NSIE.Servicios
{
    public interface IRepositorioIA
    {
        Task<string> AskIA(string prompt);
        Task<IEnumerable<dynamic>> ExecuteQueryAsync(string sqlQuery);
    }

    public class RepositorioIA : IRepositorioIA
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _connectionString;

        public RepositorioIA(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("OpenAI");
            _apiKey = configuration["OpenAI:ApiKey"];
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> AskIA(string prompt)
        {
            var requestData = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestData);

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

        public async Task<IEnumerable<dynamic>> ExecuteQueryAsync(string sqlQuery)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<dynamic>(sqlQuery);
        }

        private class OpenAIChatResponse
        {
            public Choice[] Choices { get; set; }
        }

        private class Choice
        {
            public Message Message { get; set; }
        }

        private class Message
        {
            public string Content { get; set; }
        }
    }
}
