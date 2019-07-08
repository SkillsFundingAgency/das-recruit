﻿using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Employer(VacancyRouteModel vrm, [FromQuery]string searchTerm, [FromQuery]int? page, [FromQuery] string wizard = "true")
        {
            var info = GetVacancyEmployerInfoCookie(vrm.VacancyId);

            var vm = await _orchestrator.GetEmployerViewModelAsync(vrm, searchTerm, page, info?.LegalEntityId);

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
                var vm = await _orchestrator.GetEmployerViewModelAsync(m, m.SearchTerm, m.Page, info?.LegalEntityId);
                SetVacancyEmployerInfoCookie(vm.VacancyEmployerInfoModel);
                vm.Pager.OtherRouteValues.Add(nameof(wizard), wizard.ToString());
                vm.PageInfo.SetWizard(wizard);
                return View(vm);
            }

            if (info.LegalEntityId != m.SelectedOrganisationId)
            {
                info.LegalEntityId = m.SelectedOrganisationId;
                info.HasLegalEntityChanged = true;
                info.EmployerIdentityOption = null;
                info.NewTradingName = null;
                info.AnonymousName = null;
                info.AnonymousReason = null;
            }

            SetVacancyEmployerInfoCookie(info);

            return RedirectToRoute(RouteNames.EmployerName_Get, new {Wizard = wizard});
        }

        [HttpGet("employer-cancel", Name = RouteNames.Employer_Cancel)]
        public IActionResult Cancel(VacancyRouteModel vrm, [FromQuery] bool wizard)
        {
            return CancelAndRedirect(wizard);
        }
    }
}