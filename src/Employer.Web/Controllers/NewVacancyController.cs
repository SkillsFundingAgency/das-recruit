using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.NewVacancy;
using Esfa.Recruit.Employer.Web.Configuration.Routes;
using Esfa.Recruit.Storage.Client.Core.Commands;
using Esfa.Recruit.Employer.Web.Orchestrators;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{    
    public class NewVacancyController : Controller
    {
        private readonly INewVacancyOrchestrator _orchestrator;

        public NewVacancyController(INewVacancyOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("accounts/{employerAccountId}/new-vacancy", Name = RouteNames.NewVacancy_Index_Get)]
        public IActionResult Index()
        {
            var vm = _orchestrator.GetIndexViewModel();
            return View(vm);
        }

        [HttpPost("accounts/{employerAccountId}/new-vacancy", Name = RouteNames.NewVacancy_Index_Post)]
        public async Task<IActionResult> Index(IndexViewModel vm)
        {
            if(!ModelState.IsValid)
            {
                return View(vm);
            }

            var newVacancyId = await _orchestrator.PostIndexViewModelAsync(vm);

            //todo: Do something with the id

            return RedirectToRoute(RouteNames.Sections_Index_Get);
        }
    }
}