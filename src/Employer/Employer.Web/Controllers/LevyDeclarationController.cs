using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.LevyDeclaration;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class LevyDeclarationController : Controller
    {
        private readonly LevyDeclarationOrchestrator _orchestrator;
        private readonly ILevyDeclarationCookieWriter _levyCookieWriter;

        public LevyDeclarationController(
            LevyDeclarationOrchestrator orchestrator,
            ILevyDeclarationCookieWriter levyCookieWriter)
        {
            _orchestrator = orchestrator;
            _levyCookieWriter = levyCookieWriter;
        }

        [HttpGet("levy-declaration", Name = RouteNames.LevyDeclaration_Get)]
        public IActionResult Options()
        {
            return View(new LevyDeclarationModel());
        }

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

        [HttpGet("non-levy-info", Name = RouteNames.NonLevyInfo_Get)]
        public IActionResult NonLevyInfo()
        {
            return View();
        }

        private void SetLevyDeclarationCookie(ClaimsPrincipal user, string employerAccountId)
        {
            _levyCookieWriter.WriteCookie(Response, user.GetUserId(), employerAccountId, hasLevyDeclaration: true);
        }
    }
}