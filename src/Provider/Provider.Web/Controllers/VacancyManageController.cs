using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using InfoMsg = Esfa.Recruit.Shared.Web.ViewModels.InfoMessages;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyManageController : Controller
    {
        private readonly VacancyManageOrchestrator _orchestrator;
        private readonly IUtility _utility;

        public VacancyManageController(VacancyManageOrchestrator orchestrator, IUtility utility)
        {
            _orchestrator = orchestrator;
            _utility = utility;
        }

        [HttpGet("manage", Name = RouteNames.VacancyManage_Get)]
        public async Task<IActionResult> ManageVacancy(VacancyRouteModel vrm)
        {
            var vacancy = await _orchestrator.GetVacancy(vrm);

            if (vacancy.CanEdit)
            {
                return HandleRedirectOfEditableVacancy(vacancy);
            }

            var viewModel = await _orchestrator.GetManageVacancyViewModel(vacancy, vrm);

            if (TempData.ContainsKey(TempDataKeys.VacancyClosedMessage))
                viewModel.VacancyClosedInfoMessage = TempData[TempDataKeys.VacancyClosedMessage].ToString();

            if (TempData.ContainsKey(TempDataKeys.ApplicationReviewStatusInfoMessage))
                viewModel.ApplicationReviewStatusHeaderInfoMessage = TempData[TempDataKeys.ApplicationReviewStatusInfoMessage].ToString();

            return View(viewModel);
        }


        private IActionResult HandleRedirectOfEditableVacancy(Vacancy vacancy)
        {
            if (_utility.IsTaskListCompleted(vacancy))
            {
                return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {vacancy.TrainingProvider.Ukprn, vacancyId = vacancy.Id});
            }
            return RedirectToRoute(RouteNames.ProviderTaskListGet, new {vacancy.TrainingProvider.Ukprn, vacancyId = vacancy.Id});
        }
    }
}