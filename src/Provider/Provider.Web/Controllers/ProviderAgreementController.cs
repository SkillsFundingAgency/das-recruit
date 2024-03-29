﻿using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class ProviderAgreementController : Controller
    {
        private readonly ProviderAgreementOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public ProviderAgreementController(ProviderAgreementOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }

        [HttpGet("provider-agreement", Name = RouteNames.ProviderAgreement_HardStop_Get)]
        public async Task<IActionResult> ProviderAgreementHardStop(VacancyRouteModel vrm)
        {
            var hasAgreement = await _orchestrator.HasAgreementAsync(vrm.Ukprn);

            if (hasAgreement)
            {
                if (_feature.IsFeatureEnabled(FeatureNames.ProviderTaskList))
                {
                    return RedirectToRoute(RouteNames.Vacancy_Advert_Preview_Get, new {vrm.Ukprn, vrm.VacancyId});
                }
                
                return RedirectToRoute(RouteNames.Vacancy_Preview_Get, new {vrm.Ukprn, vrm.VacancyId});
            }
                

            return View(vrm);
        }
    }
}
