using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Orchestrators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DashboardOrchestrator _orchestrator;

        public DashboardController(DashboardOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var vm = await _orchestrator.GetDashboardViewModelAsync();
            
            return View(vm);
        }
    }
}
