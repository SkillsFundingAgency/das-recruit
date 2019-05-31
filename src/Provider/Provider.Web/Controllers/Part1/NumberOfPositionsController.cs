using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Title;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.NumberOfPositions;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountRoutePath)]
    public class NumberOfPositionsController : Controller
    {
        private const string ExistingVacancyNumberOfPositionsRoute = "vacancies/{vacancyId:guid}/noofpositions";
        private readonly NumberOfPositionsOrchestrator _orchestrator;
        public IProviderVacancyClient ProviderVacancyClient { get; }

        public NumberOfPositionsController(NumberOfPositionsOrchestrator orchestrator, IProviderVacancyClient providerVacancyClient)
        {
            this.ProviderVacancyClient = providerVacancyClient;
            _orchestrator = orchestrator;
        }

        [HttpGet(ExistingVacancyNumberOfPositionsRoute, Name = RouteNames.NumberOfPositions_Get)]
        public async Task<IActionResult> NumberOfPositions(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {            
            var vm = await _orchestrator.GetNumberOfPositionsViewModelForExistingVacancyAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost(ExistingVacancyNumberOfPositionsRoute, Name = RouteNames.NumberOfPositions_Post)]
        public async Task<IActionResult> NumberOfPositions(VacancyRouteModel vrm, NumberOfPositionsEditModel model, [FromQuery] bool wizard)
        {
            var ukprn = User.GetUkprn();
            var response = await _orchestrator.PostNumberOfPositionsEditModelAsync(vrm, model, User.ToVacancyUser(), ukprn);
            
            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if(!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetNumberOfPositionsViewModelFromEditModelAsync(vrm, model, ukprn);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            return wizard
                ? RedirectToRoute(RouteNames.ShortDescription_Get, new {vacancyId = response.Data})
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }    
    }
}