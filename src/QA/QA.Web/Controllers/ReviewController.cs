using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.Controllers
{
    [Route(RoutePrefixPaths.VacancyRoutePath)]
    public class ReviewController : Controller
    {
        [HttpGet("review", Name = RouteNames.Vacancy_Review_Get)]
        public IActionResult Review([FromRoute] string vacancyReference) => View((object)vacancyReference);
    }
}
