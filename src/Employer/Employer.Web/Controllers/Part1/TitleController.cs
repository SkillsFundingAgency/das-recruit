using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Title;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;


namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountRoutePath)]
    public class TitleController : Controller
    {
        private const string VacancyTitleRoute = "vacancies/{vacancyId:guid}/title";
        private readonly TitleOrchestrator _orchestrator;

        public TitleController(TitleOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("create-vacancy", Name = RouteNames.CreateVacancy_Get)]
        public async Task<IActionResult> Title([FromRoute] string employerAccountId)
        {
            var vm = await _orchestrator.GetTitleViewModel(employerAccountId);
            vm.PageInfo.SetWizard();
            vm = (TitleViewModel) GetReferredDataFromTempData(vm);
            SetBackText(vm);
            vm.ReturnToMALink = SetBackLinkRoute(vm);
            return View(vm);
        }

        private void SetBackText(TitleViewModel vm)
        {
            vm.ReturnToMALinkText =
                vm.PageInfo.IsWizard && !string.IsNullOrWhiteSpace(vm.ReferredFromMa_ProgrammeId)
                    ? "Back to your saved favourites"
                    : "Return to home";
        }

        private string SetBackLinkRoute(TitleViewModel vm)
        {
            var referredFromMa = Convert.ToBoolean(TempData.Peek(TempDataKeys.ReferredFromMa));
            if (referredFromMa)
                return string.IsNullOrWhiteSpace(vm.ReferredFromMa_ProgrammeId)
                    ? RouteNames.Dashboard_Account_Home
                    : RouteNames.EmployerFavourites;
            return RouteNames.Dashboard_Index_Get;
        }

        [HttpGet(VacancyTitleRoute, Name = RouteNames.Title_Get)]
        public async Task<IActionResult> Title(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetTitleViewModelAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            vm.ReturnToMALink = SetBackLinkRoute(vm);
            return View(vm);
        }

        [HttpPost("create-vacancy", Name = RouteNames.CreateVacancy_Post)]
        [HttpPost(VacancyTitleRoute, Name = RouteNames.Title_Post)]
        public async Task<IActionResult> Title(TitleEditModel m, [FromQuery] bool wizard)
        {
            m = GetReferredDataFromTempData(m);
            var response = await _orchestrator.PostTitleEditModelAsync(m, User.ToVacancyUser());
            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if(!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetTitleViewModelAsync(m);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }
            if (m.ReferredFromMa_FromSavedFavourites)
                return RedirectToRoute(RouteNames.DisplayVacancy_Get, new { vacancyId = response.Data });
            return wizard
                ? RedirectToRoute(RouteNames.Training_Get, new {vacancyId = response.Data})
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }

        private TitleEditModel GetReferredDataFromTempData(TitleEditModel m)
        {
            m.ReferredFromMa_FromSavedFavourites =
                !string.IsNullOrWhiteSpace(Convert.ToString(TempData.Peek(TempDataKeys.ReferredFromMaUkprn)))
                || !string.IsNullOrWhiteSpace(Convert.ToString(TempData.Peek(TempDataKeys.ReferredFromMaProgrammeId)));
            m.ReferredFromMa_Ukprn = Convert.ToString(TempData.Peek(TempDataKeys.ReferredFromMaUkprn));
            m.ReferredFromMa_ProgrammeId = Convert.ToString(TempData.Peek(TempDataKeys.ReferredFromMaProgrammeId));
            return m;
        }
    }
}