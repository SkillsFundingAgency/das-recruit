using System.Linq;
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
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class LegalEntityController : EmployerControllerBase
    {
        private readonly LegalEntityOrchestrator _orchestrator;

        public LegalEntityController(
            LegalEntityOrchestrator orchestrator, 
            IWebHostEnvironment hostingEnvironment)
            : base(hostingEnvironment)
        {
            _orchestrator = orchestrator;
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
                if (!string.IsNullOrEmpty(info.AccountLegalEntityPublicHashedId))
                {
                    vm.SelectedOrganisationId = info.AccountLegalEntityPublicHashedId;    
                }
            }

            if (vm.HasOnlyOneOrganisation)
            {
                var model = new LegalEntityEditModel
                {
                    SelectedOrganisationId = vm.Organisations.FirstOrDefault()?.Id
                };
                await _orchestrator.SetAccountLegalEntityPublicId(vrm,model, User.ToVacancyUser());

                return RedirectToRoute(RouteNames.Training_Get, new {Wizard = wizard, vrm.Ukprn, vrm.VacancyId});
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

            var vm = await _orchestrator.GetLegalEntityViewModelAsync(m, User.GetUkprn(), m.SearchTerm, m.Page, info.AccountLegalEntityPublicHashedId);
            if (!ModelState.IsValid)
            {
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

            if (!vm.IsTaskListCompleted)
            {
                return RedirectToRoute(RouteNames.Training_Get, new { Wizard = wizard, m.Ukprn, m.VacancyId });
            }
            return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {m.Ukprn, m.VacancyId});
        }

        [HttpGet("legal-entity-cancel", Name = RouteNames.LegalEntity_Cancel)]
        public IActionResult Cancel(VacancyRouteModel vrm, [FromQuery] bool wizard)
        {
            return CancelAndRedirect(wizard, vrm);
        }
    }
}