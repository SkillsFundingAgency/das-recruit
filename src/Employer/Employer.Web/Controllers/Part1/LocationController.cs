using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Location;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class LocationController : EmployerControllerBase
    {
        private readonly LocationOrchestrator _orchestrator;
        public LocationController(LocationOrchestrator orchestrator, IHostingEnvironment hostingEnvironment)
            : base(hostingEnvironment)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("location", Name = RouteNames.Location_Get)]
        public async Task<IActionResult> Location(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {   
            var employerInfoModel = GetEmployerInfoCookie(vrm.VacancyId);         
            var vm = await _orchestrator.GetLocationViewModelAsync(vrm, employerInfoModel, User.ToVacancyUser());
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("location", Name = RouteNames.Location_Post)]
        public async Task<IActionResult> Location(LocationEditModel model, [FromQuery] bool wizard)
        {
            var employerInfoModel = GetEmployerInfoCookie(model.VacancyId);
            var response = await _orchestrator.PostLocationEditModelAsync(model, employerInfoModel, User.ToVacancyUser());
            
            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetLocationViewModelAsync(model, employerInfoModel, User.ToVacancyUser());
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            ClearCookie(model.VacancyId);

            return wizard
                ? RedirectToRoute(RouteNames.Training_Get)
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get);
        }        
    }
}