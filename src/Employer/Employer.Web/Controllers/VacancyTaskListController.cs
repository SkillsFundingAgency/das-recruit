using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Middleware;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
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

        public VacancyTaskListController (VacancyTaskListOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
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
            return View("EmployerTaskList", viewModel);
        }

        [HttpGet("vacancies/{vacancyId:guid}/task-list", Name = RouteNames.EmployerTaskListGet)]
        public async Task<IActionResult> EmployerTaskList(VacancyRouteModel vrm)
        {
            var viewModel = await _orchestrator.GetVacancyTaskListModel(vrm); 
            return viewModel.Status is VacancyStatus.Rejected or VacancyStatus.Referred or VacancyStatus.Review
                ? RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, vrm)
                : View(viewModel);
        }
    }
}