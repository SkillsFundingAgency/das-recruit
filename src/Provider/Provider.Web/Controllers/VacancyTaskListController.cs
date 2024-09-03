using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.VacanciesRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class VacancyTaskListController : Controller
    {
        private readonly VacancyTaskListOrchestrator _orchestrator;
        private readonly ServiceParameters _serviceParameters;
        private readonly IConfiguration _configuration;
        private readonly bool _isFaaV2Enabled;

        public VacancyTaskListController (VacancyTaskListOrchestrator orchestrator, ServiceParameters serviceParameters, IConfiguration configuration, IFeature feature)
        {
            _orchestrator = orchestrator;
            _serviceParameters = serviceParameters;
            _configuration = configuration;
            _isFaaV2Enabled = feature.IsFeatureEnabled(FeatureNames.FaaV2Improvements);
        }
        
        [HttpGet("create/start", Name=RouteNames.CreateVacancyStart)]
        public IActionResult StartVacancyCreate(VacancyRouteModel vrm)
        {
            if (IsTraineeshipsDisabled())
            {
                return RedirectPermanent($"{_configuration["ProviderSharedUIConfiguration:DashboardUrl"]}account");
            }
            
            return View("StartCreateVacancy", vrm);
        }
        
        [HttpGet("create/task-list", Name = RouteNames.ProviderTaskListCreateGet)]
        public async Task<IActionResult> CreateProviderTaskList(VacancyRouteModel vrm, [FromQuery] string employerAccountId)
        {
            if (IsTraineeshipsDisabled())
            {
                return RedirectPermanent($"{_configuration["ProviderSharedUIConfiguration:DashboardUrl"]}account");
            }
            
            var viewModel = await _orchestrator.GetCreateVacancyTaskListModel(vrm, employerAccountId);
            viewModel.SetSectionStates(viewModel, ModelState, _isFaaV2Enabled);
            
            return View("ProviderTaskList", viewModel);
        }
        
        [HttpGet("{vacancyId:guid}/task-list", Name = RouteNames.ProviderTaskListGet)]
        
        public async Task<IActionResult> ProviderTaskList(VacancyRouteModel vrm)
        {
            if (IsTraineeshipsDisabled())
            {
                return RedirectPermanent($"{_configuration["ProviderSharedUIConfiguration:DashboardUrl"]}account");
            }
            
            var viewModel = await _orchestrator.GetVacancyTaskListModel(vrm); 
            viewModel.SetSectionStates(viewModel, ModelState, _isFaaV2Enabled);

            if (viewModel.Status == VacancyStatus.Rejected || viewModel.Status == VacancyStatus.Referred)
            {
                return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {vrm.Ukprn, vrm.VacancyId});
            }
            
            return View(viewModel);
        }
        
        private bool IsTraineeshipsDisabled()
        {
            if (_serviceParameters.VacancyType == VacancyType.Traineeship 
                && DateTime.TryParse(_configuration["TraineeshipCutOffDate"], out var traineeshipCutOffDate))
            {
                if (traineeshipCutOffDate != DateTime.MinValue && traineeshipCutOffDate < DateTime.UtcNow)
                {
                    return true;
                }
            }
            return false;
        }
    }
}