using NumeroEmpresarial.Core.Interfaces;
using Plivo;

namespace NumeroEmpresarial.Core.Services
{
    public class PlivoService : IPlivoService
    {
        private readonly string _authId;
        private readonly string _authToken;
        private readonly string _apiVersion;
        private readonly PlivoApi _plivoClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PlivoService> _logger;

        public PlivoService(IConfiguration configuration, ILogger<PlivoService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _authId = _configuration["Plivo:AuthId"];
            _authToken = _configuration["Plivo:AuthToken"];
            _apiVersion = _configuration["Plivo:ApiVersion"];
            _plivoClient = new PlivoApi(_authId, _authToken);
        }

        public async Task<List<PhoneNumberResource>> SearchAvailableNumbersAsync(string countryIso, string type = null, string pattern = null)
        {
            try
            {
                _logger.LogInformation($"Buscando números disponibles: país={countryIso}, tipo={type}, patrón={pattern}");

                var options = new Dictionary<string, object>
                {
                    { "country_iso", countryIso },
                    { "limit", 20 }
                };

                if (!string.IsNullOrEmpty(type))
                {
                    options.Add("type", type); // "local", "tollfree", "mobile"
                }

                if (!string.IsNullOrEmpty(pattern))
                {
                    options.Add("pattern", pattern);
                }

                var response = await _plivoClient.PhoneNumber.List(options);

                _logger.LogInformation($"Números encontrados: {response.Objects.Count}");
                return response.Objects;
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

                var options = new Dictionary<string, object>
                {
                    { "number", phoneNumber }
                };

                var response = await _plivoClient.PhoneNumber.Buy(options);

                _logger.LogInformation($"Número alquilado: {response.Number}, ID: {response.Id}");
                return response.Id;
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

                await _plivoClient.Number.Delete(plivoPhoneNumberId);

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

                var options = new Dictionary<string, object>
                {
                    { "src", from },
                    { "dst", to },
                    { "text", text }
                };

                var response = await _plivoClient.Message.Create(options);

                _logger.LogInformation($"SMS enviado con éxito, ID: {response.MessageUuid[0]}");
                return response;
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

                var options = new Dictionary<string, object>
                {
                    { "message_url", webhookUrl }
                };

                await _plivoClient.Number.Update(plivoPhoneNumberId, options);

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

                var response = await _plivoClient.PhoneNumber.List();

                _logger.LogInformation($"Números obtenidos: {response.Objects.Count}");
                return response.Objects;
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

                var availableNumbers = await SearchAvailableNumbersAsync(
                    "US", // Puedes hacer esto dinámico basado en el prefijo del número
                    null,
                    phoneNumber);

                var matchingNumber = availableNumbers
                    .FirstOrDefault(n => n.Number == phoneNumber);

                if (matchingNumber != null)
                {
                    decimal cost = decimal.Parse(matchingNumber.MonthlyRentalRate);
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
}