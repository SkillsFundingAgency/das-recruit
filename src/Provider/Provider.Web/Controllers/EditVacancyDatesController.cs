using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.EditVacancyDates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class EditVacancyDatesController : Controller
    {
        private readonly EditVacancyDatesOrchestrator _orchestrator;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ServiceParameters _serviceParameters;
        private readonly IConfiguration _configuration;

        public EditVacancyDatesController(EditVacancyDatesOrchestrator orchestrator, IWebHostEnvironment hostingEnvironment, ServiceParameters serviceParameters, IConfiguration configuration)
        {
            _orchestrator = orchestrator;
            _hostingEnvironment = hostingEnvironment;
            _serviceParameters = serviceParameters;
            _configuration = configuration;
        }

        [HttpGet("edit-dates", Name = RouteNames.VacancyEditDates_Get)]
        public async Task<IActionResult> EditVacancyDates(VacancyRouteModel vrm)
        {
            if (_serviceParameters.VacancyType == VacancyType.Traineeship 
                && DateTime.TryParse(_configuration["TraineeshipCutOffDate"], out var traineeshipCutOffDate))
            {
                if (traineeshipCutOffDate != DateTime.MinValue && traineeshipCutOffDate < DateTime.UtcNow)
                {
                    return RedirectPermanent($"{_configuration["ProviderSharedUIConfiguration:DashboardUrl"]}account");
                }
            }
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