using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntity;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class LegalEntityController : EmployerControllerBase
    {
        private readonly LegalEntityOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public LegalEntityController(LegalEntityOrchestrator orchestrator, IHostingEnvironment hostingEnvironment, IFeature feature)
            : base(hostingEnvironment)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }

        [HttpGet("legal-entity", Name = RouteNames.LegalEntity_Get)]
        public async Task<IActionResult> LegalEntity(VacancyRouteModel vrm, [FromQuery]string searchTerm, [FromQuery]int? page, [FromQuery] string wizard = "true")
        {
            var info = GetVacancyEmployerInfoCookie(vrm.VacancyId.GetValueOrDefault());

            var vm = await _orchestrator.GetLegalEntityViewModelAsync(vrm, User.GetUkprn(), searchTerm, page, info?.AccountLegalEntityPublicHashedId);

            if (info == null || !string.IsNullOrEmpty(info.AccountLegalEntityPublicHashedId))
            {
                SetVacancyEmployerInfoCookie(vm.VacancyEmployerInfoModel);
            }
            else
            {
                vm.SelectedOrganisationId = info.AccountLegalEntityPublicHashedId;
            }

            if (vm.HasOnlyOneOrganisation)
            {
                return RedirectToRoute(RouteNames.EmployerName_Get, new {Wizard = wizard});
            }

            vm.Pager.OtherRouteValues.Add(nameof(wizard), wizard);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("legal-entity", Name = RouteNames.LegalEntity_Post)]
        public async Task<IActionResult> LegalEntity(LegalEntityEditModel m, [FromQuery] bool wizard)
        {
            var info = GetVacancyEmployerInfoCookie(m.VacancyId.GetValueOrDefault());
            if (info == null)
            {
                //something went wrong, the matching cookie was not found
                //Redirect the user with validation error to allow them to continue
                ModelState.AddModelError(nameof(LegalEntityEditModel.SelectedOrganisationId),
                    ValidationMessages.EmployerSelectionValidationMessages.EmployerSelectionRequired);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetLegalEntityViewModelAsync(m, User.GetUkprn(), m.SearchTerm, m.Page, info.AccountLegalEntityPublicHashedId);
                SetVacancyEmployerInfoCookie(vm.VacancyEmployerInfoModel);
                vm.Pager.OtherRouteValues.Add(nameof(wizard), wizard.ToString());
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            if (info.AccountLegalEntityPublicHashedId != m.SelectedOrganisationId)
            {
                info.AccountLegalEntityPublicHashedId = m.SelectedOrganisationId;
                info.HasLegalEntityChanged = true;
                info.EmployerIdentityOption = null;
                info.NewTradingName = null;
            }

            SetVacancyEmployerInfoCookie(info);
            await _orchestrator.SetAccountLegalEntityPublicId(new VacancyRouteModel
            {
                Ukprn = m.Ukprn,
                VacancyId = m.VacancyId
            }, m, HttpContext.User.ToVacancyUser());

            if (_feature.IsFeatureEnabled(FeatureNames.ProviderTaskList))
            {
                return RedirectToRoute(RouteNames.Training_Get, new {Wizard = wizard});    
            }
            
            return RedirectToRoute(RouteNames.EmployerName_Get, new {Wizard = wizard});
        }

        [HttpGet("legal-entity-cancel", Name = RouteNames.LegalEntity_Cancel)]
        public IActionResult Cancel(VacancyRouteModel vrm, [FromQuery] bool wizard)
        {
            return CancelAndRedirect(wizard);
        }
    }
}