using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class DashboardController : Controller
    {
        private readonly DashboardOrchestrator _orchestrator;

        public DashboardController(DashboardOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("", Name = RouteNames.Dashboard_Get)]
        public async Task<IActionResult> Dashboard([FromRoute] string employerAccountId)
        {
            var vm = await _orchestrator.GetDashboardViewModelAsync(employerAccountId, User.ToVacancyUser());
            vm.FromMaHome = ShowReferredFromMaBackLink();
            return View(vm.HasEmployerReviewPermission ? ViewNames.DashboardWithReview : ViewNames.DashboardNoReview, vm);
            //return View(vm);
        }
        private bool ShowReferredFromMaBackLink()
        {
            return Convert.ToBoolean(TempData.Peek(TempDataKeys.ReferredFromMa));
        }
    }
}