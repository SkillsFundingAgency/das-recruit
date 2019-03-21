using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private const int VacanciesPerPage = 25;
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;

        public DashboardOrchestrator(IProviderVacancyClient client, IRecruitVacancyClient vacancyClient)
        {
            _client = client;
            _vacancyClient = vacancyClient;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(long ukprn, int page)
        {
            var dashboard = await _client.GetDashboardAsync(ukprn);

            if (dashboard == null)
            {
                await _client.GenerateDashboard(ukprn);
                dashboard = await _client.GetDashboardAsync(ukprn);
            }

            var totalVacancies = dashboard.Vacancies.Count();

            page = SanitizePage(page, totalVacancies);
            
            var skip = (page - 1) * VacanciesPerPage;

            var vacancies = dashboard?.Vacancies
                .Skip(skip)
                .Take(VacanciesPerPage)
                .Select(VacancySummaryMapper.ConvertToVacancySummaryViewModel)
                .OrderByDescending(v => v.CreatedDate)
                .ToList();

            var pager = new PagerViewModel(
                totalVacancies, 
                VacanciesPerPage,
                page, 
                "Showing {0} to {1} of {2} vacancies");

            var vm = new DashboardViewModel 
            {
                Vacancies = vacancies,
                Pager = pager
            };

            return vm;
        }

        private int SanitizePage(int page, int totalVacancies)
        {
            return (page < 0 || page > (int)Math.Ceiling((double)totalVacancies / VacanciesPerPage)) ? 1 : page;
        }

        public async Task<string> GetVacancyRedirectRouteAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(vrm.VacancyId.GetValueOrDefault());

            Utility.CheckAuthorisedAccess(vacancy, vrm.Ukprn);

            if(vacancy.CanEdit == false)
                throw new InvalidStateException(ErrorMessages.VacancyNotAvailableForEditing);
  
            if (Utility.VacancyHasCompletedPartOne(vacancy))
            {
                return RouteNames.Vacancy_Preview_Get;
            }

            var resumeRouteName = Utility.GetValidRoutesForVacancy(vacancy).Last();

            return resumeRouteName;
        }
    }
}