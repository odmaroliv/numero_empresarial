using Microsoft.AspNetCore.Mvc;
using NumeroEmpresarial.Core.Interfaces;
using NumeroEmpresarial.Domain.DTOs;
using NumeroEmpresarial.Domain.Entities;

namespace NumeroEmpresarial.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IStripeService _stripeService;
        private readonly Core.Interfaces.IAuthenticationService _authService;

        public AccountController(
            IUserService userService,
            IStripeService stripeService,
            Core.Interfaces.IAuthenticationService authService)
        {
            _userService = userService;
            _stripeService = stripeService;
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model, string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Primero autenticamos para verificar las credenciales
            var authResponse = await _authService.AuthenticateAsync(model.Email, model.Password);

            if (authResponse == null)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return View(model);
            }

            // Obtenemos el usuario real (entidad) en lugar de usar el DTO
            var user = await _userService.GetUserByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Error al iniciar sesión.");
                return View(model);
            }

            // Iniciar sesión con cookies usando la entidad User
            await _authService.SignInAsync(user, model.RememberMe);

            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return LocalRedirect(returnUrl);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserCreateUpdateDto model)
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
                Id = Guid.NewGuid(),
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                PasswordHash = _userService.HashPassword(model.Password),
                RegistrationDate = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                ApiKey = Guid.NewGuid().ToString("N"),
                Active = true,
                Balance = 0,
                Language = model.Language ?? "es"
            };

            await _userService.CreateUserAsync(user);

            // Iniciar sesión automáticamente
            await _authService.SignInAsync(user, true);

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                var model = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Balance = user.Balance,
                    Language = user.Language,
                    Active = user.Active,
                    RegistrationDate = user.RegistrationDate,
                    LastLogin = user.LastLogin
                };

                return View(model);
            }
            catch
            {
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserCreateUpdateDto model)
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                if (!ModelState.IsValid)
                {
                    return View("Profile", model);
                }

                user.Name = model.Name;
                user.Phone = model.Phone;
                user.Language = model.Language ?? user.Language;

                // Si se proporcionó una nueva contraseña, actualizarla
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.PasswordHash = _userService.HashPassword(model.Password);
                }

                await _userService.UpdateUserAsync(user);

                TempData["SuccessMessage"] = "Perfil actualizado correctamente.";
                return RedirectToAction("Profile");
            }
            catch
            {
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public async Task<IActionResult> RechargeBalance()
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                var model = new RechargeBalanceDto
                {
                    Amount = 20 // Valor predeterminado
                };

                ViewBag.CurrentBalance = user.Balance;
                return View(model);
            }
            catch
            {
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RechargeBalance(RechargeBalanceDto model)
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                if (!ModelState.IsValid || model.Amount <= 0)
                {
                    ViewBag.CurrentBalance = user.Balance;
                    return View(model);
                }

                // Crear sesión de pago con Stripe
                string sessionId = await _stripeService.CreateCheckoutSessionAsync(
                    user.Id,
                    model.Amount,
                    "Recarga de saldo");

                return Redirect($"/Payment/Checkout?sessionId={sessionId}");
            }
            catch
            {
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public async Task<IActionResult> RegenerateApiKey()
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                var newApiKey = Guid.NewGuid().ToString("N");
                await _userService.UpdateApiKeyAsync(user.Id, newApiKey);

                TempData["SuccessMessage"] = "API Key regenerada correctamente.";
                return RedirectToAction("Profile");
            }
            catch
            {
                return RedirectToAction("Login");
            }
        }

        // API para obtener token JWT (para clientes móviles/SPA)
        [HttpPost("api/token")]
        public async Task<IActionResult> GetToken([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Credenciales inválidas" });
            }

            // Este método ya genera el token internamente
            var response = await _authService.AuthenticateAsync(model.Email, model.Password);

            if (response == null)
            {
                return Unauthorized(new { error = "Credenciales inválidas" });
            }

            return Ok(response);
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