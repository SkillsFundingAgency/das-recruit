using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Location;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class LocationController : EmployerControllerBase
    {
        private readonly LocationOrchestrator _orchestrator;

        public LocationController(LocationOrchestrator orchestrator, IHostingEnvironment hostingEnvironment)
            :base(hostingEnvironment)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("location", Name = RouteNames.Location_Get)]
        public async Task<IActionResult> Location(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var employerInfoModel = GetVacancyEmployerInfoCookie(vrm.VacancyId.GetValueOrDefault());
            var vm = await _orchestrator.GetLocationViewModelAsync(vrm, employerInfoModel, User.ToVacancyUser());
            
            vm.PageInfo.SetWizard(wizard);
            
            //back link is available only if cookie is not there (back link in part 1)
            //or part 2 has not started (coming from preview)
            vm.CanShowBackLink = employerInfoModel != null || vm.PageInfo.IsWizard;

            //if cookie is missing and user is in part1 then create the cookie to support back navigation
            //either part 1 is not completed or part 1 is completed but part 2 has not started
            if (employerInfoModel == null && (!vm.PageInfo.HasCompletedPartOne || !vm.PageInfo.HasStartedPartTwo))
            {
                employerInfoModel = await _orchestrator.GetVacancyEmployerInfoModelAsync(vrm);
                SetVacancyEmployerInfoCookie(employerInfoModel);
            }
            return View(vm);
        }

        [HttpPost("location", Name = RouteNames.Location_Post)]
        public async Task<IActionResult> Location(LocationEditModel m, [FromQuery] bool wizard)
        {
            var employerInfoModel = GetVacancyEmployerInfoCookie(m.VacancyId.GetValueOrDefault());
            var response = await _orchestrator.PostLocationEditModelAsync(m, User.ToVacancyUser(), User.GetUkprn(), employerInfoModel);

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetLocationViewModelAsync(m, employerInfoModel, User.ToVacancyUser());
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            return wizard
                ? RedirectToRoute(RouteNames.Training_Get)
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get, null, Anchors.AboutEmployerSection);
        }

        [HttpGet("location-cancel", Name = RouteNames.Location_Cancel)]
        public IActionResult Cancel(VacancyRouteModel vrm, [FromQuery] bool wizard)
        {
            return CancelAndRedirect(wizard);
        }  
    }
}