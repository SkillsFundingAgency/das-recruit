using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Employer.Domain.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private ExternalLinksConfiguration _externalLinks;

        public HomeController(IOptions<ExternalLinksConfiguration> externalLinksOptions)
        {
            _externalLinks = externalLinksOptions.Value;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");

            return Redirect(_externalLinks.ManageApprenticeshipSiteUrl);
        }
    }
}
