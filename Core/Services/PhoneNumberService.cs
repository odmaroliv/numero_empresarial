using NumeroEmpresarial.Core.Interfaces;
using NumeroEmpresarial.Domain.Entities;
using NumeroEmpresarial.Domain.Enums;

namespace NumeroEmpresarial.Core.Services
{
    public class PhoneNumberService : IPhoneNumberService
    {
        public Task<PhoneNumber> AddPhoneNumberAsync(Guid userId, string number, string plivoId, string redirectionNumber, decimal monthlyCost = 1.99M, PhoneNumberType type = PhoneNumberType.Standard)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeactivatePhoneNumberAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<PhoneNumber>> GetActivePhoneNumbersForUserAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<PhoneNumber> GetPhoneNumberByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<PhoneNumber> GetPhoneNumberByNumberAsync(string number)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUserByPhoneNumberAsync(string phoneNumber)
        {
            throw new NotImplementedException();
        }

        public Task<List<PhoneNumber>> GetUserPhoneNumbersAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateRedirectionNumberAsync(Guid id, string redirectionNumber)
        {
            throw new NotImplementedException();
        }
    }
}
