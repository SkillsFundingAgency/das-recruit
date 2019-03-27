using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Employer.Web.Views;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using System.Linq;
using Esfa.Recruit.Employer.Web.Extensions;
using Microsoft.AspNetCore.Hosting;
using Esfa.Recruit.Employer.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class EmployerController : EmployerControllerBase
    {
        private readonly EmployerOrchestrator _orchestrator;
        

        public EmployerController(EmployerOrchestrator orchestrator, IHostingEnvironment hostingEnvironment)
            : base(hostingEnvironment)
        {
            _orchestrator = orchestrator;            
        }

        [HttpGet("employer", Name = RouteNames.Employer_Get)]
        public async Task<IActionResult> Employer(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var info = GetVacancyEmployerInfoCookie(vrm.VacancyId);

            var vm = await _orchestrator.GetEmployerViewModelAsync(vrm);

            if (info == null || !info.LegalEntityId.HasValue)
            {
                SetVacancyEmployerInfoCookie(vm.VacancyEmployerInfoModel);
            }
            else
            {
                vm.SelectedOrganisationId = info.LegalEntityId;
            }

            if(vm.HasOnlyOneOrganisation)
            {
                return RedirectToRoute(RouteNames.EmployerName_Get);
            }

            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }
        
        [HttpPost("employer", Name = RouteNames.Employer_Post)]
        public async Task<IActionResult> Employer(EmployerEditModel m, [FromQuery] bool wizard)
        {
            var info = GetVacancyEmployerInfoCookie(m.VacancyId);
            if(info == null)
            {
                //something went wrong, the matching cookie was not found
                //Redirect the user with validation error to allow them to continue
                ModelState.AddModelError("SelectedOrganisationId", ValidationMessages.EmployerNameValidationMessages.EmployerNameRequired);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetEmployerViewModelAsync(m);
                SetVacancyEmployerInfoCookie(vm.VacancyEmployerInfoModel);
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            if(info.LegalEntityId != m.SelectedOrganisationId)
            {
                info.LegalEntityId = m.SelectedOrganisationId;
                info.HasLegalEntityChanged = true;
                info.EmployerNameOption = null;
                info.NewTradingName = null;
            }

            SetVacancyEmployerInfoCookie(info);

            return RedirectToRoute(RouteNames.EmployerName_Get);
        }

        [HttpGet("employer-cancel", Name = RouteNames.Employer_Cancel)]
        public IActionResult Cancel(VacancyRouteModel vrm, [FromQuery] bool wizard)
        {
            DeleteVacancyEmployerInfoCookie();
            return wizard 
                ? RedirectToRoute(RouteNames.Vacancy_Preview_Get, Anchors.AboutEmployerSection) 
                : RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }
    }
}