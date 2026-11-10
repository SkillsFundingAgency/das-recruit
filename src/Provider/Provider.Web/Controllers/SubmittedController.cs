using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Esfa.Recruit.Provider.Web.Controllers;

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