using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.EditVacancyDates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class EditVacancyDatesController : Controller
    {
        private readonly EditVacancyDatesOrchestrator _orchestrator;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public EditVacancyDatesController(EditVacancyDatesOrchestrator orchestrator, IWebHostEnvironment hostingEnvironment)
        {
            _orchestrator = orchestrator;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("edit-dates", Name = RouteNames.VacancyEditDates_Get)]
        public async Task<IActionResult> EditVacancyDates(VacancyRouteModel vrm)
        {
            var proposedClosingDate = Request.Cookies.GetProposedClosingDate(vrm.VacancyId.GetValueOrDefault());
            var proposedStartDate = Request.Cookies.GetProposedStartDate(vrm.VacancyId.GetValueOrDefault());

            var response = await _orchestrator.GetEditVacancyDatesViewModelAsync(vrm, proposedClosingDate, proposedStartDate);

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            response.Data.Ukprn = vrm.Ukprn;
            response.Data.VacancyId = vrm.VacancyId;

            return View(response.Data);
        }

        [HttpPost("edit-dates", Name = RouteNames.VacancyEditDates_Post)]
        public async Task<IActionResult> EditVacancyDates(EditVacancyDatesEditModel m)
        {
            var response = await _orchestrator.PostEditVacancyDatesEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetEditVacancyDatesViewModelAsync(m);
                return View(vm);
            }
            
            TempData.Add(TempDataKeys.VacanciesInfoMessage, string.Format(InfoMessages.VacancyUpdated, m.Title));


            return RedirectToRoute(RouteNames.Vacancies_Get, new {m.Ukprn});
        }
    }
}