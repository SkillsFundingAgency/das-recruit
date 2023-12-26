using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyViewController : Controller
    {
        private readonly VacancyViewOrchestrator _orchestrator;
        private readonly IUtility _utility;
        private readonly ServiceParameters _serviceParameters;
        private readonly IConfiguration _configuration;

        public VacancyViewController(VacancyViewOrchestrator orchestrator, IUtility utility, ServiceParameters serviceParameters, IConfiguration configuration)
        {
            _orchestrator = orchestrator;
            _utility = utility;
            _serviceParameters = serviceParameters;
            _configuration = configuration;
        }

        [HttpGet("view", Name = RouteNames.DisplayFullVacancy_Get)]
        public async Task<IActionResult> DisplayFullVacancy(VacancyRouteModel vrm)
        {
            if (_serviceParameters.VacancyType == VacancyType.Traineeship 
                && DateTime.TryParse(_configuration["TraineeshipCutOffDate"], out var traineeshipCutOffDate))
            {
                if (traineeshipCutOffDate != DateTime.MinValue && traineeshipCutOffDate < DateTime.UtcNow)
                {
                    return RedirectPermanent(_configuration["ProviderSharedUIConfiguration:DashboardUrl"]);
                }
            }
            
            var vacancy = await _orchestrator.GetVacancy(vrm);

            if (vacancy.CanEdit)
            {
                return HandleRedirectOfEditableVacancy(vacancy);
            }

            var vm = await _orchestrator.GetFullVacancyDisplayViewModelAsync(vacancy);
            vm.Ukprn = vrm.Ukprn;
            vm.VacancyId = vrm.VacancyId;
            return View(ViewNames.FullVacancyView, vm);
        }

        private IActionResult HandleRedirectOfEditableVacancy(Vacancy vacancy)
        {
            if (_utility.IsTaskListCompleted(vacancy))
            {
                return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {vacancy.TrainingProvider.Ukprn, vacancyId = vacancy.Id});
            }
            return RedirectToRoute(RouteNames.ProviderTaskListGet, new {vacancy.TrainingProvider.Ukprn, vacancyId = vacancy.Id});
            
        }
    }
}
