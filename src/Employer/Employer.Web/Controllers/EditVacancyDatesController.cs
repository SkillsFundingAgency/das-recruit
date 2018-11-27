using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.EditVacancyDates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
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

            Response.Cookies.SetProposedClosingDate(_hostingEnvironment, m.VacancyId, DateTime.Parse(m.ClosingDate));
            Response.Cookies.SetProposedStartDate(_hostingEnvironment, m.VacancyId, DateTime.Parse(m.StartDate));

            return RedirectToRoute(RouteNames.VacancyEdit_Get);
        }
    }
}