using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.AdditionalQuestions;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part2;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class AdditionalQuestionsController : Controller
{
    public AdditionalQuestionsController()
    {
        
    }

    [HttpGet("additional-questions", Name = RouteNames.AdditionalQuestions_Get)]
    public IActionResult AdditionalQuestions(VacancyRouteModel vrm)
    {
        return View(new AdditionalQuestionsViewModel());
    }
    
    [HttpPost("additional-questions", Name = RouteNames.AdditionalQuestions_Post)]
    public IActionResult AdditionalQuestions(AdditionalQuestionsEditModel m)
    {
        return View(new AdditionalQuestionsViewModel());
    }
}