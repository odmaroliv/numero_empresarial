using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NumeroEmpresarial.Core.Interfaces;
using NumeroEmpresarial.Data;

namespace NumeroEmpresarial.Controllers
{
    [Route("api/webhook")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMessageWindowService _messageWindowService;
        private readonly IPlivoService _plivoService;
        private readonly IPhoneNumberService _phoneNumberService;
        private readonly IStripeService _stripeService;
        private readonly IUserService _userService;

        public WebhookController(
            ILogger<WebhookController> logger,
            ApplicationDbContext context,
            IMessageWindowService messageWindowService,
            IPlivoService plivoService,
            IPhoneNumberService phoneNumberService,
            IStripeService stripeService,
            IUserService userService)
        {
            _logger = logger;
            _context = context;
            _messageWindowService = messageWindowService;
            _plivoService = plivoService;
            _phoneNumberService = phoneNumberService;
            _stripeService = stripeService;
            _userService = userService;
        }

        [HttpPost("plivo/sms")]
        public async Task<IActionResult> PlivoSmsWebhook()
        {
            try
            {
                _logger.LogInformation("SMS webhook received from Plivo");

                // Leer datos del webhook de Plivo
                string from = Request.Form["From"];
                string to = Request.Form["To"];
                string text = Request.Form["Text"];
                string messageUuid = Request.Form["MessageUUID"];

                _logger.LogInformation($"SMS recibido - De: {from}, Para: {to}, Texto: {text}");

                // Buscar una ventana activa para este número
                var activeWindow = await _messageWindowService.GetActiveWindowForPhoneNumberAsync(to);

                if (activeWindow == null)
                {
                    _logger.LogWarning($"No hay ventana activa para el número {to}");
                    return Ok(new { status = "error", message = "No active window" });
                }

                // Calcular costo del mensaje según el plan del usuario
                var phoneNumber = await _phoneNumberService.GetPhoneNumberByNumberAsync(to);
                var user = await _phoneNumberService.GetUserByPhoneNumberAsync(to);

                decimal messageCost = 0.01m; // Costo predeterminado

                // Si hay un usuario y tiene una suscripción activa, usar el costo del plan
                if (user != null)
                {
                    var subscription = await _context.Subscriptions
                        .Include(s => s.Plan)
                        .FirstOrDefaultAsync(s => s.UserId == user.Id && s.Active);

                    if (subscription != null && subscription.Plan != null)
                    {
                        messageCost = subscription.Plan.MessageCost;
                    }
                }

                // Verificar saldo del usuario
                if (user != null && user.Balance < messageCost)
                {
                    _logger.LogWarning($"Saldo insuficiente para usuario {user.Id}, número {to}");
                    return Ok(new { status = "error", message = "Insufficient balance" });
                }

                // Registrar el mensaje en la ventana activa
                var message = await _messageWindowService.RecordMessageAsync(
                    activeWindow.Id,
                    from,
                    text,
                    messageCost);

                // Redirigir el mensaje al número del usuario si la ventana está activa
                if (phoneNumber != null && !string.IsNullOrEmpty(phoneNumber.RedirectionNumber))
                {
                    try
                    {
                        var response = await _plivoService.SendSmsAsync(
                            to,  // El número de Plivo como remitente
                            phoneNumber.RedirectionNumber, // Número de redirección del usuario
                            $"De: {from}\n{text}" // Incluir el remitente original
                        );

                        // Actualizar estado de redirección del mensaje
                        message.Redirected = true;
                        await _context.SaveChangesAsync();

                        _logger.LogInformation($"SMS redirigido a {phoneNumber.RedirectionNumber}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error al redirigir SMS: {ex.Message}");
                    }
                }

                return Ok(new { status = "success", messageId = message.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en webhook SMS: {ex.Message}");
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("stripe")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeSignature = Request.Headers["Stripe-Signature"];

                // Procesar el webhook de Stripe
                await _stripeService.HandleWebhookAsync(json, stripeSignature);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en webhook Stripe: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("plivo/call")]
        public IActionResult PlivoCallWebhook()
        {
            try
            {
                _logger.LogInformation("Call webhook received from Plivo");

                // Leer datos del webhook de Plivo para llamadas
                string from = Request.Form["From"];
                string to = Request.Form["To"];
                string callUuid = Request.Form["CallUUID"];

                _logger.LogInformation($"Llamada recibida - De: {from}, Para: {to}");

                // Obtener el número de redirección
                var phoneNumber = _phoneNumberService.GetPhoneNumberByNumberAsync(to).Result;

                if (phoneNumber == null || string.IsNullOrEmpty(phoneNumber.RedirectionNumber))
                {
                    _logger.LogWarning($"No hay redirección configurada para el número {to}");
                    return Content("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response><Speak>Lo sentimos, este número no está configurado para recibir llamadas.</Speak></Response>", "application/xml");
                }

                // Generar XML de redirección de llamada
                string redirectXml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Response>
    <Dial callerId=""{to}"">{phoneNumber.RedirectionNumber}</Dial>
</Response>";

                return Content(redirectXml, "application/xml");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en webhook de llamada: {ex.Message}");

                // Devolver un mensaje de error al llamante
                return Content("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response><Speak>Lo sentimos, ha ocurrido un error al procesar la llamada.</Speak></Response>", "application/xml");
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { status = "success", message = "Webhook endpoints are working" });
        }
    }
}