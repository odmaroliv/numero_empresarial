using Microsoft.EntityFrameworkCore;
using NumeroEmpresarial.Core.Interfaces;
using NumeroEmpresarial.Data;
using NumeroEmpresarial.Domain.Entities;
using NumeroEmpresarial.Domain.Enums;

namespace NumeroEmpresarial.Core.Services
{
    public class PhoneNumberService : IPhoneNumberService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PhoneNumberService> _logger;

        public PhoneNumberService(ApplicationDbContext context, ILogger<PhoneNumberService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PhoneNumber>> GetUserPhoneNumbersAsync(Guid userId)
        {
            try
            {
                return await _context.PhoneNumbers
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.AcquisitionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener números de usuario: {userId}");
                throw new ApplicationException($"Error al obtener números: {ex.Message}", ex);
            }
        }

        public async Task<List<PhoneNumber>> GetActivePhoneNumbersForUserAsync(Guid userId)
        {
            try
            {
                return await _context.PhoneNumbers
                    .Where(p => p.UserId == userId && p.Active)
                    .OrderByDescending(p => p.AcquisitionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener números activos: {userId}");
                throw new ApplicationException($"Error al obtener números activos: {ex.Message}", ex);
            }
        }

        public async Task<PhoneNumber> GetPhoneNumberByIdAsync(Guid id)
        {
            try
            {
                return await _context.PhoneNumbers.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener número por ID: {id}");
                throw new ApplicationException($"Error al obtener número: {ex.Message}", ex);
            }
        }

        public async Task<PhoneNumber> GetPhoneNumberByNumberAsync(string number)
        {
            try
            {
                return await _context.PhoneNumbers
                    .FirstOrDefaultAsync(p => p.Number == number && p.Active);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener número por número telefónico: {number}");
                throw new ApplicationException($"Error al obtener número: {ex.Message}", ex);
            }
        }

        public async Task<PhoneNumber> AddPhoneNumberAsync(Guid userId, string number, string plivoId, string redirectionNumber, decimal monthlyCost = 1.99m, PhoneNumberType type = PhoneNumberType.Standard)
        {
            try
            {
                // Verificar que el usuario exista
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new ApplicationException("Usuario no encontrado");
                }

                // Verificar el saldo del usuario
                if (user.Balance < monthlyCost)
                {
                    throw new ApplicationException("Saldo insuficiente");
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                // Crear el número de teléfono
                var phoneNumber = new PhoneNumber
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Number = number,
                    PlivoId = plivoId,
                    AcquisitionDate = DateTime.UtcNow,
                    ExpirationDate = DateTime.UtcNow.AddMonths(1),
                    Active = true,
                    RedirectionNumber = redirectionNumber,
                    Type = type,
                    MonthlyCost = monthlyCost
                };

                // Deducir el costo del saldo del usuario
                user.Balance -= monthlyCost;

                // Registrar la transacción
                var userTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Amount = -monthlyCost,
                    TransactionDate = DateTime.UtcNow,
                    Type = TransactionType.PhoneNumberPurchase,
                    Description = $"Adquisición de número {number}",
                    Successful = true
                };

                _context.PhoneNumbers.Add(phoneNumber);
                _context.Transactions.Add(userTransaction);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return phoneNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al agregar número de teléfono: {number}, {userId}");
                throw new ApplicationException($"Error al agregar número: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeactivatePhoneNumberAsync(Guid id)
        {
            try
            {
                var phoneNumber = await _context.PhoneNumbers.FindAsync(id);
                if (phoneNumber == null)
                {
                    return false;
                }

                phoneNumber.Active = false;
                _context.PhoneNumbers.Update(phoneNumber);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al desactivar número de teléfono: {id}");
                throw new ApplicationException($"Error al desactivar número: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateRedirectionNumberAsync(Guid id, string redirectionNumber)
        {
            try
            {
                var phoneNumber = await _context.PhoneNumbers.FindAsync(id);
                if (phoneNumber == null)
                {
                    return false;
                }

                phoneNumber.RedirectionNumber = redirectionNumber;
                _context.PhoneNumbers.Update(phoneNumber);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar número de redirección: {id}");
                throw new ApplicationException($"Error al actualizar redirección: {ex.Message}", ex);
            }
        }

        public async Task<User> GetUserByPhoneNumberAsync(string phoneNumber)
        {
            try
            {
                var number = await _context.PhoneNumbers
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Number == phoneNumber && p.Active);

                return number?.User;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener usuario por número: {phoneNumber}");
                throw new ApplicationException($"Error al obtener usuario: {ex.Message}", ex);
            }
        }
    }
}