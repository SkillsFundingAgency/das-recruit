using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount
{
    public interface IEmployerAccountProvider
    {
        Task<GetUserAccountsResponse> GetEmployerIdentifiersAsync(string userId, string email);
        Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string hashedAccountId);
        Task<IEnumerable<AccountLegalEntity>> GetLegalEntitiesConnectedToAccountAsync(string hashedAccountId);
    }
}
