using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class EmployerController : EmployerControllerBase
    {
        private readonly EmployerOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public EmployerController(EmployerOrchestrator orchestrator, IWebHostEnvironment hostingEnvironment, IFeature feature)
            : base(hostingEnvironment)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        }

        [HttpGet("employer", Name = RouteNames.Employer_Get)]
        public async Task<IActionResult> Employer(VacancyRouteModel vrm, [FromQuery]string searchTerm, [FromQuery]int? page, [FromQuery] string wizard = "true")
        {
            var info = GetVacancyEmployerInfoCookie(vrm.VacancyId);

            var vm = await _orchestrator.GetEmployerViewModelAsync(vrm, searchTerm, page, info?.AccountLegalEntityPublicHashedId);

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
                if (_feature.IsFeatureEnabled(FeatureNames.EmployerTaskList))
                {
                    info = vm.VacancyEmployerInfoModel;
                    await _orchestrator.SetAccountLegalEntityPublicId(vrm,info, User.ToVacancyUser());
                    
                    return  RedirectToRoute(RouteNames.Training_Get, new { Wizard = wizard, vrm.VacancyId, vrm.EmployerAccountId  });
                }
                
                return RedirectToRoute(RouteNames.EmployerName_Get, new {Wizard = wizard, vrm.VacancyId, vrm.EmployerAccountId});
            }

            vm.Pager.OtherRouteValues.Add(nameof(wizard), wizard);
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("employer", Name = RouteNames.Employer_Post)]
        public async Task<IActionResult> Employer(EmployerEditModel m, [FromQuery] bool wizard)
        {
            var info = GetVacancyEmployerInfoCookie(m.VacancyId);
            if (info == null)
            {
                //something went wrong, the matching cookie was not found
                //Redirect the user with validation error to allow them to continue
                ModelState.AddModelError(nameof(EmployerEditModel.SelectedOrganisationId),
                    ValidationMessages.EmployerSelectionValidationMessages.EmployerSelectionRequired);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetEmployerViewModelAsync(m, m.SearchTerm, m.Page, info?.AccountLegalEntityPublicHashedId);
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
                info.AnonymousName = null;
                info.AnonymousReason = null;
            }

            SetVacancyEmployerInfoCookie(info);
            await _orchestrator.SetAccountLegalEntityPublicId(m,info, User.ToVacancyUser());
            
            return wizard 
                ? RedirectToRoute(RouteNames.Training_Get, new { wizard, m.VacancyId, m.EmployerAccountId }) 
                : RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new { m.VacancyId, m.EmployerAccountId });
        }

        [HttpGet("employer-cancel", Name = RouteNames.Employer_Cancel)]
        public IActionResult Cancel(VacancyRouteModel vrm, [FromQuery] bool wizard)
        {
            return CancelAndRedirect(wizard, vrm);
        }
    }
}