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
        private readonly ManageApprenticeshipsLinkHelper _linkHelper;

        public TitleController(TitleOrchestrator orchestrator, ManageApprenticeshipsLinkHelper linkHelper)
        {
            _orchestrator = orchestrator;
            _linkHelper = linkHelper;
        }
        
        [HttpGet("create-vacancy", Name = RouteNames.CreateVacancy_Get)]
        public async Task<IActionResult> Title([FromRoute] string employerAccountId)
        {
            var vm = await _orchestrator.GetTitleViewModel(employerAccountId);
            vm.PageInfo.SetWizard();
            GetReferredDataFromTempData(vm);
            vm.ReturnToMALinkText = SetBackText(vm);
            vm.ReturnToMALink = SetBackLinkRoute(vm);
            return View(vm);
        }

        private string SetBackText(TitleViewModel vm)
        {
            return vm.PageInfo.IsWizard && !string.IsNullOrWhiteSpace(vm.ReferredProgrammeId)
                    ? "Back to your saved favourites"
                    : "Return to home";
        }

        private string SetBackLinkRoute(TitleViewModel vm)
        {
            var referredFromMa = Convert.ToBoolean(TempData.Peek(TempDataKeys.ReferredFromMa));
            if (referredFromMa)
                return string.IsNullOrWhiteSpace(vm.ReferredProgrammeId)
                    ? RouteNames.Dashboard_Account_Home
                    : GenerateEmployerFavouriteUrl();
            return RouteNames.Dashboard_Index_Get;
        }

        private string GenerateEmployerFavouriteUrl()
        {
            var ukprn = Convert.ToString(TempData.Peek(TempDataKeys.ReferredUkprn));
            var programmeId = Convert.ToString(TempData.Peek(TempDataKeys.ReferredProgrammeId));
            return Url.RouteUrl(RouteNames.EmployerFavourites,
                new {referredUkprn = ukprn, referredProgrammeId = programmeId});
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
            GetReferredDataFromTempData(m);
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
            if (m.ReferredFromSavedFavourites)
                return RedirectToRoute(RouteNames.DisplayVacancy_Get, new { vacancyId = response.Data });
            return wizard
                ? RedirectToRoute(RouteNames.Training_Get, new {vacancyId = response.Data})
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }

        private void GetReferredDataFromTempData(TitleEditModel m)
        {
            m.ReferredFromSavedFavourites =
                !string.IsNullOrWhiteSpace(Convert.ToString(TempData.Peek(TempDataKeys.ReferredUkprn)))
                || !string.IsNullOrWhiteSpace(Convert.ToString(TempData.Peek(TempDataKeys.ReferredProgrammeId)));
            m.ReferredUkprn = Convert.ToString(TempData.Peek(TempDataKeys.ReferredUkprn));
            m.ReferredProgrammeId = Convert.ToString(TempData.Peek(TempDataKeys.ReferredProgrammeId));
        }
    }
}