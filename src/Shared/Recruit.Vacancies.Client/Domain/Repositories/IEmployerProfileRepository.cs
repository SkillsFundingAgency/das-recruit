using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IEmployerProfileRepository
    {
        Task CreateAsync(EmployerProfile profile);
        Task<IList<EmployerProfile>> GetEmployerProfilesForEmployerAsync(string employerAccountId);
        Task<EmployerProfile> GetAsync(string employerAccountId, string accountLegalEntityPublicHashedId);
        Task UpdateAsync(EmployerProfile profile);
    }
}