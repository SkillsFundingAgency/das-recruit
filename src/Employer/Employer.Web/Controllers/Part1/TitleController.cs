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
using Esfa.Recruit.Shared.Web.FeatureToggle;


namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountRoutePath)]
    public class TitleController : Controller
    {
        private const string VacancyTitleRoute = "vacancies/{vacancyId:guid}/title";
        private readonly TitleOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public TitleController(TitleOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }

        [HttpGet("create-vacancy", Name = RouteNames.CreateVacancy_Get)]
        public async Task<IActionResult> Title()
        {
            var vm = _orchestrator.GetTitleViewModel();
            await PopulateModelFromTempData(vm);
            vm.PageInfo.SetWizard();
            return View(vm);
        }

        [HttpGet(VacancyTitleRoute, Name = RouteNames.Title_Get)]
        public async Task<IActionResult> Title(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetTitleViewModelAsync(vrm);
            await PopulateModelFromTempData(vm);
            vm.PageInfo.SetWizard();
            return View(vm);
        }

        [HttpPost("create-vacancy", Name = RouteNames.CreateVacancy_Post)]
        [HttpPost(VacancyTitleRoute, Name = RouteNames.Title_Post)]
        public async Task<IActionResult> Title(TitleEditModel m, [FromQuery] bool wizard)
        {
            PopulateModelFromTempData(m);
            var response = await _orchestrator.PostTitleEditModelAsync(m, User.ToVacancyUser());
            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetTitleViewModelAsync(m);
                await PopulateModelFromTempData(vm);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            if (m.ReferredFromSavedFavourites)
            {
                if (m.VacancyId == null)
                {
                    TempData[TempDataKeys.ReferredUkprn + response.Data] = TempData[TempDataKeys.ReferredUkprn];
                    TempData[TempDataKeys.ReferredProgrammeId + response.Data] = TempData[TempDataKeys.ReferredProgrammeId];
                    TempData.Remove(TempDataKeys.ReferredUkprn);
                    TempData.Remove(TempDataKeys.ReferredProgrammeId);
                }
                return RedirectToRoute(RouteNames.DisplayVacancy_Get, new { vacancyId = response.Data });
            }

            if (_feature.IsFeatureEnabled(FeatureNames.EmployerTaskList))
            {
                if (wizard)
                {
                    return RedirectToRoute(RouteNames.Employer_Get ,new { vacancyId = response.Data });
                }
                return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet);
            }
            
            return wizard
                ? RedirectToRoute(RouteNames.Training_Get, new { vacancyId = response.Data })
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }

        private void PopulateModelFromTempData(TitleEditModel m)
        {
            m.ReferredFromMa = Convert.ToBoolean(TempData.Peek(TempDataKeys.ReferredFromMa));
            m.ReferredUkprn = GetReferredProviderUkprn(m.VacancyId);
            m.ReferredProgrammeId = GetReferredProgrammeId(m.VacancyId);
        }

        private async Task PopulateModelFromTempData(TitleViewModel vm)
        {
            vm.ReferredFromMa = Convert.ToBoolean(TempData.Peek(TempDataKeys.ReferredFromMa));
            vm.ReferredUkprn = GetReferredProviderUkprn(vm.VacancyId);
            vm.ReferredProgrammeId = GetReferredProgrammeId(vm.VacancyId);
            await UpdateTextAndLinks(vm);
        }

        private async Task UpdateTextAndLinks(TitleViewModel vm)
        {
            if (vm.ReferredFromMa && vm.VacancyId == null)
            {
                if(!string.IsNullOrWhiteSpace(vm.ReferredProgrammeId))
                {
                    var training = await _orchestrator.GetProgramme(vm.ReferredProgrammeId);
                    vm.TrainingTitle = training.Title + ", Level: " + training.EducationLevelNumber;                    
                    vm.CancelLinkRoute = GenerateEmployerFavouriteUrl(vm);
                }
                else
                {                    
                    vm.CancelLinkRoute = Url.RouteUrl(RouteNames.Dashboard_Account_Home);
                }
            }
            else
            {                
                vm.CancelLinkRoute = Url.RouteUrl(vm.CancelLink);
            }
        }

        private string GenerateEmployerFavouriteUrl(TitleViewModel vm)
        {
            return Url.RouteUrl(RouteNames.EmployerFavourites,
                new { referredUkprn = GetReferredProviderUkprn(vm.VacancyId), referredProgrammeId = GetReferredProgrammeId(vm.VacancyId) });
        }

        private string GetReferredProgrammeId(Guid? vacancyId)
        {
            return Convert.ToString(vacancyId == null ? TempData.Peek(TempDataKeys.ReferredProgrammeId) : TempData.Peek(TempDataKeys.ReferredProgrammeId + vacancyId));
        }

        private string GetReferredProviderUkprn(Guid? vacancyId)
        {
            return Convert.ToString(vacancyId == null ? TempData.Peek(TempDataKeys.ReferredUkprn) : TempData.Peek(TempDataKeys.ReferredUkprn + vacancyId));
        }
    }
}