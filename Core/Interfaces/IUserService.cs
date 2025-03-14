using NumeroEmpresarial.Domain.Entities;

namespace NumeroEmpresarial.Core.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(Guid id);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByApiKeyAsync(string apiKey);
        Task<User> AuthenticateAsync(string email, string password);
        Task<User> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(Guid id);
        Task<bool> UpdateLastLoginAsync(Guid id);
        Task<bool> UpdateApiKeyAsync(Guid id, string apiKey);
        Task<bool> DeductBalanceAsync(Guid id, decimal amount, string description);
        Task<bool> AddBalanceAsync(Guid id, decimal amount, string description);
        Task<Subscription> GetActiveSubscriptionAsync(Guid userId);
        Task<List<Plan>> GetAllPlansAsync();
        Task<List<Transaction>> GetRecentTransactionsAsync(Guid userId, int count);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}