using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Attributes;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountRoutePath)]
    public class DashboardController : Controller
    {
        private readonly DashboardOrchestrator _orchestrator;

        public DashboardController(DashboardOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [CheckEmployerBlocked]
        [HttpGet("", Name = RouteNames.Dashboard_Index_Get)]
        public async Task<IActionResult> Dashboard([FromRoute]string employerAccountId)
        {
            var vm = await _orchestrator.GetDashboardViewModelAsync(employerAccountId);

            if (TempData.ContainsKey(TempDataKeys.DashboardErrorMessage))
                vm.WarningMessage = TempData[TempDataKeys.DashboardErrorMessage].ToString();

            if (TempData.ContainsKey(TempDataKeys.DashboardInfoMessage))
                vm.InfoMessage = TempData[TempDataKeys.DashboardInfoMessage].ToString();

            return View(vm);
        }
    }
}