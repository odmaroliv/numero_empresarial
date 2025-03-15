using Microsoft.EntityFrameworkCore;
using NumeroEmpresarial.Core.Interfaces;
using NumeroEmpresarial.Data;
using NumeroEmpresarial.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace NumeroEmpresarial.Core.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            try
            {
                return await _context.Users.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener usuario por ID: {id}");
                throw new ApplicationException($"Error al obtener usuario: {ex.Message}", ex);
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener usuario por email: {email}");
                throw new ApplicationException($"Error al obtener usuario: {ex.Message}", ex);
            }
        }

        public async Task<User> GetUserByApiKeyAsync(string apiKey)
        {
            try
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.ApiKey == apiKey && u.Active);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener usuario por API key: {apiKey}");
                throw new ApplicationException($"Error al obtener usuario: {ex.Message}", ex);
            }
        }

        public async Task<User> AuthenticateAsync(string email, string password)
        {
            try
            {
                var user = await GetUserByEmailAsync(email);
                if (user == null)
                {
                    return null;
                }

                if (!VerifyPassword(password, user.PasswordHash))
                {
                    return null;
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al autenticar usuario: {email}");
                throw new ApplicationException($"Error al autenticar usuario: {ex.Message}", ex);
            }
        }

        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                // Verificar si ya existe un usuario con el mismo email
                var existingUser = await GetUserByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    throw new ApplicationException("Ya existe un usuario con ese email");
                }

                // Generar API key si no tiene
                if (string.IsNullOrEmpty(user.ApiKey))
                {
                    user.ApiKey = Guid.NewGuid().ToString("N");
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                throw new ApplicationException($"Error al crear usuario: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar usuario: {user.Id}");
                throw new ApplicationException($"Error al actualizar usuario: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            try
            {
                var user = await GetUserByIdAsync(id);
                if (user == null)
                {
                    return false;
                }

                user.Active = false;
                await UpdateUserAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar usuario: {id}");
                throw new ApplicationException($"Error al eliminar usuario: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateLastLoginAsync(Guid id)
        {
            try
            {
                var user = await GetUserByIdAsync(id);
                if (user == null)
                {
                    return false;
                }

                user.LastLogin = DateTime.UtcNow;
                await UpdateUserAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar último acceso: {id}");
                throw new ApplicationException($"Error al actualizar último acceso: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateApiKeyAsync(Guid id, string apiKey)
        {
            try
            {
                var user = await GetUserByIdAsync(id);
                if (user == null)
                {
                    return false;
                }

                user.ApiKey = apiKey;
                await UpdateUserAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar API key: {id}");
                throw new ApplicationException($"Error al actualizar API key: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeductBalanceAsync(Guid id, decimal amount, string description)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var user = await _context.Users.FindAsync(id);
                if (user == null || user.Balance < amount)
                {
                    return false;
                }

                user.Balance -= amount;

                // Registrar transacción
                var newTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = id,
                    Amount = -amount,
                    Description = description,
                    TransactionDate = DateTime.UtcNow,
                    Type = Domain.Enums.TransactionType.MessageCharge,
                    Successful = true
                };

                _context.Transactions.Add(newTransaction);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al deducir saldo: {id}, {amount}");
                throw new ApplicationException($"Error al deducir saldo: {ex.Message}", ex);
            }
        }

        public async Task<bool> AddBalanceAsync(Guid id, decimal amount, string description)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return false;
                }

                user.Balance += amount;

                // Registrar transacción
                var newTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = id,
                    Amount = amount,
                    Description = description,
                    TransactionDate = DateTime.UtcNow,
                    Type = Domain.Enums.TransactionType.Deposit,
                    Successful = true
                };

                _context.Transactions.Add(newTransaction);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al agregar saldo: {id}, {amount}");
                throw new ApplicationException($"Error al agregar saldo: {ex.Message}", ex);
            }
        }

        public async Task<Subscription> GetActiveSubscriptionAsync(Guid userId)
        {
            try
            {
                return await _context.Subscriptions
                    .Include(s => s.Plan)
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.Active);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener suscripción activa: {userId}");
                throw new ApplicationException($"Error al obtener suscripción: {ex.Message}", ex);
            }
        }

        public async Task<List<Plan>> GetAllPlansAsync()
        {
            try
            {
                return await _context.Plans.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener planes");
                throw new ApplicationException($"Error al obtener planes: {ex.Message}", ex);
            }
        }

        public async Task<List<Transaction>> GetRecentTransactionsAsync(Guid userId, int count)
        {
            try
            {
                return await _context.Transactions
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener transacciones recientes: {userId}");
                throw new ApplicationException($"Error al obtener transacciones: {ex.Message}", ex);
            }
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            string hashedInput = HashPassword(password);
            return hashedInput == passwordHash;
        }
    }
}