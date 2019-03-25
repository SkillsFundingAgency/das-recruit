using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Employer.Web.Views;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using System.Linq;
using Esfa.Recruit.Employer.Web.Extensions;
using Microsoft.AspNetCore.Hosting;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class EmployerController : EmployerControllerBase
    {
        private readonly EmployerOrchestrator _orchestrator;
        

        public EmployerController(EmployerOrchestrator orchestrator, IHostingEnvironment hostingEnvironment)
            : base(hostingEnvironment)
        {
            _orchestrator = orchestrator;            
        }

        [HttpGet("employer", Name = RouteNames.Employer_Get)]
        public async Task<IActionResult> Employer(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetEmployerViewModelAsync(vrm);
            ClearCookie(vrm.VacancyId);
            if(vm.HasOnlyOneOrganisation)
            {
                var org = vm.Organisations.FirstOrDefault();
                SetEmployerInfoCookie(vrm.VacancyId, org.Id);
                return RedirectToRoute(RouteNames.EmployerName_Get);
            }
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }
        
        [HttpPost("employer", Name = RouteNames.Employer_Post)]
        public async Task<IActionResult> Employer(EmployerEditModel m, [FromQuery] bool wizard)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetEmployerViewModelAsync(m);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            SetEmployerInfoCookie(m.VacancyId, m.SelectedOrganisationId.GetValueOrDefault());
            return RedirectToRoute(RouteNames.EmployerName_Get);
        }
    }
}