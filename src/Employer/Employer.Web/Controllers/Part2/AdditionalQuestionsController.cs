using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Interfaces;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.AdditionalQuestions;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2;

[Route(RoutePaths.AccountVacancyRoutePath)]
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
            vm.AdditionalQuestion1 = m.AdditionalQuestion1;
            vm.AdditionalQuestion2 = m.AdditionalQuestion2;
            return View(vm);
        }
            
        if (!vm.IsTaskListCompleted)
        {
            return RedirectToRoute(RouteNames.EmployerTaskListGet, new {m.VacancyId, m.EmployerAccountId});
        }
        return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {m.VacancyId, m.EmployerAccountId});
    }
}