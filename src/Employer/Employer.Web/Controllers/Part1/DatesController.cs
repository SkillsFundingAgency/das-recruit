using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Dates;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
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
            vm.PageInfo.SetWizard(wizard);
            AddSoftValidationErrorsToModelState(vm);
            return View(vm);
        }

        private void AddSoftValidationErrorsToModelState(DatesViewModel viewModel)
        {
            if (!viewModel.SoftValidationErrors.HasErrors)
                return;

            foreach (var error in viewModel.SoftValidationErrors.Errors)
            {
                viewModel.CanShowTrainingErrorHint = viewModel.SoftValidationErrors.Errors.Any(e => e.ErrorCode == ErrorCodes.TrainingExpiryDate); ;
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
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
    }
}