using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
	public class EmployerOrchestrator 
    {
        private readonly IProviderVacancyClient _providerVacancyClient;



        public EmployerOrchestrator(IProviderVacancyClient providerVacancyClient, IRecruitVacancyClient vacancyClient, ILogger<EmployerOrchestrator> logger, IReviewSummaryService reviewSummaryService)
        {
            _providerVacancyClient = providerVacancyClient;
        }

        public async Task<EmployersViewModel> GetEmployersViewModelAsync(VacancyRouteModel vrm)
        {
            var editVacancyInfo = await _providerVacancyClient.GetProviderEditVacancyInfoAsync(vrm.Ukprn);

            if (editVacancyInfo.Employers.Any() == false)
            {
                throw new MissingPermissionsException(string.Format(RecruitWebExceptionMessages.ProviderMissingPermission, vrm.Ukprn));
            }

            var vm = new EmployersViewModel
            {
                Employers = editVacancyInfo.Employers.Select(e => new EmployerViewModel {Id = e.EmployerAccountId, Name = e.Name}),
            };

            return vm;
        }
    }
}