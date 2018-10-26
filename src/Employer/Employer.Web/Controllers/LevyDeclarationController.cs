using Esfa.Recruit.Employer.Web.Attributes;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.LevyDeclaration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountRoutePath)]
    public class LevyDeclarationController : Controller
    {
        private readonly LevyDeclarationOrchestrator _orchestrator;
        private readonly IDataProtector _dataProtector;
        private readonly LevyDeclarationCookieWriter _levyCookieWriter;

        public LevyDeclarationController(
            LevyDeclarationOrchestrator orchestrator,
            IDataProtectionProvider dataProtectionProvider,
            LevyDeclarationCookieWriter levyCookieWriter)
        {
            _orchestrator = orchestrator;
            _dataProtector = dataProtectionProvider.CreateProtector(DataProtectionPurposes.LevyDeclarationCookie);
            _levyCookieWriter = levyCookieWriter;
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
                SetLevyDeclarationCookie(User, employerAccountId);

            return RedirectToRoute(response.RedirectRouteName);
        }

        [CheckEmployerBlocked]
        [HttpGet("non-levy-info", Name = RouteNames.NonLevyInfo_Get)]
        public IActionResult NonLevyInfo()
        {
            return View();
        }

        private void SetLevyDeclarationCookie(ClaimsPrincipal user, string employerAccountId)
        {
            var protectedUserId = _dataProtector.Protect(user.GetUserId());

            _levyCookieWriter.WriteCookie(Response, protectedUserId, employerAccountId);
        }
    }
}