using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.NewVacancy;
using Esfa.Recruit.Employer.Web.Configuration.Routes;
using Esfa.Recruit.Storage.Client.Core.Commands;
using Esfa.Recruit.Employer.Web.Orchestrators;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{    
    [Route("accounts/{employerAccountId}")]
    public class NewVacancyController : Controller
    {
        private readonly NewVacancyOrchestrator _orchestrator;

        public NewVacancyController(NewVacancyOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("new-vacancy", Name = RouteNames.NewVacancy_Index_Get)]
        public IActionResult Index()
        {
            var vm = _orchestrator.GetIndexViewModel();
            return View(vm);
        }

        [HttpPost("new-vacancy", Name = RouteNames.NewVacancy_Index_Post)]
        public async Task<IActionResult> Index(IndexViewModel vm)
        {
            if(!ModelState.IsValid)
            {
                return View(vm);
            }
            
            var vacancyId = await _orchestrator.PostIndexViewModelAsync(vm);
            
            return RedirectToRoute(RouteNames.Sections_Index_Get, new { vacancyId });
        }
    }
}