using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
    public class VacancyDescriptionController : Controller
    {
        private readonly VacancyDescriptionOrchestrator _orchestrator;

        public VacancyDescriptionController(VacancyDescriptionOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("vacancy-description", Name = RouteNames.VacancyDescription_Index_Get)]
        public async Task<IActionResult> VacancyDescription(Guid vacancyId)
        {
            var vm = await _orchestrator.GetVacancyDescriptionViewModelAsync(vacancyId);
            return View(vm);
        }

        [HttpPost("vacancy-description", Name =  RouteNames.VacancyDescription_Index_Post)]
        public async Task<IActionResult> VacancyDescription(VacancyDescriptionEditModel m)
        {
            var response = await _orchestrator.PostVacancyDescriptionEditModelAsync(m);

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetVacancyDescriptionViewModelAsync(m);
                return View(vm);
            }
            
            return RedirectToRoute(RouteNames.Preview_Index_Get);
        }
    }
}