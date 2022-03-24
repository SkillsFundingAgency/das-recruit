using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;

namespace  Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class Part1CompleteController : Controller
    {
        [HttpGet("part1-complete", Name = RouteNames.Part1Complete_Get)]
        public ActionResult Part1Complete(VacancyRouteModel vrm)
        {
            return View(vrm);
        }
    }
}