using NumeroEmpresarial.Core.Interfaces;
using NumeroEmpresarial.Domain.Entities;

namespace NumeroEmpresarial.Core.Services
{
    public class UserService : IUserService
    {
        public Task<bool> AddBalanceAsync(Guid id, decimal amount, string description)
        {
            throw new NotImplementedException();
        }

        public Task<User> AuthenticateAsync(string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task<User> CreateUserAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeductBalanceAsync(Guid id, decimal amount, string description)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteUserAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Subscription> GetActiveSubscriptionAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Plan>> GetAllPlansAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Transaction>> GetRecentTransactionsAsync(Guid userId, int count)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUserByApiKeyAsync(string apiKey)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUserByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUserByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public string HashPassword(string password)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateApiKeyAsync(Guid id, string apiKey)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateLastLoginAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateUserAsync(User user)
        {
            throw new NotImplementedException();
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            throw new NotImplementedException();
        }
    }
}
