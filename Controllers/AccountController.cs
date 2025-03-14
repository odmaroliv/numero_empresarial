using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NumeroEmpresarial.Core.Interfaces;
using NumeroEmpresarial.Domain.Entities;
using System.Security.Claims;

namespace NumeroEmpresarial.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IStripeService _stripeService;

        public AccountController(IUserService userService, IStripeService stripeService)
        {
            _userService = userService;
            _stripeService = stripeService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userService.AuthenticateAsync(model.Email, model.Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return View(model);
            }

            // Crear claims de identidad
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Actualizar último acceso
            await _userService.UpdateLastLoginAsync(user.Id);

            return RedirectToLocal(returnUrl);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Comprobar si el email ya está registrado
            var existingUser = await _userService.GetUserByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Este correo electrónico ya está registrado.");
                return View(model);
            }

            // Crear nuevo usuario
            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                PasswordHash = _userService.HashPassword(model.Password),
                RegistrationDate = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                ApiKey = Guid.NewGuid().ToString("N"),
                Active = true,
                Balance = 0
            };

            await _userService.CreateUserAsync(user);

            // Iniciar sesión automáticamente
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Balance = user.Balance,
                ApiKey = user.ApiKey
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.Name = model.Name;
            user.Phone = model.Phone;

            // Si se proporcionó una nueva contraseña, actualizarla
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                user.PasswordHash = _userService.HashPassword(model.NewPassword);
            }

            await _userService.UpdateUserAsync(user);

            TempData["SuccessMessage"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public async Task<IActionResult> RechargeBalance()
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new RechargeBalanceViewModel
            {
                CurrentBalance = user.Balance
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RechargeBalance(RechargeBalanceViewModel model)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid || model.Amount <= 0)
            {
                return View(model);
            }

            // Crear sesión de pago con Stripe
            string sessionId = await _stripeService.CreateCheckoutSessionAsync(
                userId,
                model.Amount,
                "Recarga de saldo");

            return Redirect($"/Payment/Checkout?sessionId={sessionId}");
        }

        [HttpGet]
        public async Task<IActionResult> RegenerateApiKey()
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login");
            }

            var newApiKey = Guid.NewGuid().ToString("N");
            await _userService.UpdateApiKeyAsync(userId, newApiKey);

            TempData["SuccessMessage"] = "API Key regenerada correctamente.";
            return RedirectToAction("Profile");
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

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
    }
}