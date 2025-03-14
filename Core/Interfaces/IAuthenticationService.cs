using NumeroEmpresarial.Domain.DTOs;
using NumeroEmpresarial.Domain.Entities;

namespace NumeroEmpresarial.Core.Interfaces
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Autentica a un usuario mediante email y contraseña, generando un token JWT
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <returns>Respuesta con token JWT si la autenticación es exitosa, o null si falla</returns>
        Task<AuthResponseDto> AuthenticateAsync(string email, string password);

        /// <summary>
        /// Inicia sesión de un usuario en el sistema (autenticación basada en cookies)
        /// </summary>
        /// <param name="user">Usuario a autenticar</param>
        /// <param name="rememberMe">Indica si la sesión debe persistir</param>
        /// <returns>True si la autenticación es exitosa, false en caso contrario</returns>
        Task<bool> SignInAsync(User user, bool rememberMe = false);

        /// <summary>
        /// Cierra la sesión del usuario actual
        /// </summary>
        Task SignOutAsync();

        /// <summary>
        /// Obtiene el ID del usuario actual autenticado
        /// </summary>
        /// <returns>GUID del usuario actual</returns>
        /// <exception cref="UnauthorizedAccessException">Si el usuario no está autenticado</exception>
        Task<Guid> GetCurrentUserIdAsync();

        /// <summary>
        /// Obtiene el usuario actual autenticado
        /// </summary>
        /// <returns>Entidad de usuario o null si no está autenticado</returns>
        Task<User> GetCurrentUserAsync();

        /// <summary>
        /// Genera un token JWT para un usuario
        /// </summary>
        /// <param name="user">Usuario para el que se genera el token</param>
        /// <returns>Token JWT como string</returns>
        string GenerateJwtToken(User user);

        /// <summary>
        /// Renueva un token JWT utilizando un refresh token
        /// </summary>
        /// <param name="token">Token JWT expirado</param>
        /// <param name="refreshToken">Refresh token válido</param>
        /// <returns>Nueva respuesta con token JWT si es válido, o null si no es válido</returns>
        Task<AuthResponseDto> RefreshTokenAsync(string token, string refreshToken);
    }
}