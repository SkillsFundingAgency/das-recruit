using Esfa.Recruit.Employer.Web.Attributes;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.LevyDeclaration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountRoutePath)]
    public class LevyDeclarationController : Controller
    {
        private readonly LevyDeclarationOrchestrator _orchestrator;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IDataProtector _dataProtector;

        public LevyDeclarationController(
            LevyDeclarationOrchestrator orchestrator,
            IHostingEnvironment hostingEnvironment,
            IDataProtectionProvider dataProtectionProvider)
        {
            _orchestrator = orchestrator;
            _hostingEnvironment = hostingEnvironment;
            _dataProtector = dataProtectionProvider.CreateProtector(DataProtectionPurposes.LevyDeclarationCookie);
        }

        [CheckEmployerBlocked]
        [HttpGet("levy-declaration", Name = RouteNames.LevyDeclaration_Get)]
        public IActionResult Options()
        {
            return View(new LevyDeclarationModel());
        }

        [CheckEmployerBlocked]
        [HttpPost("levy-declaration", Name = RouteNames.LevyDeclaration_Post)]
        public async Task<IActionResult> Options(string employerAccountId, LevyDeclarationModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var response = await _orchestrator.SaveSelectionAsync(viewModel, User);

            if (response.CreateLevyCookie)
                SetLevyDeclarationCookie(User);

            return RedirectToRoute(response.RedirectRouteName);
        }

        [CheckEmployerBlocked]
        [HttpGet("non-levy-info", Name = RouteNames.NonLevyInfo_Get)]
        public IActionResult NonLevyInfo()
        {
            return View();
        }

        private void SetLevyDeclarationCookie(ClaimsPrincipal user)
        {
            var protectedUserId = _dataProtector.Protect(user.GetUserId());

            Response.Cookies.Append(CookieNames.LevyEmployerIndicator, protectedUserId, EsfaCookieOptions.GetDefaultHttpCookieOption(_hostingEnvironment));
        }
    }
}