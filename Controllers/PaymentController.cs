using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NumeroEmpresarial.Core.Interfaces;
using System.Security.Claims;

namespace NumeroEmpresarial.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IStripeService _stripeService;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        public PaymentController(
        IStripeService stripeService,
            IUserService userService,
            IConfiguration configuration)
        {
            _stripeService = stripeService;
            _userService = userService;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var model = new PaymentViewModel
            {
                PublicKey = _configuration["Stripe:PublicKey"]
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Checkout(string sessionId)
        {
            var model = new CheckoutViewModel
            {
                SessionId = sessionId,
                PublicKey = _configuration["Stripe:PublicKey"]
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Success(string session_id)
        {
            if (string.IsNullOrEmpty(session_id))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            try
            {
                // Procesar pago exitoso
                await _stripeService.ProcessPaymentSuccessAsync(session_id);

                TempData["SuccessMessage"] = "¡Pago completado con éxito! Tu saldo ha sido actualizado.";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al procesar el pago: {ex.Message}";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpGet]
        public IActionResult Cancel()
        {
            TempData["InfoMessage"] = "Pago cancelado.";
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Plans()
        {
            int userId = GetCurrentUserId();
            var plans = await _userService.GetAllPlansAsync();
            var currentSubscription = await _userService.GetActiveSubscriptionAsync(userId);

            var model = new PlansViewModel
            {
                Plans = plans,
                CurrentSubscription = currentSubscription
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(int planId)
        {
            int userId = GetCurrentUserId();

            try
            {
                // Crear sesión de checkout para suscripción
                string sessionId = await _stripeService.CreateSubscriptionCheckoutAsync(userId, planId);
                return Redirect($"/Payment/Checkout?sessionId={sessionId}");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al crear suscripción: {ex.Message}";
                return RedirectToAction("Plans");
            }
        }

        [HttpGet]
        public async Task<IActionResult> SubscriptionSuccess(string session_id)
        {
            if (string.IsNullOrEmpty(session_id))
            {
                return RedirectToAction("Plans");
            }

            try
            {
                // Procesar suscripción exitosa
                await _stripeService.ProcessSubscriptionSuccessAsync(session_id);

                TempData["SuccessMessage"] = "¡Suscripción activada con éxito!";
                return RedirectToAction("Plans");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al procesar la suscripción: {ex.Message}";
                return RedirectToAction("Plans");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelSubscription(string subscriptionId)
        {
            try
            {
                await _stripeService.CancelSubscriptionAsync(subscriptionId);
                TempData["SuccessMessage"] = "Suscripción cancelada con éxito.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cancelar la suscripción: {ex.Message}";
            }

            return RedirectToAction("Plans");
        }

        private int GetCurrentUserId()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return 0;
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return 0;
            }

            return int.Parse(userIdClaim.Value);
        }
    }
}