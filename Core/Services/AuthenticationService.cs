using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.IdentityModel.Tokens;
using NumeroEmpresarial.Core.Interfaces;
using NumeroEmpresarial.Domain.DTOs;
using NumeroEmpresarial.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IAuthenticationService = NumeroEmpresarial.Core.Interfaces.IAuthenticationService;

namespace NumeroEmpresarial.Core.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ProtectedLocalStorage _localStorage;

        public AuthenticationService(
            IUserService userService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            AuthenticationStateProvider authenticationStateProvider,
            ProtectedLocalStorage localStorage)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
        }

        public async Task<AuthResponseDto> AuthenticateAsync(string email, string password)
        {
            var user = await _userService.AuthenticateAsync(email, password);
            if (user == null)
            {
                return null;
            }

            // Generar token JWT
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // Guardar refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userService.UpdateUserAsync(user);

            // Crear respuesta de autenticación
            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationMinutes"] ?? "60")),
                User = new UserDto
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
                }
            };
        }

        public async Task<bool> SignInAsync(User user, bool rememberMe = false)
        {
            try
            {
                // Crear claims para la identidad
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("Language", user.Language),
                    new Claim("ApiKey", user.ApiKey)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = rememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(1)
                };

                // Iniciar sesión
                await _httpContextAccessor.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Actualizar último acceso
                user.LastLogin = DateTime.UtcNow;
                await _userService.UpdateUserAsync(user);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task SignOutAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await _localStorage.DeleteAsync("auth-token");
            await _localStorage.DeleteAsync("refresh-token");
        }

        public async Task<Guid> GetCurrentUserIdAsync()
        {
            // Para solicitudes HTTP regulares
            if (_httpContextAccessor.HttpContext != null)
            {
                var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }
            }

            // Para componentes Blazor
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var userIdClaimBlazor = authState.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaimBlazor != null && Guid.TryParse(userIdClaimBlazor.Value, out var userIdBlazor))
            {
                return userIdBlazor;
            }

            // Intentar obtener de localStorage como último recurso (para SPA)
            try
            {
                var tokenResult = await _localStorage.GetAsync<string>("auth-token");
                if (tokenResult.Success && !string.IsNullOrEmpty(tokenResult.Value))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(tokenResult.Value);
                    var userIdTokenClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                    if (userIdTokenClaim != null && Guid.TryParse(userIdTokenClaim.Value, out var userIdToken))
                    {
                        return userIdToken;
                    }
                }
            }
            catch
            {
                // Ignorar errores de localStorage
            }

            throw new UnauthorizedAccessException("Usuario no autenticado o ID de usuario no válido");
        }

        public async Task<User> GetCurrentUserAsync()
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                return await _userService.GetUserByIdAsync(userId);
            }
            catch
            {
                return null;
            }
        }

        public string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expirationMinutes = Convert.ToDouble(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Language", user.Language),
                new Claim("ApiKey", user.ApiKey)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string token, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            if (principal == null)
            {
                return null;
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return null;
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }

            var newToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            // Actualizar refresh token
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userService.UpdateUserAsync(user);

            return new AuthResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationMinutes"] ?? "60")),
                User = new UserDto
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
                }
            };
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // No validar el tiempo de expiración
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
    }
}