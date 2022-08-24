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
        
        [HttpGet("traineeship-terms-and-conditions", Name = RouteNames.TraineeshipTermsAndConditions)]
        public IActionResult TermsAndConditions()
        {
            return View();
        }
        
        [HttpGet("traineeship-cookies", Name = RouteNames.TraineeshipCookies)]
        public IActionResult Cookies()
        {
            return View();
        }
        
        [HttpGet("traineeship-cookies-details", Name = RouteNames.TraineeshipCookiesDetails)]
        public IActionResult CookiesDetails()
        {
            return View();
        }
    }
}