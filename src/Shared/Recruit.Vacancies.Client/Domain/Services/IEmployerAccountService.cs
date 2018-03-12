using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Domain.Services
{
    public interface IEmployerAccountService
    {
        Task<IDictionary<string, EmployerIdentifier>> GetEmployerIdentifiersAsync(string userId);
    }
}
