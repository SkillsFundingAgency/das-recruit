using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntity;
using Esfa.Recruit.Provider.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class LegalEntityController : EmployerControllerBase
    {
        private readonly LegalEntityOrchestrator _orchestrator;

        public LegalEntityController(LegalEntityOrchestrator orchestrator, IHostingEnvironment hostingEnvironment)
            : base(hostingEnvironment)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("legal-entity", Name = RouteNames.LegalEntity_Get)]
        public async Task<IActionResult> LegalEntity(VacancyRouteModel vrm, [FromQuery]string searchTerm, [FromQuery]int? page, [FromQuery] string wizard = "true")
        {
            var info = GetVacancyEmployerInfoCookie(vrm.VacancyId.GetValueOrDefault());

            var vm = await _orchestrator.GetLegalEntityViewModelAsync(vrm, User.GetUkprn(), searchTerm, page, info?.LegalEntityId);

            if (info == null || !info.LegalEntityId.HasValue)
            {
                SetVacancyEmployerInfoCookie(vm.VacancyEmployerInfoModel);
            }
            else
            {
                vm.SelectedOrganisationId = info.LegalEntityId;
            }

            if (vm.HasOnlyOneOrganisation)
            {
                return RedirectToRoute(RouteNames.EmployerName_Get, new {Wizard = wizard});
            }

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
                    ValidationMessages.EmployerNameValidationMessages.EmployerNameRequired);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetLegalEntityViewModelAsync(m, User.GetUkprn(), m.SearchTerm, m.Page, info.LegalEntityId);
                SetVacancyEmployerInfoCookie(vm.VacancyEmployerInfoModel);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            if (info.LegalEntityId != m.SelectedOrganisationId)
            {
                info.LegalEntityId = m.SelectedOrganisationId;
                info.HasLegalEntityChanged = true;
                info.EmployerIdentityOption = null;
                info.NewTradingName = null;
            }

            SetVacancyEmployerInfoCookie(info);

            return RedirectToRoute(RouteNames.EmployerName_Get, new {Wizard = wizard});
        }

        [HttpGet("legal-entity-cancel", Name = RouteNames.LegalEntity_Cancel)]
        public IActionResult Cancel(VacancyRouteModel vrm, [FromQuery] bool wizard)
        {
            return CancelAndRedirect(wizard);
        }
    }
}