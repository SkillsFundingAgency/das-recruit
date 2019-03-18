using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class CreateReportController : Controller
    {
        [HttpGet("create", Name = RouteNames.CreateReport_Get)]
        public IActionResult Create()
        {
            return View();
        }      
    }
}
