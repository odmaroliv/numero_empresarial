using NumeroEmpresarial.Domain.DTOs;
using NumeroEmpresarial.Domain.Entities;

namespace NumeroEmpresarial.Core.Interfaces
{
    public interface IMessageWindowService
    {
        Task<MessageWindow> CreateMessageWindowAsync(Guid phoneNumberId, int durationMinutes, int maxMessages, decimal windowCost);
        Task<List<MessageWindowDto>> GetActiveWindowsAsync(Guid phoneNumberId);
        Task<bool> IsWindowActiveAsync(Guid windowId);
        Task<MessageWindow> GetWindowByIdAsync(Guid windowId);
        Task<MessageWindow> GetActiveWindowForPhoneNumberAsync(string phoneNumber);
        Task<Message> RecordMessageAsync(Guid windowId, string from, string text, decimal messageCost);
        Task<List<Message>> GetWindowMessagesAsync(Guid windowId);
        Task<bool> CloseWindowAsync(Guid windowId);
        Task<List<MessageWindowDto>> GetActiveWindowsForUserAsync(Guid userId);
        Task<List<MessageWindowDto>> GetWindowsByPhoneNumberIdAsync(Guid phoneNumberId);
        Task<int> GetTotalMessagesCountForUserAsync(Guid userId);
    }
}