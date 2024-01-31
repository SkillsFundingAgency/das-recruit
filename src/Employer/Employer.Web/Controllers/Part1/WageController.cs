using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Microsoft.FeatureManagement.Mvc;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
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

            switch (m.WageType)
            {
                case WageType.FixedWage:
                    return RedirectToRoute(RouteNames.CustomWage_Get, new { m.VacancyId, m.EmployerAccountId, wizard });
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
                    return RedirectToRoute(RouteNames.AddExtraInformation_Get, new { m.VacancyId, m.EmployerAccountId, wizard });

                case WageType.CompetitiveSalary:
                    return RedirectToRoute(RouteNames.SetCompetitivePayRate_Get, new { m.VacancyId, m.EmployerAccountId, wizard });
                default:
                    return HandleDefaultView(vm, wizard, m.WageType);
            }
        }

        [FeatureGate(FeatureNames.CompetitiveSalary)]
        [HttpGet("competitive-wage", Name = RouteNames.SetCompetitivePayRate_Get)]
        public async Task<IActionResult> CompetitiveSalary(VacancyRouteModel vrm)
        {
            var vm = await _orchestrator.GetCompetitiveWageViewModelAsync(vrm);
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

                return RedirectToRoute(RouteNames.AddExtraInformation_Get, new { m.VacancyId, m.EmployerAccountId, wizard });
            }

            return RedirectToRoute(RouteNames.Wage_Get, new { m.VacancyId, m.EmployerAccountId, wizard });
        }

        [HttpGet("extra-information-wage", Name = RouteNames.AddExtraInformation_Get)]
        public async Task<IActionResult> AdditionalInformation(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetExtraInformationViewModelAsync(vrm);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("extra-information-wage", Name = RouteNames.AddExtraInformation_Post)]
        public async Task<IActionResult> AdditionalInformation(WageExtraInformationViewModel vrm, [FromQuery] bool wizard)
        {
            if (!ModelState.IsValid)
            {
                return await HandleAdditionalInformationDefaultView(vrm, wizard);
            }

            var response = await _orchestrator.PostExtraInformationEditModelAsync(vrm, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                return await HandleAdditionalInformationDefaultView(vrm, wizard);
            }

            if (wizard)

            {
                return RedirectToRoute(RouteNames.NumberOfPositions_Get, new { vrm.VacancyId, vrm.EmployerAccountId, wizard });
            }
            return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new { vrm.VacancyId, vrm.EmployerAccountId });
        }

        async Task<IActionResult> HandleAdditionalInformationDefaultView(WageExtraInformationViewModel vrm, bool wizard)
        {
            var vm = await _orchestrator.GetExtraInformationViewModelAsync(vrm);
            vm.WageAdditionalInformation = vrm.WageAdditionalInformation;
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        IActionResult HandleDefaultView(WageViewModel vm, bool wizard, WageType? wageType)
        {
            vm.WageType = wageType;
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }
    }
}