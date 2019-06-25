using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications;
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

        [HttpGet("qualification-add", Name = RouteNames.Qualification_Add_Get)]
        public async Task<IActionResult> AddQualification(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetAddQualificationViewModelAsync(vrm);

            return View(vm);
        }

        [HttpPost("qualification-add", Name = RouteNames.Qualification_Add_Post)]
        public async Task<IActionResult> AddQualification(VacancyRouteModel vrm, QualificationEditModel m)
        {
            var response = await _orchestrator.PostAddQualificationEditModelAsync(vrm, m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetAddQualificationViewModelAsync(vrm,m);
                
                return View(vm);
            }
            
            return RedirectToRoute(RouteNames.Qualifications_Get);
        }

        [HttpPost("qualification-delete", Name = RouteNames.Qualification_Delete_Post)]
        public async Task<IActionResult> DeleteQualification(VacancyRouteModel vrm, [FromForm] int index)
        {
            await _orchestrator.DeleteQualificationAsync(vrm, index, User.ToVacancyUser());

            TempData[QualificationDeletedTempDataKey] = 1;
            return RedirectToRoute(RouteNames.Qualifications_Get);
        }
    }
}