using Microsoft.EntityFrameworkCore;
using NumeroEmpresarial.Core.Interfaces;
using NumeroEmpresarial.Data;
using NumeroEmpresarial.Domain.DTOs;
using NumeroEmpresarial.Domain.Entities;
using NumeroEmpresarial.Domain.Enums;

namespace NumeroEmpresarial.Core.Services
{
    public class MessageWindowService : IMessageWindowService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MessageWindowService> _logger;

        public MessageWindowService(ApplicationDbContext context, ILogger<MessageWindowService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MessageWindow> CreateMessageWindowAsync(Guid phoneNumberId, int durationMinutes, int maxMessages, decimal windowCost)
        {
            try
            {
                // Verificar que el número exista y esté activo
                var phoneNumber = await _context.PhoneNumbers
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == phoneNumberId && p.Active);

                if (phoneNumber == null)
                {
                    throw new ApplicationException("Número de teléfono no encontrado o inactivo");
                }

                // Verificar que el usuario tenga saldo suficiente
                if (phoneNumber.User.Balance < windowCost)
                {
                    throw new ApplicationException("Saldo insuficiente");
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                // Crear la ventana de mensajes
                var window = new MessageWindow
                {
                    Id = Guid.NewGuid(),
                    PhoneNumberId = phoneNumberId,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddMinutes(durationMinutes),
                    Active = true,
                    MaxMessages = maxMessages,
                    ReceivedMessages = 0,
                    WindowCost = windowCost
                };

                // Deducir el costo del saldo del usuario
                phoneNumber.User.Balance -= windowCost;

                // Registrar la transacción
                var transaction1 = new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = phoneNumber.UserId,
                    Amount = -windowCost,
                    TransactionDate = DateTime.UtcNow,
                    Type = TransactionType.MessageWindowCreation,
                    Description = $"Creación de ventana de mensajes para {phoneNumber.Number}",
                    Successful = true
                };

                _context.MessageWindows.Add(window);
                _context.Transactions.Add(transaction1);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return window;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al crear ventana de mensajes: {phoneNumberId}");
                throw new ApplicationException($"Error al crear ventana: {ex.Message}", ex);
            }
        }

