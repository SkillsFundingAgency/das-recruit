using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Training;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    [Authorize(Policy = nameof(PolicyNames.IsTraineeshipWeb))]
    public class TraineeshipSectorController : Controller
    {
        private readonly TraineeSectorOrchestrator _orchestrator;

        public TraineeshipSectorController(TraineeSectorOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        [HttpGet("traineeship-sector", Name = RouteNames.TraineeSector_Get)]
        public async Task<IActionResult> TraineeshipSector(VacancyRouteModel vrm, [FromQuery] string wizard = "true", [FromQuery] string clear = "", [FromQuery] int? routeId = null)
        {
            var clearTraining = string.IsNullOrWhiteSpace(clear) == false;

            var vm = await _orchestrator.GetTraineeSectorViewModelAsync(vrm);

            if (routeId != null &&
                vm.Routes.Any(p => p.Id == routeId))
            {
                vm.SelectedRouteId = routeId.Value;
            }

            if (clearTraining)
                vm.SelectedRouteId = null;

            vm.PageInfo.SetWizard(wizard);

            return View(vm);
        }
        
        [HttpPost("traineeship-sector", Name = RouteNames.TraineeSector_Post)]
        public async Task<IActionResult> TraineeshipSector(TraineeSectorEditModel editModel)
        {
            var vm = await _orchestrator.GetTraineeSectorViewModelAsync(editModel);
            
            if (editModel.SelectedRouteId == -1)
            {
                ModelState.AddModelError(nameof(editModel.SelectedRouteId), ValidationMessages.TraineeshipSectorValidationMessages.SelectionRequired);
            }
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            
            var response = await _orchestrator.PostTraineeSectorEditModelAsync(editModel, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }
            
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            
            if (!vm.IsTaskListCompleted)
            {
                return RedirectToRoute(RouteNames.ShortDescription_Get, new {editModel.VacancyId, editModel.Ukprn});
            }
            return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {editModel.VacancyId, editModel.Ukprn});
        }
    }
}