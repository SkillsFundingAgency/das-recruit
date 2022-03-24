using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Title;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class TitleController : Controller
    {
        private const string NewVacancyTitleRoute = "create-vacancy";
        private const string ExistingVacancyTitleRoute = "vacancies/{vacancyId:guid}/title";
        private readonly TitleOrchestrator _orchestrator;
        private readonly IFeature _feature;
        public IProviderVacancyClient ProviderVacancyClient { get; }

        public TitleController(TitleOrchestrator orchestrator, IProviderVacancyClient providerVacancyClient, IFeature feature)
        {
            this.ProviderVacancyClient = providerVacancyClient;
            _orchestrator = orchestrator;
            _feature = feature;
        }

        [HttpGet(NewVacancyTitleRoute, Name = RouteNames.CreateVacancy_Get)]
        public async Task<IActionResult> Title([FromQuery] string employerAccountId)
        {
            var vm = await _orchestrator.GetTitleViewModelForNewVacancyAsync(employerAccountId, User.GetUkprn());
            vm.PageInfo.SetWizard();
            return View(vm);
        }

        [HttpGet(ExistingVacancyTitleRoute, Name = RouteNames.Title_Get)]
        public async Task<IActionResult> Title(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {            
            var vm = await _orchestrator.GetTitleViewModelForExistingVacancyAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost(NewVacancyTitleRoute, Name = RouteNames.CreateVacancy_Post)]
        [HttpPost(ExistingVacancyTitleRoute, Name = RouteNames.Title_Post)]
        public async Task<IActionResult> Title(VacancyRouteModel vrm, TitleEditModel model, [FromQuery] bool wizard)
        {
            var ukprn = User.GetUkprn();
            var response = await _orchestrator.PostTitleEditModelAsync(vrm, model, User.ToVacancyUser(), ukprn);
            
            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if(!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetTitleViewModelFromEditModelAsync(vrm, model, ukprn);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            return wizard
                ? _feature.IsFeatureEnabled(FeatureNames.ProviderTaskList) ? RedirectToRoute(RouteNames.LegalEntity_Get, new {vrm.Ukprn, vacancyId = response.Data}) : RedirectToRoute(RouteNames.Training_Get, new {vrm.Ukprn, vacancyId = response.Data})
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get, new {vrm.Ukprn, vacancyId = response.Data});
        }    
    }
}