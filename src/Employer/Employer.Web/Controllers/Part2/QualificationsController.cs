using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class QualificationsController : Controller
    {
        private readonly QualificationsOrchestrator _orchestrator;

        public QualificationsController(QualificationsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("qualifications", Name = RouteNames.Qualifications_Get)]
        public async Task<IActionResult> Qualifications(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetQualificationsViewModelAsync(vrm);

            TryUpdateQualificationsFromTempData(vm);
            
            return View(vm);
        }

        [HttpPost("qualifications", Name = RouteNames.Qualifications_Post)]
        public async Task<IActionResult> Qualifications(QualificationsEditModel m)
        {
            var response = await _orchestrator.PostQualificationsEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetQualificationsViewModelAsync(m);
                
                return View(vm);
            }

            if (m.IsAddingQualification || m.IsRemovingQualification)
            {
                TempData.Put(TempDataKeys.Qualifications, m.Qualifications);
                return RedirectToRoute(RouteNames.Qualifications_Get);
            }
            
            return RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }

        private void TryUpdateQualificationsFromTempData(QualificationsViewModel vm)
        {
            if (TempData.ContainsKey(TempDataKeys.Qualifications))
            {
                var tempDataQualifications = TempData.Get<List<QualificationEditModel>>(TempDataKeys.Qualifications);
                vm.Qualifications = tempDataQualifications;
            }
        }
    }
}