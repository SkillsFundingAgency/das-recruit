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
        public IActionResult Title()
        {
            var vm = _orchestrator.GetTitleViewModel();
            vm.PageInfo.SetWizard();
            vm.BackLinkText = SetBackText(vm);
            vm.BackLinkRoute = SetBackLinkRoute(vm);
            return View(vm);
        }

        private string SetBackText(TitleViewModel vm)
        {
            return !string.IsNullOrWhiteSpace(GetReferredProgrammeId(vm.VacancyId))
                    ? "Back to your saved favourites"
                    : "Return to home";
        }

        private string SetBackLinkRoute(TitleViewModel vm)
        {
            var referredFromMa = Convert.ToBoolean(TempData.Peek(TempDataKeys.ReferredFromMa));
            if (referredFromMa)
                return string.IsNullOrWhiteSpace(GetReferredProgrammeId(vm.VacancyId))
                    ? Url.RouteUrl(RouteNames.Dashboard_Account_Home)
                    : GenerateEmployerFavouriteUrl(vm);
            return Url.RouteUrl(RouteNames.Dashboard_Index_Get);
        }

        private string GenerateEmployerFavouriteUrl(TitleViewModel vm)
        {
            return Url.RouteUrl(RouteNames.EmployerFavourites,
                new {referredUkprn = GetReferredProviderUkprn(vm.VacancyId), referredProgrammeId = GetReferredProgrammeId(vm.VacancyId)});
        }

        [HttpGet(VacancyTitleRoute, Name = RouteNames.Title_Get)]
        public async Task<IActionResult> Title(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetTitleViewModelAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            vm.BackLinkRoute = SetBackLinkRoute(vm);
            vm.BackLinkText = SetBackText(vm);
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
            {
                TempData[TempDataKeys.ReferredUkprn + response.Data] = TempData[TempDataKeys.ReferredUkprn];
                TempData[TempDataKeys.ReferredProgrammeId + response.Data] = TempData[TempDataKeys.ReferredProgrammeId];
                return RedirectToRoute(RouteNames.DisplayVacancy_Get, new { vacancyId = response.Data });
            }
            return wizard
                ? RedirectToRoute(RouteNames.Training_Get, new {vacancyId = response.Data})
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }

        private void GetReferredDataFromTempData(TitleEditModel m)
        {
            m.ReferredFromMa = Convert.ToBoolean(TempData.Peek(TempDataKeys.ReferredFromMa));
            m.ReferredUkprn = GetReferredProviderUkprn(m.VacancyId);
            m.ReferredProgrammeId = GetReferredProgrammeId(m.VacancyId);
        }

        private string GetReferredProgrammeId(Guid? vacancyId)
        {
            if (vacancyId == null)
            {
                return Convert.ToString(TempData.Peek(TempDataKeys.ReferredProgrammeId));
            }
            return Convert.ToString(TempData.Peek(TempDataKeys.ReferredProgrammeId + vacancyId));
        }

        private string GetReferredProviderUkprn(Guid? vacancyId)
        {
            if (vacancyId == null)
            {
                return Convert.ToString(TempData.Peek(TempDataKeys.ReferredUkprn));
            }
            return Convert.ToString(TempData.Peek(TempDataKeys.ReferredUkprn + vacancyId));
        }
    }
}