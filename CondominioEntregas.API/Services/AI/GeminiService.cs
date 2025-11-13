using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PortSafe.Services.AI.Interfaces;
using PortSafe.Services.AI.Models;

namespace PortSafe.Services.AI
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(
            HttpClient httpClient, 
            IOptions<AIConfiguration> aiConfig,
            ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _settings = aiConfig.Value.Gemini;
            _logger = logger;

            if (string.IsNullOrEmpty(_settings.ApiKey))
            {
                throw new InvalidOperationException("Gemini API Key não configurada!");
            }
        }

        public async Task<string> GenerateContentAsync(string prompt, double? temperature = null)
        {
            try
            {
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = temperature ?? _settings.Temperature,
                        maxOutputTokens = _settings.MaxTokens
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var url = $"{_settings.BaseUrl}/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro na API Gemini: {response.StatusCode} - {errorContent}");
                    throw new HttpRequestException($"Erro ao chamar Gemini API: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonDocument.Parse(responseContent);
                
                var text = jsonResponse.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar conteúdo com Gemini");
                throw;
            }
        }

        public async Task<T?> GenerateStructuredResponseAsync<T>(string prompt, double? temperature = null) where T : class
        {
            var response = await GenerateContentAsync(prompt, temperature);
            
            try
            {
                // Remove markdown code blocks se existirem
                var cleanedResponse = response.Trim();
                if (cleanedResponse.StartsWith("```json"))
                {
                    cleanedResponse = cleanedResponse.Substring(7);
                }
                if (cleanedResponse.StartsWith("```"))
                {
                    cleanedResponse = cleanedResponse.Substring(3);
                }
                if (cleanedResponse.EndsWith("```"))
                {
                    cleanedResponse = cleanedResponse.Substring(0, cleanedResponse.Length - 3);
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<T>(cleanedResponse.Trim(), options);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Erro ao deserializar resposta: {response}");
                throw new InvalidOperationException("Resposta da IA não está no formato JSON esperado", ex);
            }
        }
    }
}
