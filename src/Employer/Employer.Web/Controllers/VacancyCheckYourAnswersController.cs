using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyCheckYourAnswersController : Controller
    {
        private readonly VacancyTaskListOrchestrator _orchestrator;

        public VacancyCheckYourAnswersController (VacancyTaskListOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("check-answers", Name = RouteNames.EmployerCheckYourAnswersGet)]
        
        public async Task<IActionResult> CheckYourAnswers(VacancyRouteModel vrm)
        {
            var viewModel = await _orchestrator.GetVacancyTaskListModel(vrm); 
            
            viewModel.SetSectionStates(viewModel, ModelState);
            
            return View(viewModel);
        }
    }
}