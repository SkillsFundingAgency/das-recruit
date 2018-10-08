using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.CreateVacancy;
using Esfa.Recruit.Employer.Web.ViewModels.DeleteVacancy;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountRoutePath)]
    public class CreateVacancyController : Controller
    {
        private readonly CreateVacancyOrchestrator _orchestrator;

        public CreateVacancyController(CreateVacancyOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("create-options", Name = RouteNames.CreateVacancyOptions_Get)]
        public async Task<IActionResult> Options([FromRoute]string employerAccountId)
        {
            var vm = await _orchestrator.GetCreateOptionsViewModelAsync(employerAccountId);

            return View(vm);
        }

        [HttpPost("create-options", Name = RouteNames.CreateVacancyOptions_Post)]
        public IActionResult Options(CreateOptionsEditModel model)
        {
            return View();
        }
    }
}
