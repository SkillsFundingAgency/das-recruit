using System.Diagnostics;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [AllowAnonymous]    
    public class ErrorController : Controller
    {
        public ErrorController()
        {
        }

        [Route("Error/{id?}")]
        public IActionResult Error(int id)
        {
            return View(new ErrorViewModel { StatusCode = id, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
