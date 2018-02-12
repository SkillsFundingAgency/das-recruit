using System.Diagnostics;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ErrorController()
        {
        }

        public IActionResult Error(int id)
        {
            return View(new ErrorViewModel { StatusCode = id, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
