using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
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
        public async Task<IActionResult> Qualifications(Guid vacancyId)
        {
            var vm = await _orchestrator.GetQualificationsViewModelAsync(vacancyId);

            TryUpdateQualificationsFromTempData(vm);
            
            return View(vm);
        }

        [HttpPost("qualifications", Name = RouteNames.Qualifications_Post)]
        public async Task<IActionResult> Qualifications(QualificationsEditModel m)
        {
            
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetQualificationsViewModelAsync(m);
                
                return View(vm);
            }

            if (m.IsAddingQualification || m.IsRemovingQualification)
            {
                HandleQualificationChange(m);
                return RedirectToRoute(RouteNames.Qualifications_Get);
            }

            await _orchestrator.PostQualificationsEditModelAsync(m);
            
            return RedirectToRoute(RouteNames.Preview_Index_Get);
        }

        private void HandleQualificationChange(QualificationsEditModel m)
        {
            var qualifications = m.Qualifications?.ToList() ?? new List<QualificationEditModel>();

            if (m.IsAddingQualification)
            {
                qualifications.Add(m);
            }

            if (m.IsRemovingQualification)
            {
                qualifications.RemoveAt(int.Parse(m.RemoveQualification));
            }

            TempData.Add(TempDataKeys.Qualifications, qualifications);
        }

        private void TryUpdateQualificationsFromTempData(QualificationsViewModel vm)
        {
            if (TempData.ContainsKey(TempDataKeys.Qualifications))
            {
                var tempDataQualifications = TempData[TempDataKeys.Qualifications] as QualificationEditModel[] ?? new QualificationEditModel[0];
                vm.Qualifications = tempDataQualifications.ToList();
            }
        }
    }
}