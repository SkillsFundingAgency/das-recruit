using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.VacanciesRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class VacancyTaskListController : Controller
    {
        private readonly VacancyTaskListOrchestrator _orchestrator;
        private readonly bool _isFaaV2Enabled;

        public VacancyTaskListController (VacancyTaskListOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _isFaaV2Enabled = feature.IsFeatureEnabled(FeatureNames.FaaV2Improvements);
        }
        
        [HttpGet("create/start", Name=RouteNames.CreateVacancyStart)]
        public IActionResult StartVacancyCreate(VacancyRouteModel vrm)
        {
            return View("StartCreateVacancy", vrm);
        }
        
        [HttpGet("create/task-list", Name = RouteNames.ProviderTaskListCreateGet)]
        public async Task<IActionResult> CreateProviderTaskList(VacancyRouteModel vrm, [FromQuery] string employerAccountId)
        {
            var viewModel = await _orchestrator.GetCreateVacancyTaskListModel(vrm, employerAccountId);
            viewModel.SetSectionStates(viewModel, ModelState, _isFaaV2Enabled);
            
            return View("ProviderTaskList", viewModel);
        }
        
        [HttpGet("{vacancyId:guid}/task-list", Name = RouteNames.ProviderTaskListGet)]
        
        public async Task<IActionResult> ProviderTaskList(VacancyRouteModel vrm)
        {
            var viewModel = await _orchestrator.GetVacancyTaskListModel(vrm); 
            viewModel.SetSectionStates(viewModel, ModelState, _isFaaV2Enabled);

            if (viewModel.Status == VacancyStatus.Rejected || viewModel.Status == VacancyStatus.Referred)
            {
                return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {vrm.Ukprn, vrm.VacancyId});
            }
            
            return View(viewModel);
        }
    }
}