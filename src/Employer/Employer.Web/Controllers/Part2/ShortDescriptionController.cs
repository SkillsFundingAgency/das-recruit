using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.ShortDescription;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class ShortDescriptionController : Controller
    {
        private readonly ShortDescriptionOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public ShortDescriptionController(ShortDescriptionOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }
        
        [HttpGet("description", Name = RouteNames.ShortDescription_Get)]
        public async Task<IActionResult> ShortDescription(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetShortDescriptionViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("description", Name = RouteNames.ShortDescription_Post)]
        public async Task<IActionResult> ShortDescription(ShortDescriptionEditModel m)
        {
            var response = await _orchestrator.PostShortDescriptionEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }
            
            var vm = await _orchestrator.GetShortDescriptionViewModelAsync(m);
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            string vacancyDescriptionIndexGet = _feature.IsFeatureEnabled(FeatureNames.FaaV2Improvements) 
                ? RouteNames.VacancyWorkDescription_Index_Get : RouteNames.VacancyDescription_Index_Get;
            
            return RedirectToRoute(vm.IsTaskListCompleted 
                ? RouteNames.EmployerCheckYourAnswersGet 
                : vacancyDescriptionIndexGet, 
                new {m.VacancyId, m.EmployerAccountId});
        }
    }
}