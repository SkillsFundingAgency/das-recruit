using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IProviderVacancyClient
    {
        Task<Guid> CreateVacancyAsync(SourceOrigin origin, string employerAccountId, long ukprn, string title, int numberOfPositions, VacancyUser user);
        Task GenerateDashboard(long ukprn);
        Task<ProviderDashboard> GetDashboardAsync(long ukprn);
        Task SetupProviderAsync(long ukprn);
        Task UpdateDraftVacancyAsync(Vacancy vacancy, VacancyUser user);
        Task<ProviderEditVacancyInfo> GetProviderEditVacancyInfoAsync(long ukprn);
    }
}