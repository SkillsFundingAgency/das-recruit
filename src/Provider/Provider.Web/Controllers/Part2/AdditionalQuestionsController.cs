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
public class AdditionalQuestionsController : Controller
{
    private readonly IAdditionalQuestionsOrchestrator _orchestrator;

    public AdditionalQuestionsController(IAdditionalQuestionsOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [HttpGet("additional-questions", Name = RouteNames.AdditionalQuestions_Get)]
    public async Task<IActionResult> AdditionalQuestions(VacancyRouteModel vrm)
    {
        var vm = await _orchestrator.GetViewModel(vrm);
        return View(vm);
    }
    
    [HttpPost("additional-questions", Name = RouteNames.AdditionalQuestions_Post)]
    public async Task<IActionResult> AdditionalQuestions(AdditionalQuestionsEditModel m)
    {
        var vm = await _orchestrator.GetViewModel(m);
        
        var response = await _orchestrator.PostEditModel(m, User.ToVacancyUser());

        if (!response.Success)
        {
            response.AddErrorsToModelState(ModelState);
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }
            
        if (!vm.IsTaskListCompleted)
        {
            return RedirectToRoute(RouteNames.ProviderTaskListGet, new {m.VacancyId, m.Ukprn});
        }
        return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {m.VacancyId, m.Ukprn});
    }
}