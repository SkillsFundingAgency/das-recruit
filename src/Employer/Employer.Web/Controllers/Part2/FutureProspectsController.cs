using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Interfaces;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.FutureProspects;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class FutureProspectsController(IFutureProspectsOrchestrator orchestrator) : Controller
{
    [HttpGet("future-prospects", Name = RouteNames.FutureProspects_Get)]
    public async Task<IActionResult> FutureProspects(VacancyRouteModel vrm)
    {
        var vm = await orchestrator.GetViewModel(vrm);
        return View(vm);
    }

    [HttpPost("future-prospects", Name = RouteNames.FutureProspects_Post)]
    public async Task<IActionResult> FutureProspects(FutureProspectsEditModel m)
    {
        var response = await orchestrator.PostEditModel(m, User.ToVacancyUser());

        if (!response.Success)
        {
            response.AddErrorsToModelState(ModelState);
        }
            
        var vm = await orchestrator.GetViewModel(m);
        if (!ModelState.IsValid)
        {
            return View(vm);
        }
            
        if (!vm.IsTaskListCompleted)
        {
            return RedirectToRoute(RouteNames.Considerations_Get, new {m.VacancyId, m.EmployerAccountId});
        }
        return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {m.VacancyId, m.EmployerAccountId});
    }
}