using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountApplicationReviewsRoutePath)]
    public class ApplicationReviewsController : Controller
    {
        private readonly IApplicationReviewsOrchestrator _orchestrator;

        public ApplicationReviewsController(IApplicationReviewsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        [HttpGet("unsuccessful", Name = RouteNames.ApplicationReviewsToUnsuccessful_Get)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessful(VacancyRouteModel rm)
        {
            var viewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulViewModelAsync(rm);

            return View(viewModel);
        }

        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        [HttpPost("unsuccessful", Name = RouteNames.ApplicationReviewsToUnsuccessful_Post)]
        public IActionResult ApplicationReviewsToShare(ApplicationReviewsToUnsuccessfulRouteModel rm)
        {
            return RedirectToAction(nameof(ApplicationReviewsToUnsuccessfulConfirmation), new { rm.ApplicationsToUnsuccessful, rm.EmployerAccountId, rm.VacancyId });
        }


        [FeatureGate(FeatureNames.MultipleApplicationsManagement)]
        [HttpGet("unsuccessful-confirmation", Name = RouteNames.ApplicationReviewsToUnsuccessfulConfirmation_Get)]
        public async Task<IActionResult> ApplicationReviewsToUnsuccessfulConfirmation(ApplicationReviewsToUnsuccessfulConfirmationRouteModel rm)
        {
            var viewModel = await _orchestrator.GetApplicationReviewsToUnsuccessfulConfirmationViewModelAsync(rm);

            return View(viewModel);
        }
    }
}
