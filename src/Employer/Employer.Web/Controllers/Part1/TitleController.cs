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
using Esfa.Recruit.Shared.Web.Mappers;

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
            vm.ReturnToMALinkText = Convert.ToBoolean(TempData.Peek(TempDataKeys.ReferredFromMAHome_FromSavedFavourites)) 
                ? "Back to your saved favourites" : "Return to home";
            vm.PageInfo.SetWizard();
            return View(vm);
        }

        [HttpGet(VacancyTitleRoute, Name = RouteNames.Title_Get)]
        public async Task<IActionResult> Title(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetTitleViewModelAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("create-vacancy", Name = RouteNames.CreateVacancy_Post)]
        [HttpPost(VacancyTitleRoute, Name = RouteNames.Title_Post)]
        public async Task<IActionResult> Title(TitleEditModel m, [FromQuery] bool wizard)
        {
            m.ReferredFromMAHome_FromSavedFavourites = Convert.ToBoolean(TempData.Peek(TempDataKeys.ReferredFromMAHome_FromSavedFavourites));
            m.ReferredFromMAHome_UKPRN = Convert.ToString(TempData.Peek(TempDataKeys.ReferredFromMAHome_UKPRN));
            m.ReferredFromMAHome_ProgrammeId = Convert.ToString(TempData.Peek(TempDataKeys.ReferredFromMAHome_ApprenticeshipId));
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
            var currentVacancy = await _orchestrator.GetCurrentVacancy(m,response.Data);
            if (m.ReferredFromMAHome_FromSavedFavourites)
            {
                if (currentVacancy.TrainingProvider != null && !string.IsNullOrWhiteSpace(currentVacancy.ProgrammeId))
                    return RedirectToRoute(RouteNames.NumberOfPositions_Get, new { vacancyId = response.Data });
                if (currentVacancy.TrainingProvider == null)
                    return RedirectToRoute(RouteNames.TrainingProvider_Select_Get, new { vacancyId = response.Data });
            }
                
            return wizard
                ? RedirectToRoute(RouteNames.Training_Get, new {vacancyId = response.Data})
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }
    }
}