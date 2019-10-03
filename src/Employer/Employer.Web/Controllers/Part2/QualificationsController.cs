using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.ViewModels.Qualifications;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class QualificationsController : Controller
    {
        private const string QualificationDeletedTempDataKey = "QualificationDeletedTempDataKey";
        private readonly QualificationsOrchestrator _orchestrator;

        public QualificationsController(QualificationsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("qualifications", Name = RouteNames.Qualifications_Get)]
        public async Task<IActionResult> Qualifications(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetQualificationsViewModelAsync(vrm);

            if (vm.Qualifications.Any() == false)
            {
                TempData.Remove(QualificationDeletedTempDataKey);
                return RedirectToRoute(RouteNames.Qualification_Add_Get);
            }
            
            if (TempData[QualificationDeletedTempDataKey] != null)
                vm.InfoMessage = "Successfully removed qualification";

            return View(vm);
        }

        [HttpGet("qualifications/add", Name = RouteNames.Qualification_Add_Get)]
        public async Task<IActionResult> Qualification(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetQualificationViewModelForAddAsync(vrm);

            return View(vm);
        }

        [HttpGet("qualifications/{index:int}", Name = RouteNames.Qualification_Edit_Get)]
        public async Task<IActionResult> Qualification(VacancyRouteModel vrm, int index)
        {
            var vm = await _orchestrator.GetQualificationViewModelForEditAsync(vrm, index);

            return View(vm);
        }

        [HttpPost("qualifications/add", Name = RouteNames.Qualification_Add_Post)]
        public async Task<IActionResult> Qualification(VacancyRouteModel vrm, QualificationEditModel m)
        {
            var response = await _orchestrator.PostQualificationEditModelForAddAsync(vrm, m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetQualificationViewModelForAddAsync(vrm,m);
                
                return View(vm);
            }
            
            return RedirectToRoute(RouteNames.Qualifications_Get);
        }

        [HttpPost("qualifications/{index:int}", Name = RouteNames.Qualification_Edit_Post)]
        public async Task<IActionResult> Qualification(VacancyRouteModel vrm, QualificationEditModel m, [FromRoute] int index)
        {
            var response = await _orchestrator.PostQualificationEditModelForEditAsync(vrm, m, User.ToVacancyUser(), index);

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetQualificationViewModelForEditAsync(vrm, m, index);

                return View(vm);
            }

            return RedirectToRoute(RouteNames.Qualifications_Get);
        }

        [HttpPost("qualifications/delete", Name = RouteNames.Qualification_Delete_Post)]
        public async Task<IActionResult> DeleteQualification(VacancyRouteModel vrm, [FromForm] int index)
        {
            await _orchestrator.DeleteQualificationAsync(vrm, index, User.ToVacancyUser());

            TempData[QualificationDeletedTempDataKey] = 1;
            return RedirectToRoute(RouteNames.Qualifications_Get);
        }
    }
}