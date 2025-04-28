using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Dashboard;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount
{
    public interface IEmployerAccountProvider
    {
        Task<GetUserAccountsResponse> GetEmployerIdentifiersAsync(string userId, string email);
        Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string hashedAccountId);
        Task<string> GetEmployerAccountPublicHashedIdAsync(string hashedAccountId);
        Task<IEnumerable<AccountLegalEntity>> GetLegalEntitiesConnectedToAccountAsync(string hashedAccountId);
        Task<DashboardApplicationReviewStats> GetEmployerDashboardApplicationReviewStats(string hashedAccountId,
            List<long> vacancyReferences);
    }
}
