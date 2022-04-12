using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class CloseVacancyController : Controller
    {
        private readonly CloseVacancyOrchestrator _orchestrator;

        public CloseVacancyController(CloseVacancyOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("close", Name = RouteNames.CloseVacancy_Get)]
        public Task<IActionResult> Close(VacancyRouteModel vrm)
        {
            return GetCloseVacancyConfirmationView(vrm);
        }

        [HttpPost("close", Name = RouteNames.CloseVacancy_Post)]
        public async Task<IActionResult> Close(CloseEditModel m)
        {
            if (!ModelState.IsValid)
            {
                var vrm = new VacancyRouteModel { VacancyId = m.VacancyId, Ukprn = m.Ukprn };
                return await GetCloseVacancyConfirmationView(vrm);
            }

            if (m.ConfirmClose != null && !m.ConfirmClose.Value)
            {
                return RedirectToRoute(RouteNames.VacancyManage_Get);
            }

            var response = await _orchestrator.CloseVacancyAsync(m, User.ToVacancyUser());

            TempData.Add(TempDataKeys.VacancyClosedMessage, string.Format(InfoMessages.VacancyClosed, response.Data.VacancyReference, response.Data.Title));

            return RedirectToRoute(RouteNames.VacancyManage_Get);
        }

        private async Task<IActionResult> GetCloseVacancyConfirmationView(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetCloseViewModelAsync(vrm);
            return View(ViewNames.CloseVacancyView, vm);
        }
    }
}
