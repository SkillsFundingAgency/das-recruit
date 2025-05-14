using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.NumberOfPositions;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Microsoft.AspNetCore.Authorization;
using FeatureNames = Esfa.Recruit.Provider.Web.Configuration.FeatureNames;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class NumberOfPositionsController(NumberOfPositionsOrchestrator orchestrator, IFeature feature) : Controller
    {
        [HttpGet("number-of-positions", Name = RouteNames.NumberOfPositions_Get)]
        public async Task<IActionResult> NumberOfPositions(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {            
            var vm = await orchestrator.GetNumberOfPositionsViewModelForExistingVacancyAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("number-of-positions", Name = RouteNames.NumberOfPositions_Post)]
        public async Task<IActionResult> NumberOfPositions(NumberOfPositionsEditModel model, [FromQuery] bool wizard)
        {
            var response = await orchestrator.PostNumberOfPositionsEditModelAsync(model, User.ToVacancyUser());
            
            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await orchestrator.GetNumberOfPositionsViewModelFromEditModelAsync(model);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }
            
            return wizard
                ? feature.IsFeatureEnabled(FeatureNames.MultipleLocations)
                    ? RedirectToRoute(RouteNames.MultipleLocations_Get, new { Wizard = wizard, model.Ukprn, model.VacancyId })
                    : RedirectToRoute(RouteNames.Location_Get, new { Wizard = wizard, model.Ukprn, model.VacancyId })
                : RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {model.Ukprn, model.VacancyId});
        }    
    }
}