using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.EditVacancyDates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class EditVacancyDatesController : Controller
    {
        private readonly EditVacancyDatesOrchestrator _orchestrator;
        private readonly IHostingEnvironment _hostingEnvironment;

        public EditVacancyDatesController(EditVacancyDatesOrchestrator orchestrator, IHostingEnvironment hostingEnvironment)
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

            return View(response.Data);
        }

        [HttpPost("edit-dates", Name = RouteNames.VacancyEditDates_Post)]
        public async Task<IActionResult> EditVacancyDates(EditVacancyDatesEditModel m)
        {
            var response = await _orchestrator.PostEditVacancyDatesEditModelAsync(m);

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetEditVacancyDatesViewModelAsync(m);
                return View(vm);
            }

            Response.Cookies.SetProposedClosingDate(_hostingEnvironment, m.VacancyId.GetValueOrDefault(), DateTime.Parse(m.ClosingDate));
            Response.Cookies.SetProposedStartDate(_hostingEnvironment, m.VacancyId.GetValueOrDefault(), DateTime.Parse(m.StartDate));

            return RedirectToRoute(RouteNames.VacancyEdit_Get);
        }
    }
}