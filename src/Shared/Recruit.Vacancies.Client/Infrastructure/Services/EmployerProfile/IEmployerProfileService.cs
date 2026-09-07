using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerProfile;

public interface IEmployerProfileService
{
    Task CreateAsync(Domain.Entities.EmployerProfile profile);
    Task<IList<Domain.Entities.EmployerProfile>> GetEmployerProfilesForEmployerAsync(string employerAccountId);
    Task<Domain.Entities.EmployerProfile> GetAsync(string employerAccountId, string accountLegalEntityPublicHashedId);
    Task UpdateAsync(Domain.Entities.EmployerProfile profile);
}