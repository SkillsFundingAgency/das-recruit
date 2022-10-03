using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.EditVacancyDates;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class EditVacancyDatesController : Controller
    {
        private readonly EditVacancyDatesOrchestrator _orchestrator;

        public EditVacancyDatesController(EditVacancyDatesOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("edit-dates", Name = RouteNames.VacancyEditDates_Get)]
        public async Task<IActionResult> EditVacancyDates(VacancyRouteModel vrm)
        {
            var proposedClosingDate = Request.Cookies.GetProposedClosingDate(vrm.VacancyId);
            var proposedStartDate = Request.Cookies.GetProposedStartDate(vrm.VacancyId);

            var response = await _orchestrator.GetEditVacancyDatesViewModelAsync(vrm, proposedClosingDate, proposedStartDate);

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            return View(response.Data);
        }

        [HttpPost("edit-dates", Name = RouteNames.VacancyEditDates_Post)]
        public async Task<IActionResult> EditVacancyDates(EditVacancyDatesEditModel m)
        {
            var response = await _orchestrator.PostEditVacancyDatesEditModelAsync(m,User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetEditVacancyDatesViewModelAsync(m);
                return View(vm);
            }
            TempData.TryAdd(TempDataKeys.DashboardInfoMessage, string.Format(InfoMessages.AdvertUpdated, m.Title));
            
            return RedirectToRoute(RouteNames.Vacancies_Get);
        }
    }
}