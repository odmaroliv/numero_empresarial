using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NumeroEmpresarial.Core.Interfaces;

namespace NumeroEmpresarial.Controllers
{
    [Route("api/v1/phone-numbers")]
    [ApiController]
    public class PhoneNumberApiController : ControllerBase
    {
        private readonly IPhoneNumberService _phoneNumberService;
        private readonly IMessageWindowService _messageWindowService;
        private readonly IUserService _userService;
        private readonly IPlivoService _plivoService;
        private readonly ILogger<PhoneNumberApiController> _logger;
        public PhoneNumberApiController(
            IPhoneNumberService phoneNumberService,
            IMessageWindowService messageWindowService,
            IUserService userService,
            IPlivoService plivoService,
            ILogger<PhoneNumberApiController> logger)
        {
            _phoneNumberService = phoneNumberService;
            _messageWindowService = messageWindowService;
            _userService = userService;
            _plivoService = plivoService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPhoneNumbers()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { error = "Invalid API key" });
                }

                var phoneNumbers = await _phoneNumberService.GetUserPhoneNumbersAsync(userId);

                var result = phoneNumbers.Select(p => new
                {
                    id = p.Id,
                    number = p.Number,
                    acquisitionDate = p.AcquisitionDate,
                    expirationDate = p.ExpirationDate,
                    active = p.Active,
                    redirectionNumber = p.RedirectionNumber,
                    type = p.Type.ToString(),
                    monthlyCost = p.MonthlyCost
                });

                return Ok(new { phoneNumbers = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user phone numbers");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetPhoneNumber(Guid id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { error = "Invalid API key" });
                }

                var phoneNumber = await _phoneNumberService.GetPhoneNumberByIdAsync(id);

                if (phoneNumber == null || phoneNumber.UserId != userId)
                {
                    return NotFound(new { error = "Phone number not found" });
                }

                var result = new
                {
                    id = phoneNumber.Id,
                    number = phoneNumber.Number,
                    acquisitionDate = phoneNumber.AcquisitionDate,
                    expirationDate = phoneNumber.ExpirationDate,
                    active = phoneNumber.Active,
                    redirectionNumber = phoneNumber.RedirectionNumber,
                    type = phoneNumber.Type.ToString(),
                    monthlyCost = phoneNumber.MonthlyCost
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting phone number {id}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchPhoneNumbers([FromQuery] string countryCode,
                                                        [FromQuery] string type = null,
                                                        [FromQuery] string pattern = null)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { error = "Invalid API key" });
                }

                if (string.IsNullOrEmpty(countryCode))
                {
                    return BadRequest(new { error = "Country code is required" });
                }

                var numbers = await _plivoService.SearchAvailableNumbersAsync(countryCode, type, pattern);

                var result = numbers.Select(n => new
                {
                    number = n.Number,
                    type = n.Type,
                    monthlyCost = decimal.Parse(n.MonthlyRentalRate),
                    setupFee = decimal.Parse(n.SetupRate)
                });

