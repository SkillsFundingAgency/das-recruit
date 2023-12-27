using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using InfoMsg = Esfa.Recruit.Shared.Web.ViewModels.InfoMessages;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyManageController : Controller
    {
        private readonly VacancyManageOrchestrator _orchestrator;
        private readonly IUtility _utility;
        private readonly ServiceParameters _serviceParameters;
        private readonly IConfiguration _configuration;

        public VacancyManageController(VacancyManageOrchestrator orchestrator, IUtility utility, ServiceParameters serviceParameters, IConfiguration configuration)
        {
            _orchestrator = orchestrator;
            _utility = utility;
            _serviceParameters = serviceParameters;
            _configuration = configuration;
        }

        [HttpGet("manage", Name = RouteNames.VacancyManage_Get)]
        public async Task<IActionResult> ManageVacancy(VacancyRouteModel vrm, [FromQuery] string sortColumn, [FromQuery] string sortOrder)
        {
            if (_serviceParameters.VacancyType == VacancyType.Traineeship 
                && DateTime.TryParse(_configuration["TraineeshipCutOffDate"], out var traineeshipCutOffDate))
            {
                if (traineeshipCutOffDate != DateTime.MinValue && traineeshipCutOffDate < DateTime.UtcNow)
                {
                    return RedirectPermanent($"{_configuration["ProviderSharedUIConfiguration:DashboardUrl"]}account");
                }
            }
            
            Enum.TryParse<SortOrder>(sortOrder, out var outputSortOrder);
            Enum.TryParse<SortColumn>(sortColumn, out var outputSortColumn);

            var vacancy = await _orchestrator.GetVacancy(vrm);

            if (vacancy.CanEdit)
            {
                return HandleRedirectOfEditableVacancy(vacancy);
            }

            var viewModel = await _orchestrator.GetManageVacancyViewModel(vacancy, vrm, outputSortColumn, outputSortOrder);

            if (TempData.ContainsKey(TempDataKeys.VacancyClosedMessage))
                viewModel.VacancyClosedInfoMessage = TempData[TempDataKeys.VacancyClosedMessage].ToString();

            if (TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusInfoMessage))
                viewModel.ApplicationReviewStatusHeaderInfoMessage = TempData[TempDataKeys.ApplicationReviewStatusInfoMessage].ToString();

            if (TempData.ContainsKey(TempDataKeys.ApplicationReviewSuccessStatusInfoMessage))
            {
                viewModel.ApplicationReviewStatusChangeBannerHeader = TempData[TempDataKeys.ApplicationReviewSuccessStatusInfoMessage].ToString();
            }

            if (TempData.ContainsKey(TempDataKeys.ApplicationReviewUnsuccessStatusInfoMessage))
                viewModel.ApplicationReviewStatusChangeBannerHeader = TempData[TempDataKeys.ApplicationReviewUnsuccessStatusInfoMessage].ToString();

            if (TempData.ContainsKey(TempDataKeys.SharedMultipleApplicationsHeader))
            {
                viewModel.ApplicationReviewStatusChangeBannerHeader = TempData[TempDataKeys.SharedMultipleApplicationsHeader].ToString();
                viewModel.ApplicationReviewStatusChangeBannerMessage = InfoMsg.SharedMultipleApplicationsBannerMessage;
            }

            if (TempData.ContainsKey(TempDataKeys.ApplicationsToUnsuccessfulHeader))
            {
                viewModel.ApplicationReviewStatusChangeBannerHeader = TempData[TempDataKeys.ApplicationsToUnsuccessfulHeader].ToString();
            }

            if (TempData.ContainsKey(TempDataKeys.SharedSingleApplicationsHeader))
            {
                viewModel.ApplicationReviewStatusChangeBannerHeader = TempData[TempDataKeys.SharedSingleApplicationsHeader].ToString();
                viewModel.ApplicationReviewStatusChangeBannerMessage = InfoMsg.SharedSingleApplicationsBannerMessage;
            }

            if (TempData.ContainsKey(TempDataKeys.ApplicationStatusChangedHeader))
            {
                viewModel.ApplicationReviewStatusChangeBannerHeader = TempData[TempDataKeys.ApplicationStatusChangedHeader].ToString();
            }

            return View(viewModel);
        }


        private IActionResult HandleRedirectOfEditableVacancy(Vacancy vacancy)
        {
            if (_utility.IsTaskListCompleted(vacancy))
            {
                return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new { vacancy.TrainingProvider.Ukprn, vacancyId = vacancy.Id });
            }
            return RedirectToRoute(RouteNames.ProviderTaskListGet, new { vacancy.TrainingProvider.Ukprn, vacancyId = vacancy.Id });
        }
    }
}