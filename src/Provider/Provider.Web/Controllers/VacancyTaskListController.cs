using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
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

        public VacancyTaskListController (VacancyTaskListOrchestrator orchestrator, ServiceParameters serviceParameters, IConfiguration configuration)
        {
            _orchestrator = orchestrator;
            _serviceParameters = serviceParameters;
            _configuration = configuration;
        }
        
        [HttpGet("create/start", Name=RouteNames.CreateVacancyStart)]
        public IActionResult StartVacancyCreate(VacancyRouteModel vrm)
        {
            if (_serviceParameters.VacancyType == VacancyType.Traineeship 
                && DateTime.TryParse(_configuration["TraineeshipCutOffDate"], out var traineeshipCutOffDate))
            {
                if (traineeshipCutOffDate != DateTime.MinValue && traineeshipCutOffDate < DateTime.UtcNow)
                {
                    return RedirectPermanent(_configuration["ProviderSharedUIConfiguration:DashboardUrl"]);
                }
            }
            
            return View("StartCreateVacancy", vrm);
        }
        
        [HttpGet("create/task-list", Name = RouteNames.ProviderTaskListCreateGet)]
        public async Task<IActionResult> CreateProviderTaskList(VacancyRouteModel vrm, [FromQuery] string employerAccountId)
        {
            if (_serviceParameters.VacancyType == VacancyType.Traineeship 
                && DateTime.TryParse(_configuration["TraineeshipCutOffDate"], out var traineeshipCutOffDate))
            {
                if (traineeshipCutOffDate != DateTime.MinValue && traineeshipCutOffDate < DateTime.UtcNow)
                {
                    return RedirectPermanent(_configuration["ProviderSharedUIConfiguration:DashboardUrl"]);
                }
            }
            
            var viewModel = await _orchestrator.GetCreateVacancyTaskListModel(vrm, employerAccountId);
            viewModel.SetSectionStates(viewModel, ModelState);
            
            return View("ProviderTaskList", viewModel);
        }
        
        [HttpGet("{vacancyId:guid}/task-list", Name = RouteNames.ProviderTaskListGet)]
        
        public async Task<IActionResult> ProviderTaskList(VacancyRouteModel vrm)
        {
            if (_serviceParameters.VacancyType == VacancyType.Traineeship 
                && DateTime.TryParse(_configuration["TraineeshipCutOffDate"], out var traineeshipCutOffDate))
            {
                if (traineeshipCutOffDate != DateTime.MinValue && traineeshipCutOffDate < DateTime.UtcNow)
                {
                    return RedirectPermanent(_configuration["ProviderSharedUIConfiguration:DashboardUrl"]);
                }
            }
            
            var viewModel = await _orchestrator.GetVacancyTaskListModel(vrm); 
            viewModel.SetSectionStates(viewModel, ModelState);

            if (viewModel.Status == VacancyStatus.Rejected || viewModel.Status == VacancyStatus.Referred)
            {
                return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {vrm.Ukprn, vrm.VacancyId});
            }
            
            return View(viewModel);
        }
    }
}