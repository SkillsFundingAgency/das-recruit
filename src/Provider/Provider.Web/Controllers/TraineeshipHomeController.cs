using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    public class TraineeshipHomeController: Controller
    {
        [HttpGet("traineeship-help", Name = RouteNames.TraineeshipHelp)]
        public IActionResult Help()
        {
            return View();
        }
        
        [HttpGet("traineeship-privacy", Name = RouteNames.TraineeshipPrivacy)]
        public IActionResult Privacy()
        {
            return View();
        }
    }
}