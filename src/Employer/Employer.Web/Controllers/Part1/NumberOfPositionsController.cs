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

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class NumberOfPositionsController : Controller
    {
       private readonly NumberOfPositionsOrchestrator _orchestrator;

        public NumberOfPositionsController(NumberOfPositionsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("number-of-positions", Name = RouteNames.NumberOfPositions_Get)]
        public async Task<IActionResult> NumberOfPositions(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetNumberOfPositionsViewModelAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            vm.ReferredFromMAHome_FromSavedFavourites = Convert.ToBoolean(TempData.Peek(TempDataKeys.ReferredFromMAHome_FromSavedFavourites));
            var referredFromMAHome_UKPRN = Convert.ToString(TempData.Peek(TempDataKeys.ReferredFromMAHome_UKPRN));
            var referredFromMAHome_ProgrammeId = Convert.ToString(TempData.Peek(TempDataKeys.ReferredFromMAHome_ProgrammeId));
            if (vm.ReferredFromMAHome_FromSavedFavourites)
            {
                if (!string.IsNullOrWhiteSpace(referredFromMAHome_UKPRN) && !string.IsNullOrWhiteSpace(referredFromMAHome_ProgrammeId))
                    vm.BackLink = RouteNames.Title_Get;
                if (string.IsNullOrWhiteSpace(referredFromMAHome_UKPRN) && !string.IsNullOrWhiteSpace(referredFromMAHome_ProgrammeId))
                    vm.BackLink = RouteNames.TrainingProvider_Select_Get;
            }
            return View(vm);
        }

        [HttpPost("number-of-positions", Name = RouteNames.NumberOfPositions_Post)]
        public async Task<IActionResult> NumberOfPositions(NumberOfPositionsEditModel m, [FromQuery] bool wizard)
        {
            var response = await _orchestrator.PostNumberOfPositionsEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if(!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetNumberOfPositionsViewModelAsync(m);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            return wizard
                ? RedirectToRoute(RouteNames.Employer_Get, new {vacancyId = response.Data})
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }
    }
}