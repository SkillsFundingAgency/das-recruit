using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.VacanciesRoutePath)]
    public class VacancyTaskListController : Controller
    {
        private readonly VacancyTaskListOrchestrator _orchestrator;

        public VacancyTaskListController (VacancyTaskListOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("create/start", Name=RouteNames.CreateVacancyStart)]
        public IActionResult StartVacancyCreate(VacancyRouteModel vrm)
        {
            return View("StartCreateVacancy");
        }
        
        [HttpGet("create/task-list", Name = RouteNames.ProviderTaskListCreateGet)]
        public async Task<IActionResult> CreateProviderTaskList(VacancyRouteModel vrm)
        {
            var viewModel = await _orchestrator.GetCreateVacancyTaskListModel(vrm);
            viewModel.SetSectionStates(viewModel, ModelState);
            
            return View("ProviderTaskList", viewModel);
        }
        
        [HttpGet("{vacancyId:guid}/task-list", Name = RouteNames.ProviderTaskListGet)]
        
        public async Task<IActionResult> ProviderTaskList(VacancyRouteModel vrm)
        {
            var viewModel = await _orchestrator.GetVacancyTaskListModel(vrm); 
            viewModel.SetSectionStates(viewModel, ModelState);
            
            return View(viewModel);
        }
    }
}