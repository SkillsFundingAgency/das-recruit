using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.LevyDeclaration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
            _dataProtector = dataProtectionProvider.CreateProtector("levy-declaration");
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
            {
                return View(viewModel);
            }

            if (viewModel.ConfirmAsLevyPayer.Value)
            {
                var userId = User.GetUserId();
                var protectedUserId = _dataProtector.Protect(userId);

                await _orchestrator.SaveSelection(viewModel, userId);

                Response.Cookies.Append(CookieNames.LevyEmployerIndicator, protectedUserId, EsfaCookieOptions.GetDefaultHttpCookieOption(_hostingEnvironment));
            
                return RedirectToRoute(RouteNames.Dashboard_Index_Get);
            }
                
            return RedirectToRoute(RouteNames.NonLevyInfo_Get);
        }

        [HttpGet("non-levy-info", Name = RouteNames.NonLevyInfo_Get)]
        public IActionResult NonLevyInfo()
        {
            return View();
        }
    }
}