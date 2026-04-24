using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ArchiveVacancy;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
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
                    EmployerAccountId = m.EmployerAccountId,
                    filter = FilteringOptions.Closed
                });
            }

            var vm = await orchestrator.ArchiveVacancyAsync(m, User.ToVacancyUser());

            var archivePageUrl = Url.RouteUrl(RouteNames.VacanciesGetAll, new
            {
                EmployerAccountId = m.EmployerAccountId,
                filter = FilteringOptions.Archived
            });
            TempData.Add(TempDataKeys.DashboardInfoMessage, string.Format(InfoMessages.AdvertArchived, vm.Title, vm.VacancyReference, archivePageUrl));

            return RedirectToRoute(RouteNames.VacanciesGetAll, new {
                EmployerAccountId = m.EmployerAccountId,
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