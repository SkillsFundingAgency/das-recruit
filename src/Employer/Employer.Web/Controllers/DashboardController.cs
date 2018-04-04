using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountRoutePath)]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly DashboardOrchestrator _orchestrator;

        public DashboardController(ILogger<DashboardController> logger, DashboardOrchestrator orchestrator)
        {
            _logger = logger;
            _orchestrator = orchestrator;
        }

        [HttpGet("dashboard", Name = RouteNames.Dashboard_Index_Get)]
        public async Task<IActionResult> Dashboard([FromRoute]string employerAccountId)
        {
            var vm = await _orchestrator.GetDashboardViewModelAsync(employerAccountId);
            
            if (TempData.ContainsKey(TempDataKeys.DashboardErrorMessage))
            {
                ModelState.AddModelError(string.Empty, TempData[TempDataKeys.DashboardErrorMessage].ToString());
            }

            return View(vm);
        }
    }
}