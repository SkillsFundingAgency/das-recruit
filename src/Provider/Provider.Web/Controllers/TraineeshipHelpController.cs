using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    public class TraineeshipHelpController: Controller
    {
        [HttpGet("traineeship-help", Name = RouteNames.TraineeshipHelp)]
        public IActionResult Index()
        {
            return View();
        }
    }
}