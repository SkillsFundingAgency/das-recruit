using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class ManageNotificationsController : Controller
    {
        private readonly ManageNotificationsOrchestrator _orchestrator;
        public ManageNotificationsController(ManageNotificationsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("manage-notifications", Name = RouteNames.ManageNotifications_Get)]
        public async Task<IActionResult> ManageNotifications()
        {
            var vm = await _orchestrator.GetManageNotificationsViewModelAsync(User.ToVacancyUser());
            return View(vm);
        }

        [HttpPost("manage-notifications", Name = RouteNames.ManageNotifications_Post)]
        public async Task<IActionResult> ManageNotifications(ManageNotificationsEditModel model)
        {
            await _orchestrator.UpdateUserNotificationPreferencesAsync(model, User.ToVacancyUser());
            var vm = await _orchestrator.GetManageNotificationsViewModelAsync(User.ToVacancyUser());
            return View(vm);
        }
    }
}