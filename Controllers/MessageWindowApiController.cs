using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NumeroEmpresarial.Core.Interfaces;

namespace NumeroEmpresarial.Controllers
{
    [Route("api/v1/message-windows")]
    [ApiController]
    public class MessageWindowApiController : ControllerBase
    {
        private readonly IMessageWindowService _messageWindowService;
        private readonly IPhoneNumberService _phoneNumberService;
        private readonly IUserService _userService;
        private readonly ILogger<MessageWindowApiController> _logger;
        public MessageWindowApiController(
            IMessageWindowService messageWindowService,
            IPhoneNumberService phoneNumberService,
            IUserService userService,
            ILogger<MessageWindowApiController> logger)
        {
            _messageWindowService = messageWindowService;
            _phoneNumberService = phoneNumberService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserMessageWindows()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { error = "Invalid API key" });
                }

                var windows = await _messageWindowService.GetActiveWindowsForUserAsync(userId);

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
                _logger.LogError(ex, "Error getting user message windows");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetMessageWindow(Guid id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { error = "Invalid API key" });
                }

                var window = await _messageWindowService.GetWindowByIdAsync(id);

                if (window == null || window.PhoneNumber?.UserId != userId)
                {
                    return NotFound(new { error = "Message window not found" });
                }

                var result = new
                {
                    id = window.Id,
                    startTime = window.StartTime,
                    endTime = window.EndTime,
                    active = window.Active,
                    maxMessages = window.MaxMessages,
                    receivedMessages = window.ReceivedMessages,
                    windowCost = window.WindowCost,
                    phoneNumber = new
                    {
                        id = window.PhoneNumber.Id,
                        number = window.PhoneNumber.Number
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting message window {id}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateMessageWindow([FromBody] CreateMessageWindowDto model)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { error = "Invalid API key" });
                }

                // Validar parámetros
                if (model.PhoneNumberId == Guid.Empty || model.DurationMinutes <= 0 || model.MaxMessages <= 0)
                {
                    return BadRequest(new { error = "Invalid parameters" });
                }

                // Verificar que el número de teléfono pertenezca al usuario
                var phoneNumber = await _phoneNumberService.GetPhoneNumberByIdAsync(model.PhoneNumberId);

                if (phoneNumber == null || phoneNumber.UserId != userId)
                {
                    return NotFound(new { error = "Phone number not found" });
                }

                // Verificar saldo del usuario
                var user = await _userService.GetUserByIdAsync(userId);
                var subscription = await _userService.GetActiveSubscriptionAsync(userId);

                decimal windowCost = 0.50m; // Costo predeterminado

                if (subscription != null && subscription.Plan != null)
                {
                    windowCost = subscription.Plan.WindowCost;
                }

                if (user.Balance < windowCost)
                {
                    return BadRequest(new { error = "Insufficient balance" });
                }

                // Crear ventana de mensajes
                var window = await _messageWindowService.CreateMessageWindowAsync(
                    model.PhoneNumberId,
                    model.DurationMinutes,
                    model.MaxMessages,
                    windowCost
                );

                var result = new
                {
                    id = window.Id,
                    startTime = window.StartTime,
                    endTime = window.EndTime,
                    active = window.Active,
                    maxMessages = window.MaxMessages,
                    receivedMessages = window.ReceivedMessages,
                    windowCost = window.WindowCost,
                    phoneNumber = new
                    {
                        id = phoneNumber.Id,
                        number = phoneNumber.Number
                    }
                };

                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating message window");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> CloseMessageWindow(Guid id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { error = "Invalid API key" });
                }

                var window = await _messageWindowService.GetWindowByIdAsync(id);

                if (window == null || window.PhoneNumber?.UserId != userId)
                {
                    return NotFound(new { error = "Message window not found" });
                }

                // Cerrar ventana de mensajes
                await _messageWindowService.CloseWindowAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error closing message window {id}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}/messages")]
        [Authorize]
        public async Task<IActionResult> GetWindowMessages(Guid id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { error = "Invalid API key" });
                }

                var window = await _messageWindowService.GetWindowByIdAsync(id);

                if (window == null || window.PhoneNumber?.UserId != userId)
                {
                    return NotFound(new { error = "Message window not found" });
                }

                var messages = await _messageWindowService.GetWindowMessagesAsync(id);

                var result = messages.Select(m => new
                {
                    id = m.Id,
                    from = m.From,
                    text = m.Text,
                    receivedTime = m.ReceivedTime,
                    redirected = m.Redirected,
                    messageCost = m.MessageCost
                });

                return Ok(new { messages = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting messages for window {id}");
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

    public class CreateMessageWindowDto
    {
        public Guid PhoneNumberId { get; set; }
        public int DurationMinutes { get; set; } = 10;
        public int MaxMessages { get; set; } = 10;
    }
}