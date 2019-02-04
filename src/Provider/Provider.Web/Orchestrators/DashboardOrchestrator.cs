using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;

        public DashboardOrchestrator(IProviderVacancyClient client, IRecruitVacancyClient vacancyClient)
        {
            _client = client;
            _vacancyClient = vacancyClient;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(long ukprn)
        {
            var dashboard = await _client.GetDashboardAsync(ukprn);

            if (dashboard == null)
            {
                await _client.GenerateDashboard(ukprn);
                dashboard = await _client.GetDashboardAsync(ukprn);
            }

            var vm = DashboardMapper.MapFromProviderDashboard(dashboard);
            return vm;
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