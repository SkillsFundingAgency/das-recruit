using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount
{
    public interface IEmployerAccountProvider
    {
        Task<IEnumerable<string>> GetEmployerIdentifiersAsync(string userId);
        Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string accountId);
    }
}
