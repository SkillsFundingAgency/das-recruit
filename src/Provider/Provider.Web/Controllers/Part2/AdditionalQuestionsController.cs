using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Interfaces;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.AdditionalQuestions;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2;

[Route(RoutePaths.AccountVacancyRoutePath)]
[Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
public class AdditionalQuestionsController(IAdditionalQuestionsOrchestrator orchestrator) : Controller
{
    [HttpGet("additional-questions", Name = RouteNames.AdditionalQuestions_Get)]
    public async Task<IActionResult> AdditionalQuestions(VacancyRouteModel vrm)
    {
        var vm = await orchestrator.GetViewModel(vrm);
        return View(vm);
    }
    
    [HttpPost("additional-questions", Name = RouteNames.AdditionalQuestions_Post)]
    public async Task<IActionResult> AdditionalQuestions(AdditionalQuestionsEditModel m)
    {
        var vm = await orchestrator.GetViewModel(m);
        
        var response = await orchestrator.PostEditModel(m, User.ToVacancyUser());

        if (!response.Success)
        {
            response.AddErrorsToModelState(ModelState);
        }

        if (!ModelState.IsValid)
        {
            vm.AdditionalQuestion1 = m.AdditionalQuestion1;
            vm.AdditionalQuestion2 = m.AdditionalQuestion2;
            return View(vm);
        }
            
        if (!vm.IsTaskListCompleted)
        {
            return RedirectToRoute(RouteNames.ProviderTaskListGet, new {m.VacancyId, m.Ukprn});
        }
        return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {m.VacancyId, m.Ukprn});
    }
}