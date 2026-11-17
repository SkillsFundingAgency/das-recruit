using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class SubmittedController(SubmittedOrchestrator orchestrator) : Controller
{
    [HttpGet("submitted", Name = RouteNames.Submitted_Index_Get)]
    public async Task<IActionResult> Confirmation(VacancyRouteModel vrm)
    {
        var vm = await orchestrator.GetVacancySubmittedConfirmationViewModelAsync(vrm, User.ToVacancyUser());
        return View(vm);
    }
}