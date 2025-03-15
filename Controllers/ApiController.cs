using Microsoft.AspNetCore.Mvc;
using NumeroEmpresarial.Core.Interfaces;

namespace NumeroEmpresarial.Controllers
{
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPhoneNumberService _phoneNumberService;
        private readonly IPlivoService _plivoService;
        private readonly IAuthenticationService _authService;
        private readonly ILogger<ApiController> _logger;

        public ApiController(
            IUserService userService,
            IPhoneNumberService phoneNumberService,
            IPlivoService plivoService,
            IAuthenticationService authService,
            ILogger<ApiController> logger)
        {
            _userService = userService;
            _phoneNumberService = phoneNumberService;
            _plivoService = plivoService;
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var authResponse = await _authService.AuthenticateAsync(request.Email, request.Password);

                if (authResponse == null)
                {
                    return Unauthorized(new { error = "Invalid credentials" });
                }

                return Ok(new
                {
                    token = authResponse.Token,
                    refreshToken = authResponse.RefreshToken,
                    expiration = authResponse.Expiration,
                    user = new
                    {
                        id = authResponse.User.Id,
                        name = authResponse.User.Name,
                        email = authResponse.User.Email,
                        balance = authResponse.User.Balance
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("auth/refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var authResponse = await _authService.RefreshTokenAsync(request.Token, request.RefreshToken);

                if (authResponse == null)
                {
                    return Unauthorized(new { error = "Invalid token" });
                }

                return Ok(new
                {
                    token = authResponse.Token,
                    refreshToken = authResponse.RefreshToken,
                    expiration = authResponse.Expiration
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("numbers/rent")]
        public async Task<IActionResult> RentNumber([FromBody] RentPhoneNumberDto model)
        {
            try
            {
                // Extraer el API key del encabezado
                var apiKey = GetApiKeyFromHeader();
                if (string.IsNullOrEmpty(apiKey))
                {
                    return Unauthorized(new { error = "API key not provided" });
                }

                // Obtener el usuario
                var user = await _userService.GetUserByApiKeyAsync(apiKey);
                if (user == null)
                {
                    return Unauthorized(new { error = "Invalid API key" });
                }

                // Verificar saldo
                var numberCost = await _plivoService.GetNumberRentalCostAsync(model.PhoneNumber);
                if (user.Balance < numberCost)
                {
                    return BadRequest(new { error = "Insufficient balance" });
                }

                // Alquilar número
                var plivoId = await _plivoService.RentPhoneNumberAsync(model.PhoneNumber);

                // Configurar webhook
                var webhookUrl = $"{Request.Scheme}://{Request.Host}/api/webhook/sms";
                await _plivoService.SetupRedirectionWebhookAsync(plivoId, webhookUrl);

                // Registrar número en la base de datos
                var phoneNumber = await _phoneNumberService.AddPhoneNumberAsync(
                    user.Id,
                    model.PhoneNumber,
                    plivoId,
                    model.RedirectionNumber,
                    numberCost,
                    model.PhoneNumberType); // Usar PhoneNumberType en lugar de Type

                return Ok(new
                {
                    id = phoneNumber.Id,
                    number = phoneNumber.Number,
                    acquisitionDate = phoneNumber.AcquisitionDate,
                    expirationDate = phoneNumber.ExpirationDate,
                    monthlyCost = phoneNumber.MonthlyCost
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renting number");
                return StatusCode(500, new { error = "Internal server error: " + ex.Message });
            }
        }

        private string GetApiKeyFromHeader()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            return authHeader.Substring("Bearer ".Length).Trim();
        }
    }

    // Clases para las solicitudes
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}