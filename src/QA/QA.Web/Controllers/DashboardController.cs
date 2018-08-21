using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Orchestrators;
using Esfa.Recruit.Qa.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DashboardOrchestrator _orchestrator;

        public DashboardController(DashboardOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("/", Name = RouteNames.Dashboard_Index_Get)]
        public async Task<IActionResult> Index()
        {
            var vm = await _orchestrator.GetDashboardViewModelAsync();
            
            return View(vm);
        }

        [HttpPost(Name = RouteNames.Dashboard_Index_Post)]
        public async Task<IActionResult> Index(DashboardViewModel viewModel)
        {
            var vm = await _orchestrator.GetDashboardViewModelAsync();

            vm.LastSearchTerm = viewModel.SearchTerm;
            vm.IsPostBack = true;
            if (int.TryParse(vm.LastSearchTerm, out int _))
            {
                vm.SearchResults = new List<ReviewVacancyModel>()
                {
                    new ReviewVacancyModel() { AssignedTo = "John Doe", VacancyReference = "VAC100101", ClosingDate = DateTime.Now.AddDays(10), EmployerName = "Manufacturing Engineering Ltd.", VacancyTitle = "Mechanical Apprenticeship", SubmittedDate = DateTime.Now.AddDays(-1), AssignedTimeElapsed = "1h 20m", ReviewId = Guid.Empty},
                    new ReviewVacancyModel() { AssignedTo = "Steve Smith", VacancyReference = "VAC100123", ClosingDate = DateTime.Now.AddDays(20), EmployerName = "Amazing Restaurant", VacancyTitle = "Chef Apprenticeship", SubmittedDate = DateTime.Now.AddDays(-1), AssignedTimeElapsed = "20h 5m", ReviewId = Guid.Empty},
                    new ReviewVacancyModel() { AssignedTo = null, VacancyReference = "VAC100111", ClosingDate = DateTime.Now.AddDays(30), EmployerName = "Newspaper Company.", VacancyTitle = "Journalist Apprenticeship", SubmittedDate = DateTime.Now.AddDays(-2), AssignedTimeElapsed = null, ReviewId = Guid.Empty},
                    new ReviewVacancyModel() { AssignedTo = null, VacancyReference = "VAC100135", ClosingDate = DateTime.Now.AddDays(40), EmployerName = "Amazing Restaurant", VacancyTitle = "Chef Apprenticeship", SubmittedDate = DateTime.Now.AddDays(-1), AssignedTimeElapsed = null, ReviewId = Guid.Empty},
                };
            }

            return View(vm);
        }
    }
}
