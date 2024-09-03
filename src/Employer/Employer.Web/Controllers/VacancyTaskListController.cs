using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Middleware;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerOrTransactorAccount))]
    public class VacancyTaskListController : Controller
    {
        private readonly VacancyTaskListOrchestrator _orchestrator;
        private readonly bool _isFaaV2Enabled;

        public VacancyTaskListController (VacancyTaskListOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _isFaaV2Enabled = feature.IsFeatureEnabled(FeatureNames.FaaV2Improvements);
        }

        [HttpGet("vacancies/create/start", Name=RouteNames.CreateVacancyStart)]
        public IActionResult StartVacancyCreate(VacancyRouteModel vrm)
        {
            return View("StartCreateVacancy", vrm);
        }

        [HttpGet("vacancies/create/task-list", Name = RouteNames.EmployerTaskListCreateGet)]
        public async Task<IActionResult> CreateEmployerTaskList(VacancyRouteModel vrm)
        {
            var viewModel = await _orchestrator.GetCreateVacancyTaskListModel(vrm);
            viewModel.SetSectionStates(viewModel, ModelState,_isFaaV2Enabled);
            
            return View("EmployerTaskList", viewModel);
        }

        [HttpGet("vacancies/{vacancyId:guid}/task-list", Name = RouteNames.EmployerTaskListGet)]
        public async Task<IActionResult> EmployerTaskList(VacancyRouteModel vrm)
        {
            var viewModel = await _orchestrator.GetVacancyTaskListModel(vrm); 
            
            viewModel.SetSectionStates(viewModel, ModelState,_isFaaV2Enabled);

            if (viewModel.Status == VacancyStatus.Rejected
                || viewModel.Status == VacancyStatus.Referred
                || viewModel.Status == VacancyStatus.Review)
            {
                return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, vrm);
            }
            
            return View(viewModel);
        }
    }
}