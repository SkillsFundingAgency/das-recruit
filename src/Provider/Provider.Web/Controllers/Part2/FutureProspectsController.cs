﻿using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.FutureProspects;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class FutureProspectsController : Controller
    {
        private readonly FutureProspectsOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public FutureProspectsController(FutureProspectsOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }

        [HttpGet("future-prospects", Name = RouteNames.FutureProspects_Get)]
        public async Task<IActionResult> FutureProspects(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetFutureProspectsViewModelAsync(vrm);
            return View(vm);
        }

        [HttpPost("future-prospects", Name = RouteNames.FutureProspects_Post)]
        public async Task<IActionResult> FutureProspects(FutureProspectsEditModel m)
        {
            var response = await _orchestrator.PostFutureProspectsEditModelAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetFutureProspectsViewModelAsync(m);
                return View(vm);
            }

            return RedirectToRoute(RouteNames.Considerations_Get);
        }
    }
}
