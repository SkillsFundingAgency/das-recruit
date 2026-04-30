using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ArchiveVacancy;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class ArchiveVacancyController(ArchiveVacancyOrchestrator orchestrator) : Controller
    {
        [HttpGet("archive", Name = RouteNames.ArchiveVacancy_Get)]
        public Task<IActionResult> Archive(VacancyRouteModel vrm) 
            => GetArchiveVacancyConfirmationView(vrm);

        [HttpPost("archive", Name = RouteNames.ArchiveVacancy_Post)]
        public async Task<IActionResult> Archive(ArchiveEditModel m)
        {
            if (!ModelState.IsValid)
            {
                return await GetArchiveVacancyConfirmationView(m);
            }

            if (m.ConfirmArchive != null && !m.ConfirmArchive.Value)
            {
                return RedirectToRoute(RouteNames.VacanciesGetAll, new
                {
                    ukprn = m.Ukprn,
                    filter = FilteringOptions.Closed
                });
            }

            var vm = await orchestrator.ArchiveVacancyAsync(m, User.ToVacancyUser());

            var archivePageUrl = Url.RouteUrl(RouteNames.VacanciesGetAll, new
            {
                ukprn = m.Ukprn,
                filter = FilteringOptions.Archived
            });
            TempData.Add(TempDataKeys.VacanciesInfoMessage, string.Format(InfoMessages.VacancyArchived, vm.Title, vm.VacancyReference, archivePageUrl));

            return RedirectToRoute(RouteNames.VacanciesGetAll, new {
                ukprn = m.Ukprn,
                filter = FilteringOptions.Closed
            });
        }

        private async Task<IActionResult> GetArchiveVacancyConfirmationView(VacancyRouteModel vrm)
        {
            var vm = await orchestrator.GetArchiveViewModelAsync(vrm);
            return View(vm);
        }
    }
}