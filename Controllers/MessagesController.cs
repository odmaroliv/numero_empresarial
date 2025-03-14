using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NumeroEmpresarial.Core.Interfaces;
using NumeroEmpresarial.Data;
using NumeroEmpresarial.Domain.DTOs;
using System.Security.Claims;

namespace NumeroEmpresarial.Controllers
{
    [Route("api/v1/messages")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageWindowService _messageWindowService;
        private readonly IPhoneNumberService _phoneNumberService;
        private readonly IUserService _userService;
        private readonly ILocalizationService _localizationService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MessagesController> _logger;
        public MessagesController(
            IMessageWindowService messageWindowService,
            IPhoneNumberService phoneNumberService,
            IUserService userService,
            ILocalizationService localizationService,
            ApplicationDbContext context,
            ILogger<MessagesController> logger)
        {
            _messageWindowService = messageWindowService;
            _phoneNumberService = phoneNumberService;
            _userService = userService;
            _localizationService = localizationService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMessages([FromQuery] GetMessagesQuery query)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Crear consulta base
                var messagesQuery = _context.Messages
                    .Include(m => m.MessageWindow)
                        .ThenInclude(w => w.PhoneNumber)
                    .AsQueryable();

                // Filtrar por mensajes que pertenecen al usuario actual
                messagesQuery = messagesQuery.Where(m => m.MessageWindow.PhoneNumber.UserId == userId);

                // Aplicar filtros adicionales si se proporcionan
                if (query.PhoneNumberId.HasValue)
                {
                    messagesQuery = messagesQuery.Where(m => m.MessageWindow.PhoneNumber.Id == query.PhoneNumberId);
                }

                if (query.WindowId.HasValue)
                {
                    messagesQuery = messagesQuery.Where(m => m.MessageWindowId == query.WindowId);
                }

                if (query.StartDate.HasValue)
                {
                    var startDate = query.StartDate.Value.Date;
                    messagesQuery = messagesQuery.Where(m => m.ReceivedTime >= startDate);
                }

                if (query.EndDate.HasValue)
                {
                    var endDate = query.EndDate.Value.Date.AddDays(1); // Incluir todo el día
                    messagesQuery = messagesQuery.Where(m => m.ReceivedTime < endDate);
                }

                // Ordenar los resultados
                messagesQuery = query.OrderByDescending
                    ? messagesQuery.OrderByDescending(m => m.ReceivedTime)
                    : messagesQuery.OrderBy(m => m.ReceivedTime);

                // Aplicar paginación
                var pageSize = query.PageSize > 0 ? query.PageSize : 10;
                var pageNumber = query.Page > 0 ? query.Page : 1;

                var totalMessages = await messagesQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalMessages / (double)pageSize);

                var messages = await messagesQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Mapear a DTOs
                var messageDtos = messages.Select(m => new MessageDto
                {
                    Id = m.Id,
                    MessageWindowId = m.MessageWindowId,
                    From = m.From,
                    Text = m.Text,
                    ReceivedTime = m.ReceivedTime,
                    Redirected = m.Redirected,
                    MessageCost = m.MessageCost
                }).ToList();

                // Crear respuesta paginada
                var response = new
                {
                    messages = messageDtos,
                    page = pageNumber,
                    pageSize = pageSize,
                    totalPages = totalPages,
                    totalMessages = totalMessages
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessage(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var message = await _context.Messages
                    .Include(m => m.MessageWindow)
                        .ThenInclude(w => w.PhoneNumber)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (message == null)
                {
                    return NotFound(new { error = "Message not found" });
                }

                // Verificar que el mensaje pertenece al usuario actual
                if (message.MessageWindow.PhoneNumber.UserId != userId)
                {
                    return Forbid();
                }

                var messageDto = new MessageDto
                {
                    Id = message.Id,
                    MessageWindowId = message.MessageWindowId,
                    From = message.From,
                    Text = message.Text,
                    ReceivedTime = message.ReceivedTime,
                    Redirected = message.Redirected,
                    MessageCost = message.MessageCost
                };

                return Ok(messageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting message {id}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("windows/{windowId}")]
        public async Task<IActionResult> GetMessagesByWindow(Guid windowId)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Verificar que la ventana pertenece al usuario actual
                var window = await _context.MessageWindows
                    .Include(w => w.PhoneNumber)
                    .FirstOrDefaultAsync(w => w.Id == windowId);

                if (window == null)
                {
                    return NotFound(new { error = "Message window not found" });
                }

                if (window.PhoneNumber.UserId != userId)
                {
                    return Forbid();
                }

                // Obtener los mensajes de la ventana
                var messages = await _context.Messages
                    .Where(m => m.MessageWindowId == windowId)
                    .OrderByDescending(m => m.ReceivedTime)
                    .ToListAsync();

                // Mapear a DTOs
                var messageDtos = messages.Select(m => new MessageDto
                {
                    Id = m.Id,
                    MessageWindowId = m.MessageWindowId,
                    From = m.From,
                    Text = m.Text,
                    ReceivedTime = m.ReceivedTime,
                    Redirected = m.Redirected,
                    MessageCost = m.MessageCost
                }).ToList();

                return Ok(new { messages = messageDtos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting messages for window {windowId}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("phone-numbers/{phoneNumberId}")]
        public async Task<IActionResult> GetMessagesByPhoneNumber(Guid phoneNumberId)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Verificar que el número pertenece al usuario actual
                var phoneNumber = await _context.PhoneNumbers
                    .FirstOrDefaultAsync(p => p.Id == phoneNumberId);

                if (phoneNumber == null)
                {
                    return NotFound(new { error = "Phone number not found" });
                }

                if (phoneNumber.UserId != userId)
                {
                    return Forbid();
                }

                // Obtener los mensajes del número
                var messages = await _context.Messages
                    .Include(m => m.MessageWindow)
                    .Where(m => m.MessageWindow.PhoneNumberId == phoneNumberId)
                    .OrderByDescending(m => m.ReceivedTime)
                    .Take(100) // Limitar a los 100 más recientes
                    .ToListAsync();

                // Mapear a DTOs
                var messageDtos = messages.Select(m => new MessageDto
                {
                    Id = m.Id,
                    MessageWindowId = m.MessageWindowId,
                    From = m.From,
                    Text = m.Text,
                    ReceivedTime = m.ReceivedTime,
                    Redirected = m.Redirected,
                    MessageCost = m.MessageCost
                }).ToList();

                return Ok(new { messages = messageDtos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting messages for phone number {phoneNumberId}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetMessageStatistics([FromQuery] MessageStatsQuery query)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Crear consulta base
                var messagesQuery = _context.Messages
                    .Include(m => m.MessageWindow)
                        .ThenInclude(w => w.PhoneNumber)
                    .Where(m => m.MessageWindow.PhoneNumber.UserId == userId)
                    .AsQueryable();

                // Aplicar filtros adicionales si se proporcionan
                if (query.PhoneNumberId.HasValue)
                {
                    messagesQuery = messagesQuery.Where(m => m.MessageWindow.PhoneNumber.Id == query.PhoneNumberId);
                }

                if (query.StartDate.HasValue)
                {
                    var startDate = query.StartDate.Value.Date;
                    messagesQuery = messagesQuery.Where(m => m.ReceivedTime >= startDate);
                }

                if (query.EndDate.HasValue)
                {
                    var endDate = query.EndDate.Value.Date.AddDays(1); // Incluir todo el día
                    messagesQuery = messagesQuery.Where(m => m.ReceivedTime < endDate);
                }

                // Calcular estadísticas
                var totalMessages = await messagesQuery.CountAsync();
                var redirectedMessages = await messagesQuery.CountAsync(m => m.Redirected);
                var totalCost = await messagesQuery.SumAsync(m => m.MessageCost);

                // Estadísticas por número
                var phoneNumberStats = await messagesQuery
                    .GroupBy(m => new { m.MessageWindow.PhoneNumber.Id, m.MessageWindow.PhoneNumber.Number })
                    .Select(g => new
                    {
                        phoneNumberId = g.Key.Id,
                        phoneNumber = g.Key.Number,
                        messagesCount = g.Count(),
                        totalCost = g.Sum(m => m.MessageCost)
                    })
                    .OrderByDescending(x => x.messagesCount)
                    .Take(5)
                    .ToListAsync();

                // Estadísticas por remitente
                var senderStats = await messagesQuery
                    .GroupBy(m => m.From)
                    .Select(g => new
                    {
                        from = g.Key,
                        messagesCount = g.Count()
                    })
                    .OrderByDescending(x => x.messagesCount)
                    .Take(10)
                    .ToListAsync();

                // Estadísticas por día
                var dailyStats = await messagesQuery
                    .GroupBy(m => m.ReceivedTime.Date)
                    .Select(g => new
                    {
                        date = g.Key,
                        messagesCount = g.Count(),
                        totalCost = g.Sum(m => m.MessageCost)
                    })
                    .OrderBy(x => x.date)
                    .ToListAsync();

                var statistics = new
                {
                    totalMessages,
                    redirectedMessages,
                    totalCost,
                    phoneNumberStats,
                    senderStats,
                    dailyStats
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting message statistics");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportMessages([FromBody] ExportMessagesDto model)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Crear consulta base
                var messagesQuery = _context.Messages
                    .Include(m => m.MessageWindow)
                        .ThenInclude(w => w.PhoneNumber)
                    .Where(m => m.MessageWindow.PhoneNumber.UserId == userId)
                    .AsQueryable();

                // Aplicar filtros
                if (model.PhoneNumberId.HasValue)
                {
                    messagesQuery = messagesQuery.Where(m => m.MessageWindow.PhoneNumber.Id == model.PhoneNumberId);
                }

                if (model.WindowId.HasValue)
                {
                    messagesQuery = messagesQuery.Where(m => m.MessageWindowId == model.WindowId);
                }

                if (model.StartDate.HasValue)
                {
                    var startDate = model.StartDate.Value.Date;
                    messagesQuery = messagesQuery.Where(m => m.ReceivedTime >= startDate);
                }

                if (model.EndDate.HasValue)
                {
                    var endDate = model.EndDate.Value.Date.AddDays(1); // Incluir todo el día
                    messagesQuery = messagesQuery.Where(m => m.ReceivedTime < endDate);
                }

                // Ordenar los resultados
                messagesQuery = messagesQuery.OrderBy(m => m.ReceivedTime);

                // Obtener los mensajes
                var messages = await messagesQuery.ToListAsync();

                // Construir CSV o JSON según el formato solicitado
                if (model.Format.ToLower() == "csv")
                {
                    // Generar contenido CSV
                    var csv = new System.Text.StringBuilder();
                    csv.AppendLine("ID,Phone Number,From,Text,Received Time,Redirected,Cost");

                    foreach (var message in messages)
                    {
                        csv.AppendLine($"{message.Id}," +
                            $"{message.MessageWindow.PhoneNumber.Number}," +
                            $"{message.From}," +
                            $"\"{message.Text.Replace("\"", "\"\"")}\"," + // Escapar comillas en el texto
                            $"{message.ReceivedTime:yyyy-MM-dd HH:mm:ss}," +
                            $"{message.Redirected}," +
                            $"{message.MessageCost}");
                    }

                    return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()),
                        "text/csv",
                        $"messages_export_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
                }
                else // JSON por defecto
                {
                    // Mapear a DTOs
                    var messageDtos = messages.Select(m => new
                    {
                        id = m.Id,
                        phoneNumber = m.MessageWindow.PhoneNumber.Number,
                        from = m.From,
                        text = m.Text,
                        receivedTime = m.ReceivedTime,
                        redirected = m.Redirected,
                        messageCost = m.MessageCost
                    }).ToList();

                    return Ok(new { messages = messageDtos });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting messages");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID");
            }

            return userId;
        }
    }

    public class GetMessagesQuery
    {
        public Guid? PhoneNumberId { get; set; }
        public Guid? WindowId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool OrderByDescending { get; set; } = true;
    }

    public class MessageStatsQuery
    {
        public Guid? PhoneNumberId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ExportMessagesDto
    {
        public Guid? PhoneNumberId { get; set; }
        public Guid? WindowId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Format { get; set; } = "json"; // "json" o "csv"
    }
}