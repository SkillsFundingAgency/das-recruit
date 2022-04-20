using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using SFA.DAS.EAS.Account.Api.Types;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount
{
    public interface IEmployerAccountProvider
    {
        Task<IEnumerable<string>> GetEmployerIdentifiersAsync(string userId);
        Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string accountId);
        Task<string> GetEmployerAccountPublicHashedIdAsync(long accountId);
        Task<IEnumerable<AccountLegalEntity>> GetLegalEntitiesConnectedToAccountAsync(string accountId);
    }
}
