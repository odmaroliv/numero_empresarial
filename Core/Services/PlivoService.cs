using NumeroEmpresarial.Core.Interfaces;
using NumeroEmpresarial.Domain.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NumeroEmpresarial.Core.Services
{
    public class PlivoService : IPlivoService
    {
        private readonly string _authId;
        private readonly string _authToken;
        private readonly string _apiBaseUrl;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PlivoService> _logger;

        public PlivoService(IConfiguration configuration, ILogger<PlivoService> logger, HttpClient httpClient = null)
        {
            _configuration = configuration;
            _logger = logger;
            _authId = _configuration["Plivo:AuthId"];
            _authToken = _configuration["Plivo:AuthToken"];
            _apiBaseUrl = "https://api.plivo.com/v1/Account/" + _authId;

            _httpClient = httpClient ?? new HttpClient();

            // Configurar autenticación HTTP básica
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_authId}:{_authToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        }

        public async Task<List<PhoneNumberResource>> SearchAvailableNumbersAsync(string countryIso, string type = null, string pattern = null)
        {
            try
            {
                _logger.LogInformation($"Buscando números disponibles: país={countryIso}, tipo={type}, patrón={pattern}");

                var queryParams = new List<string> { $"country_iso={countryIso}", "limit=20" };

                if (!string.IsNullOrEmpty(type))
                {
                    queryParams.Add($"type={type}");
                }

                if (!string.IsNullOrEmpty(pattern))
                {
                    queryParams.Add($"pattern={pattern}");
                }

                var url = $"{_apiBaseUrl}/PhoneNumber/?{string.Join("&", queryParams)}";
                var response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PlivoPhoneNumberResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogInformation($"Números encontrados: {result?.Objects?.Count ?? 0}");
                return result?.Objects ?? new List<PhoneNumberResource>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar números disponibles");
                throw new ApplicationException($"Error al buscar números disponibles: {ex.Message}", ex);
            }
        }

        public async Task<string> RentPhoneNumberAsync(string phoneNumber)
        {
            try
            {
                _logger.LogInformation($"Alquilando número: {phoneNumber}");

                var data = new { numbers = phoneNumber };
                var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/PhoneNumber/{phoneNumber}/", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PlivoBuyResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogInformation($"Número alquilado: {result?.Number}, ID: {result?.Id}");
                return result?.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al alquilar número: {phoneNumber}");
                throw new ApplicationException($"Error al alquilar número: {ex.Message}", ex);
            }
        }

        public async Task<bool> ReleasePhoneNumberAsync(string plivoPhoneNumberId)
        {
            try
            {
                _logger.LogInformation($"Liberando número con ID: {plivoPhoneNumberId}");

                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/Number/{plivoPhoneNumberId}/");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation($"Número liberado con éxito: {plivoPhoneNumberId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al liberar número: {plivoPhoneNumberId}");
                throw new ApplicationException($"Error al liberar número: {ex.Message}", ex);
            }
        }

        public async Task<MessageResponse> SendSmsAsync(string from, string to, string text)
        {
            try
            {
                _logger.LogInformation($"Enviando SMS desde {from} a {to}");

                var data = new
                {
                    src = from,
                    dst = to,
                    text = text
                };

                var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/Message/", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<MessageResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.MessageUuid?.Count > 0)
                {
                    _logger.LogInformation($"SMS enviado con éxito, ID: {result.MessageUuid[0]}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar SMS desde {from} a {to}");
                throw new ApplicationException($"Error al enviar SMS: {ex.Message}", ex);
            }
        }

        public async Task<bool> SetupRedirectionWebhookAsync(string plivoPhoneNumberId, string webhookUrl)
        {
            try
            {
                _logger.LogInformation($"Configurando webhook para número {plivoPhoneNumberId}: {webhookUrl}");

                var data = new { message_url = webhookUrl };
                var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/Number/{plivoPhoneNumberId}/", content);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Webhook configurado con éxito");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al configurar webhook para {plivoPhoneNumberId}");
                throw new ApplicationException($"Error al configurar webhook: {ex.Message}", ex);
            }
        }

        public async Task<List<PhoneNumberResource>> GetAccountNumbersAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo números de la cuenta");

                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/Number/");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PlivoPhoneNumberResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogInformation($"Números obtenidos: {result?.Objects?.Count ?? 0}");
                return result?.Objects ?? new List<PhoneNumberResource>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener números de la cuenta");
                throw new ApplicationException($"Error al obtener números de la cuenta: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetNumberRentalCostAsync(string phoneNumber)
        {
            try
            {
                _logger.LogInformation($"Obteniendo costo de número: {phoneNumber}");

                string countryCode = "US"; // Por defecto, puedes extraer el código de país del número
                if (phoneNumber.StartsWith("+"))
                {
                    // Extraer código de país básico (muy simplificado)
                    if (phoneNumber.StartsWith("+1"))
                        countryCode = "US";
                    else if (phoneNumber.StartsWith("+44"))
                        countryCode = "GB";
                    else if (phoneNumber.StartsWith("+34"))
                        countryCode = "ES";
                    else if (phoneNumber.StartsWith("+52"))
                        countryCode = "MX";
                }

                var availableNumbers = await SearchAvailableNumbersAsync(countryCode);

                var matchingNumber = availableNumbers
                    .FirstOrDefault(n => n.Number == phoneNumber || n.Number == "+" + phoneNumber);

                if (matchingNumber != null)
                {
                    decimal cost = decimal.TryParse(matchingNumber.MonthlyRentalRate, out var parsed) ? parsed : 1.99m;
                    _logger.LogInformation($"Costo mensual para {phoneNumber}: ${cost}");
                    return cost;
                }

                // Si no encontramos el número exacto, usamos un costo predeterminado
                _logger.LogWarning($"No se encontró información de costo para {phoneNumber}, usando valor predeterminado");
                return 1.99m; // Valor predeterminado
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener costo del número {phoneNumber}");
                return 1.99m; // Valor predeterminado en caso de error
            }
        }
    }

    // Clases auxiliares para deserializar respuestas de Plivo
    public class PlivoPhoneNumberResponse
    {
        public List<PhoneNumberResource> Objects { get; set; }
    }

    public class PlivoBuyResponse
    {
        public string Number { get; set; }
        public string Id { get; set; }
    }
}