using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IProviderVacancyClient
    {
        Task<Guid> CreateVacancyAsync(SourceOrigin origin, string title, int numberOfPositions, long ukprn, VacancyUser user, UserType userType);
        Task GenerateDashboard(long ukprn);

        Task<ProviderDashboard> GetDashboardAsync(long ukprn);
    }
}