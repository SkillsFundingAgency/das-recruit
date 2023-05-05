using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.Qa.Web.Controllers
{
    [Route("")]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            bool isDfESignInAllowed = _configuration.GetValue<bool>("UseDfeSignIn");

            if(!isDfESignInAllowed) return RedirectToAction("Index", "Dashboard");

            return View();
        }
    }
}
