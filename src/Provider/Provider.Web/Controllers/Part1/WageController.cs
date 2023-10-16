using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    [Authorize(Policy = nameof(PolicyNames.IsApprenticeshipWeb))]
    public class WageController : Controller
    {
        private readonly IWageOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public WageController(IWageOrchestrator orchestrator, IFeature feature)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }

        [HttpGet("wage", Name = RouteNames.Wage_Get)]
        public async Task<IActionResult> Wage(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetWageViewModelAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("wage", Name = RouteNames.Wage_Post)]
        public async Task<IActionResult> Wage(WageEditModel m, [FromQuery] bool wizard)
        {
            var vm = await _orchestrator.GetWageViewModelAsync((VacancyRouteModel)m);

            if (!ModelState.IsValid)
            {
                return HandleDefaultView(vm, wizard, m.WageType);
            }

            if (_feature.IsFeatureEnabled(FeatureNames.ProviderTaskList))
            {
                if (wizard)
                {
                    switch (m.WageType)
                    {
                        case WageType.FixedWage:
                            return RedirectToRoute(RouteNames.CustomWage_Get, new { m.VacancyId, m.Ukprn, wizard });
                        case WageType.NationalMinimumWage or WageType.NationalMinimumWageForApprentices:

                            if (vm.WageType != m.WageType)
                            {
                                var response = await _orchestrator.PostWageEditModelAsync(m, User.ToVacancyUser());

                                if (!response.Success)
                                {
                                    response.AddErrorsToModelState(ModelState);
                                }

                                if (!ModelState.IsValid)
                                {
                                    return HandleDefaultView(vm, wizard, m.WageType);
                                }
                            }
                            return RedirectToRoute(RouteNames.AddExtraInformation_Get, new { m.VacancyId, m.Ukprn, wizard });

                        case WageType.CompetitiveSalary:
                            return RedirectToRoute(RouteNames.SetCompetitivePayRate_Get, new { m.VacancyId, m.Ukprn, wizard });
                        default:
                            return HandleDefaultView(vm, wizard, m.WageType);
                    }
                }
                return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new { m.VacancyId, m.Ukprn });
            }

            return wizard
                ? RedirectToRoute(RouteNames.Part1Complete_Get, new { m.VacancyId, m.Ukprn })
                : _feature.IsFeatureEnabled(FeatureNames.ProviderTaskList)
                    ? RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new { m.VacancyId, m.Ukprn })
                    : RedirectToRoute(RouteNames.Vacancy_Preview_Get, new { m.VacancyId, m.Ukprn });
        }

        [FeatureGate(FeatureNames.CompetitiveSalary)]
        [HttpGet("competitive-wage", Name = RouteNames.SetCompetitivePayRate_Get)]
        public async Task<IActionResult> CompetitiveSalary(VacancyRouteModel vrm, [FromQuery] bool wizard)
        {
            var vm = await _orchestrator.GetCompetitiveWageViewModelAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [FeatureGate(FeatureNames.CompetitiveSalary)]
        [HttpPost("competitive-wage", Name = RouteNames.SetCompetitivePayRate_Post)]
        public async Task<IActionResult> CompetitiveSalary(CompetitiveWageEditModel m, [FromQuery] bool wizard)
        {
            var vm = await _orchestrator.GetCompetitiveWageViewModelAsync(m);

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (m.IsSalaryAboveNationalMinimumWage.HasValue && m.IsSalaryAboveNationalMinimumWage.Value)
            {
                m.WageType = WageType.CompetitiveSalary;

                if (vm.WageType != m.WageType)
                {
                    var response = await _orchestrator.PostCompetitiveWageEditModelAsync(m, User.ToVacancyUser());

                    if (!response.Success)
                    {
                        response.AddErrorsToModelState(ModelState);
                    }
                }

                return RedirectToRoute(RouteNames.AddExtraInformation_Get, new { m.VacancyId, m.Ukprn, wizard });
            }

            return RedirectToRoute(RouteNames.Wage_Get, new { m.VacancyId, m.Ukprn, wizard });
        }

        [HttpGet("extra-information-wage", Name = RouteNames.AddExtraInformation_Get)]
        public async Task<IActionResult> AdditionalInformation(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            return View();
        }

        IActionResult HandleDefaultView(WageViewModel vm, bool wizard, WageType? wageType)
        {
            vm.WageType = wageType;
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }
    }
}