using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Dates;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{    
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class DatesController : Controller
    {
        private readonly DatesOrchestrator _orchestrator;
        public DatesController(DatesOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("dates", Name = RouteNames.Dates_Get)]
        public async Task<IActionResult> Dates(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetDatesViewModelAsync(vrm);
            AddSoftValidationErrorsToModelState(vm);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("dates", Name = RouteNames.Dates_Post)]
        public async Task<IActionResult> Dates(DatesEditModel m, [FromQuery] bool wizard)
        {
            var response = await _orchestrator.PostDatesEditModelAsync(m, User.ToVacancyUser());
            
            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetDatesViewModelAsync(m);
                vm.CanShowTrainingErrorHint = response.Errors.Errors.Any(e => e.ErrorCode == ErrorCodes.TrainingExpiryDate);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            return wizard
                ? RedirectToRoute(RouteNames.Duration_Get)
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }
        private void AddSoftValidationErrorsToModelState(DatesViewModel viewModel)
        {
            if (viewModel.SoftValidationErrors == null)
                return;

            foreach (var error in viewModel.SoftValidationErrors.Errors)
            {
                viewModel.CanShowTrainingErrorHint = viewModel.SoftValidationErrors.Errors.Any(e => e.ErrorCode == ErrorCodes.TrainingExpiryDate); ;
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }

    }
}