                return Ok(new { availableNumbers = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching phone numbers");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RentPhoneNumber([FromBody] RentPhoneNumberDto model)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { error = "Invalid API key" });
                }

                if (string.IsNullOrEmpty(model.PhoneNumber) || string.IsNullOrEmpty(model.RedirectionNumber))
                {
                    return BadRequest(new { error = "Phone number and redirection number are required" });
                }

                // Verificar saldo del usuario
                var user = await _userService.GetUserByIdAsync(userId);
                var numberCost = await _plivoService.GetNumberRentalCostAsync(model.PhoneNumber);

                if (user.Balance < numberCost)
                {
                    return BadRequest(new { error = "Insufficient balance" });
                }

                // Alquilar número en Plivo
                var plivoId = await _plivoService.RentPhoneNumberAsync(model.PhoneNumber);

                // Configurar webhook
                var webhookUrl = $"{Request.Scheme}://{Request.Host}/api/webhook/plivo/sms";
                await _plivoService.SetupRedirectionWebhookAsync(plivoId, webhookUrl);

                // Registrar en la base de datos
                var phoneNumber = await _phoneNumberService.AddPhoneNumberAsync(
                    userId,
                    model.PhoneNumber,
                    plivoId,
                    model.RedirectionNumber,
                    numberCost,
                    Domain.Enums.PhoneNumberType.Standard);

                var result = new
                {
                    id = phoneNumber.Id,
                    number = phoneNumber.Number,
                    acquisitionDate = phoneNumber.AcquisitionDate,
                    expirationDate = phoneNumber.ExpirationDate,
                    active = phoneNumber.Active,
                    redirectionNumber = phoneNumber.RedirectionNumber,
                    type = phoneNumber.Type.ToString(),
                    monthlyCost = phoneNumber.MonthlyCost
                };

                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renting phone number");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> ReleasePhoneNumber(Guid id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { error = "Invalid API key" });
                }

                var phoneNumber = await _phoneNumberService.GetPhoneNumberByIdAsync(id);

                if (phoneNumber == null || phoneNumber.UserId != userId)
                {
                    return NotFound(new { error = "Phone number not found" });
                }

                // Liberar número en Plivo
                await _plivoService.ReleasePhoneNumberAsync(phoneNumber.PlivoId);

                // Actualizar en la base de datos
                await _phoneNumberService.DeactivatePhoneNumberAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error releasing phone number {id}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}/message-windows")]
        [Authorize]
        public async Task<IActionResult> GetPhoneNumberMessageWindows(Guid id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { error = "Invalid API key" });
                }

                var phoneNumber = await _phoneNumberService.GetPhoneNumberByIdAsync(id);

                if (phoneNumber == null || phoneNumber.UserId != userId)
                {
                    return NotFound(new { error = "Phone number not found" });
                }

                var windows = await _messageWindowService.GetWindowsByPhoneNumberIdAsync(id);

                var result = windows.Select(w => new
                {
                    id = w.Id,
                    startTime = w.StartTime,
                    endTime = w.EndTime,
                    active = w.Active,
                    maxMessages = w.MaxMessages,
                    receivedMessages = w.ReceivedMessages,
                    windowCost = w.WindowCost,
                    phoneNumber = new
                    {
                        id = w.PhoneNumber.Id,
                        number = w.PhoneNumber.Number
                    }
                });

                return Ok(new { messageWindows = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting message windows for phone number {id}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPut("{id}/redirection")]
        [Authorize]
        public async Task<IActionResult> UpdateRedirectionNumber(Guid id, [FromBody] UpdateRedirectionDto model)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { error = "Invalid API key" });
                }

                if (string.IsNullOrEmpty(model.RedirectionNumber))
                {
                    return BadRequest(new { error = "Redirection number is required" });
                }

                var phoneNumber = await _phoneNumberService.GetPhoneNumberByIdAsync(id);

                if (phoneNumber == null || phoneNumber.UserId != userId)
                {
                    return NotFound(new { error = "Phone number not found" });
                }

                await _phoneNumberService.UpdateRedirectionNumberAsync(id, model.RedirectionNumber);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating redirection number for phone number {id}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        private Guid GetUserIdFromToken()
        {
            // Extraer el API key del encabezado de autorización
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Guid.Empty;
            }

            var apiKey = authHeader.Substring("Bearer ".Length).Trim();

            // Buscar el usuario por API key
            var user = _userService.GetUserByApiKeyAsync(apiKey).Result;
            return user?.Id ?? Guid.Empty;
        }
    }

    public class RentPhoneNumberDto
    {
        public string PhoneNumber { get; set; }
        public string RedirectionNumber { get; set; }
    }

    public class UpdateRedirectionDto
    {
        public string RedirectionNumber { get; set; }
    }
}