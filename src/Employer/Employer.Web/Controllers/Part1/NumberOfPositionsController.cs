using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.NumberOfPositions;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using FeatureNames = Esfa.Recruit.Employer.Web.Configuration.FeatureNames;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class NumberOfPositionsController(NumberOfPositionsOrchestrator orchestrator, IFeature feature) : Controller
    {
        [HttpGet("number-of-positions", Name = RouteNames.NumberOfPositions_Get)]
        public async Task<IActionResult> NumberOfPositions(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await orchestrator.GetNumberOfPositionsViewModelAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            vm.BackLinkRoute = GetBackLink(vrm.VacancyId);
            return View(vm);
        }

        private string GetBackLink(Guid vacancyId)
        {
            var referredUkprn = Convert.ToString(TempData.Peek(TempDataKeys.ReferredUkprn + vacancyId));
            var referredProgrammeId = Convert.ToString(TempData.Peek(TempDataKeys.ReferredProgrammeId + vacancyId));
            if (!string.IsNullOrWhiteSpace(referredUkprn) && !string.IsNullOrWhiteSpace(referredProgrammeId))
                   return RouteNames.Title_Get;
            return RouteNames.TrainingProvider_Select_Get;
        }

        [HttpPost("number-of-positions", Name = RouteNames.NumberOfPositions_Post)]
        public async Task<IActionResult> NumberOfPositions(NumberOfPositionsEditModel m, [FromQuery] bool wizard)
        {
            var response = await orchestrator.PostNumberOfPositionsEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await orchestrator.GetNumberOfPositionsViewModelAsync(m);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            return wizard 
                ? feature.IsFeatureEnabled(FeatureNames.MultipleLocations)
                    ? RedirectToRoute(RouteNames.MultipleLocations_Get, new {m.VacancyId, m.EmployerAccountId, wizard})
                    : RedirectToRoute(RouteNames.Location_Get, new {m.VacancyId, m.EmployerAccountId, wizard}) 
                : RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {m.VacancyId, m.EmployerAccountId});
        }
    }
}