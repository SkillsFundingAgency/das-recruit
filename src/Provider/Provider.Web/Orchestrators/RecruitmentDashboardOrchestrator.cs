using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class RecruitmentDashboardOrchestrator
    {
        private const int ClosingSoonDays = 5;
        private readonly IProviderVacancyClient _client;
        private readonly ITimeProvider _timeProvider;

        public RecruitmentDashboardOrchestrator(IProviderVacancyClient client, ITimeProvider timeProvider)
        {
            _client = client;
            _timeProvider = timeProvider;
        }

        public async Task<RecruitmentDashboardViewModel> GetDashboardViewModelAsync(long ukprn)
        {
            List<VacancySummary> vacancies = await GetVacanciesAsync(ukprn);

            var vm = new RecruitmentDashboardViewModel
            {
                Vacancies = vacancies,
                HasAnyVacancies = vacancies.Any(),
                NoOfVacanciesClosingSoonWithNoApplications = vacancies.Count(v =>
                    v.ClosingDate <= _timeProvider.Today.AddDays(ClosingSoonDays) &&
                    v.Status == VacancyStatus.Live &&
                    v.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship &&
                    v.NoOfApplications == 0),
                NoOfVacanciesClosingSoon = vacancies.Count(v =>
                    v.ClosingDate <= _timeProvider.Today.AddDays(ClosingSoonDays) &&
                    v.Status == VacancyStatus.Live)
            };
            return vm;
        }

        private async Task<List<VacancySummary>> GetVacanciesAsync(long ukprn)
        {
            var dashboard = await _client.GetDashboardAsync(ukprn);

            if (dashboard == null)
            {
                await _client.GenerateDashboard(ukprn);
                dashboard = await _client.GetDashboardAsync(ukprn);
            }

            return dashboard?.Vacancies?.ToList() ?? new List<VacancySummary>();
        }      
    }
}