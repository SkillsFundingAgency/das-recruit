using System.Diagnostics;
using System.Net;
using Esfa.Recruit.Qa.Web.Configuration;
using Esfa.Recruit.Qa.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        [Route("error/{id?}")]
        public IActionResult Error(int id)
        {
            switch (id)
            {
                case 403:
                    return AccessDenied();
            }

            return View(new ErrorViewModel { StatusCode = id, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("error/handle")]
        public IActionResult ErrorHandler()
        {
            return View(ViewNames.ErrorView, new ErrorViewModel { StatusCode = (int)HttpStatusCode.InternalServerError, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private IActionResult AccessDenied()
        {
            return View(ViewNames.AccessDenied);
        }
    }
}