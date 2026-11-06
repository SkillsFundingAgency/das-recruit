using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class VacancyViewController(VacancyViewOrchestrator orchestrator, IUtility utility) : Controller
{
    [HttpGet("", Name = RouteNames.DisplayVacancy_Get)]
    public async Task<IActionResult> DisplayVacancy(VacancyRouteModel vrm)
    {
        var vacancy = await orchestrator.GetVacancy(vrm);

        if (vacancy.CanEmployerEdit)
        {
            return HandleRedirectOfEditableVacancy(vacancy);
        }

        var vm = await orchestrator.GetFullVacancyDisplayViewModelAsync(vacancy);
        return View(ViewNames.FullVacancyView, vm);
    }

    [HttpGet("view", Name = RouteNames.DisplayFullVacancy_Get)]
    public async Task<IActionResult> DisplayFullVacancy(VacancyRouteModel vrm)
    {
        var vacancy = await orchestrator.GetVacancy(vrm);

        if (vacancy.CanEmployerEdit)
        {
            return HandleRedirectOfEditableVacancy(vacancy);
        }

        var vm = await orchestrator.GetFullVacancyDisplayViewModelAsync(vacancy);
        return View(ViewNames.FullVacancyView, vm);
    }

    private IActionResult HandleRedirectOfEditableVacancy(Vacancy vacancy)
    {
        if (utility.IsTaskListCompleted(vacancy))
        {
            return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {vacancyId = vacancy.Id, vacancy.EmployerAccountId});
        }
        return RedirectToRoute(RouteNames.EmployerTaskListGet, new {vacancyId = vacancy.Id, vacancy.EmployerAccountId});
    }
}