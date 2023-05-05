using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Configuration;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Extensions;
using Esfa.Recruit.Qa.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.Qa.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DashboardOrchestrator _orchestrator;
        private readonly IConfiguration _configuration;

        public DashboardController(DashboardOrchestrator orchestrator, IConfiguration configuration)
        {
            _orchestrator = orchestrator;
            _configuration = configuration;
        }

        [HttpGet(Name = RouteNames.Dashboard_Index_Get)]
        public async Task<IActionResult> Index([FromQuery]string searchTerm)
        {
            bool isDfESignInAllowed = _configuration.GetValue<bool>("UseDfeSignIn");
            if (isDfESignInAllowed && User.Identity is { IsAuthenticated: false })
            {
                // if the user is not authenticated, redirect them back to start now page.
                return RedirectToAction("Index", "Home");
            }

            var vm = await _orchestrator.GetDashboardViewModelAsync(searchTerm, User);

            vm.DashboardMessage = TempData[TempDataKeys.DashboardMessage]?.ToString();
            
            return View(vm);
        }

        [HttpPost("next-vacancy", Name = RouteNames.Dashboard_Next_Vacancy_Post)]
        public async Task<IActionResult> NextVacancy()
        {
            var vacancyReviewId = await _orchestrator.AssignNextVacancyReviewAsync(User.GetVacancyUser());

            if (vacancyReviewId == null)
                return RedirectToRoute(RouteNames.Dashboard_Index_Get);

            return RedirectToRoute(RouteNames.Vacancy_Review_Get, new {reviewId = vacancyReviewId});
        }

        [HttpGet("next-vacancy", Name = RouteNames.Dashboard_Next_Vacancy_Post)]
        public IActionResult NextVacancyCallback()
        {
            //This GET handles the authentication callback when NextVacancy is POSTed after a session timeout
            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }
    }
}