        public async Task<List<MessageWindowDto>> GetActiveWindowsAsync(Guid phoneNumberId)
        {
            try
            {
                var now = DateTime.UtcNow;
                var windows = await _context.MessageWindows
                    .Include(w => w.PhoneNumber)
                    .Include(w => w.Messages)
                    .Where(w => w.PhoneNumberId == phoneNumberId && w.Active && w.EndTime > now && w.ReceivedMessages < w.MaxMessages)
                    .OrderBy(w => w.EndTime)
                    .ToListAsync();

                return windows.Select(w => MapToDto(w)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener ventanas activas: {phoneNumberId}");
                throw new ApplicationException($"Error al obtener ventanas: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsWindowActiveAsync(Guid windowId)
        {
            try
            {
                var window = await _context.MessageWindows.FindAsync(windowId);
                if (window == null)
                {
                    return false;
                }

                var now = DateTime.UtcNow;
                return window.Active && window.EndTime > now && window.ReceivedMessages < window.MaxMessages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al verificar si la ventana está activa: {windowId}");
                throw new ApplicationException($"Error al verificar ventana: {ex.Message}", ex);
            }
        }

        public async Task<MessageWindow> GetWindowByIdAsync(Guid windowId)
        {
            try
            {
                return await _context.MessageWindows
                    .Include(w => w.PhoneNumber)
                    .Include(w => w.Messages)
                    .FirstOrDefaultAsync(w => w.Id == windowId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener ventana por ID: {windowId}");
                throw new ApplicationException($"Error al obtener ventana: {ex.Message}", ex);
            }
        }

        public async Task<MessageWindow> GetActiveWindowForPhoneNumberAsync(string phoneNumber)
        {
            try
            {
                var now = DateTime.UtcNow;
                return await _context.MessageWindows
                    .Include(w => w.PhoneNumber)
                    .Include(w => w.Messages)
                    .Where(w => w.PhoneNumber.Number == phoneNumber && w.Active && w.EndTime > now && w.ReceivedMessages < w.MaxMessages)
                    .OrderBy(w => w.EndTime)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener ventana activa para número: {phoneNumber}");
                throw new ApplicationException($"Error al obtener ventana: {ex.Message}", ex);
            }
        }

        public async Task<Message> RecordMessageAsync(Guid windowId, string from, string text, decimal messageCost)
        {
            try
            {
                var window = await _context.MessageWindows
                    .Include(w => w.PhoneNumber)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(w => w.Id == windowId);

                if (window == null)
                {
                    throw new ApplicationException("Ventana no encontrada");
                }

                // Verificar si la ventana está activa
                if (!window.Active || window.EndTime <= DateTime.UtcNow || window.ReceivedMessages >= window.MaxMessages)
                {
                    throw new ApplicationException("La ventana no está activa o ha alcanzado su límite");
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                // Crear el mensaje
                var message = new Message
                {
                    Id = Guid.NewGuid(),
                    MessageWindowId = windowId,
                    From = from,
                    Text = text,
                    ReceivedTime = DateTime.UtcNow,
                    Redirected = false,
                    MessageCost = messageCost
                };

                // Incrementar contador de mensajes
                window.ReceivedMessages++;

                // Deducir el costo del mensaje del saldo del usuario
                window.PhoneNumber.User.Balance -= messageCost;

                // Registrar la transacción
                var userTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = window.PhoneNumber.UserId,
                    Amount = -messageCost,
                    TransactionDate = DateTime.UtcNow,
                    Type = TransactionType.MessageCharge,
                    Description = $"Cargo por mensaje a {window.PhoneNumber.Number}",
                    Successful = true
                };

                _context.Messages.Add(message);
                _context.Transactions.Add(userTransaction);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al registrar mensaje: {windowId}");
                throw new ApplicationException($"Error al registrar mensaje: {ex.Message}", ex);
            }
        }

        public async Task<List<Message>> GetWindowMessagesAsync(Guid windowId)
        {
            try
            {
                return await _context.Messages
                    .Where(m => m.MessageWindowId == windowId)
                    .OrderByDescending(m => m.ReceivedTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener mensajes de ventana: {windowId}");
                throw new ApplicationException($"Error al obtener mensajes: {ex.Message}", ex);
            }
        }

        public async Task<bool> CloseWindowAsync(Guid windowId)
        {
            try
            {
                var window = await _context.MessageWindows.FindAsync(windowId);
                if (window == null)
                {
                    return false;
                }

                window.Active = false;
                window.EndTime = DateTime.UtcNow;
                _context.MessageWindows.Update(window);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cerrar ventana: {windowId}");
                throw new ApplicationException($"Error al cerrar ventana: {ex.Message}", ex);
            }
        }

        public async Task<List<MessageWindowDto>> GetActiveWindowsForUserAsync(Guid userId)
        {
            try
            {
                var now = DateTime.UtcNow;
                var windows = await _context.MessageWindows
                    .Include(w => w.PhoneNumber)
                    .Include(w => w.Messages)
                    .Where(w => w.PhoneNumber.UserId == userId && w.Active && w.EndTime > now && w.ReceivedMessages < w.MaxMessages)
                    .OrderBy(w => w.EndTime)
                    .ToListAsync();

                return windows.Select(w => MapToDto(w)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener ventanas activas para usuario: {userId}");
                throw new ApplicationException($"Error al obtener ventanas: {ex.Message}", ex);
            }
        }

        public async Task<List<MessageWindowDto>> GetWindowsByPhoneNumberIdAsync(Guid phoneNumberId)
        {
            try
            {
                var windows = await _context.MessageWindows
                    .Include(w => w.PhoneNumber)
                    .Include(w => w.Messages)
                    .Where(w => w.PhoneNumberId == phoneNumberId)
                    .OrderByDescending(w => w.StartTime)
                    .ToListAsync();

                return windows.Select(w => MapToDto(w)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener ventanas por número: {phoneNumberId}");
                throw new ApplicationException($"Error al obtener ventanas: {ex.Message}", ex);
            }
        }

        public async Task<int> GetTotalMessagesCountForUserAsync(Guid userId)
        {
            try
            {
                return await _context.Messages
                    .Where(m => m.MessageWindow.PhoneNumber.UserId == userId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener total de mensajes para usuario: {userId}");
                throw new ApplicationException($"Error al obtener total de mensajes: {ex.Message}", ex);
            }
        }

        private MessageWindowDto MapToDto(MessageWindow window)
        {
            return new MessageWindowDto
            {
                Id = window.Id,
                PhoneNumberId = window.PhoneNumberId,
                StartTime = window.StartTime,
                EndTime = window.EndTime,
                Active = window.Active,
                MaxMessages = window.MaxMessages,
                ReceivedMessages = window.ReceivedMessages,
                WindowCost = window.WindowCost,
                PhoneNumber = window.PhoneNumber,
                Messages = window.Messages?.Select(m => new MessageDto
                {
                    Id = m.Id,
                    MessageWindowId = m.MessageWindowId,
                    From = m.From,
                    Text = m.Text,
                    ReceivedTime = m.ReceivedTime,
                    Redirected = m.Redirected,
                    MessageCost = m.MessageCost
                }).ToList() ?? new List<MessageDto>()
            };
        }
    }
}