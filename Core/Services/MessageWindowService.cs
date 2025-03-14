using NumeroEmpresarial.Core.Interfaces;
using NumeroEmpresarial.Domain.DTOs;
using NumeroEmpresarial.Domain.Entities;

namespace NumeroEmpresarial.Core.Services
{
    public class MessageWindowService : IMessageWindowService
    {
        public Task<bool> CloseWindowAsync(Guid windowId)
        {
            throw new NotImplementedException();
        }

        public Task<MessageWindow> CreateMessageWindowAsync(Guid phoneNumberId, int durationMinutes, int maxMessages, decimal windowCost)
        {
            throw new NotImplementedException();
        }

        public Task<MessageWindow> GetActiveWindowForPhoneNumberAsync(string phoneNumber)
        {
            throw new NotImplementedException();
        }

        public Task<List<MessageWindowDto>> GetActiveWindowsAsync(Guid phoneNumberId)
        {
            throw new NotImplementedException();
        }

        public Task<List<MessageWindowDto>> GetActiveWindowsForUserAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalMessagesCountForUserAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<MessageWindow> GetWindowByIdAsync(Guid windowId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Message>> GetWindowMessagesAsync(Guid windowId)
        {
            throw new NotImplementedException();
        }

        public Task<List<MessageWindowDto>> GetWindowsByPhoneNumberIdAsync(Guid phoneNumberId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsWindowActiveAsync(Guid windowId)
        {
            throw new NotImplementedException();
        }

        public Task<Message> RecordMessageAsync(Guid windowId, string from, string text, decimal messageCost)
        {
            throw new NotImplementedException();
        }
    }
}
