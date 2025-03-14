using NumeroEmpresarial.Domain.Entities;
using NumeroEmpresarial.Domain.Enums;

namespace NumeroEmpresarial.Core.Interfaces
{
    public interface IPhoneNumberService
    {
        Task<List<PhoneNumber>> GetUserPhoneNumbersAsync(Guid userId);
        Task<List<PhoneNumber>> GetActivePhoneNumbersForUserAsync(Guid userId);
        Task<PhoneNumber> GetPhoneNumberByIdAsync(Guid id);
        Task<PhoneNumber> GetPhoneNumberByNumberAsync(string number);
        Task<PhoneNumber> AddPhoneNumberAsync(Guid userId, string number, string plivoId, string redirectionNumber, decimal monthlyCost = 1.99m, PhoneNumberType type = PhoneNumberType.Standard);
        Task<bool> DeactivatePhoneNumberAsync(Guid id);
        Task<bool> UpdateRedirectionNumberAsync(Guid id, string redirectionNumber);
        Task<User> GetUserByPhoneNumberAsync(string phoneNumber);
    }
}