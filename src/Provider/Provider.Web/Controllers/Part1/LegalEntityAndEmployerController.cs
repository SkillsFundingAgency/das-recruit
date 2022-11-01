using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class LegalEntityAndEmployerController : EmployerControllerBase
    {
        private readonly LegalEntityAndEmployerOrchestrator _orchestrator;
        private readonly ServiceParameters _serviceParameters;

        public LegalEntityAndEmployerController(
            LegalEntityAndEmployerOrchestrator orchestrator, 
            IWebHostEnvironment hostingEnvironment,
            ServiceParameters serviceParameters)
            : base(hostingEnvironment)
        {
            _orchestrator = orchestrator;
            _serviceParameters = serviceParameters;
        }

        [HttpGet("employer-legal-entity", Name = RouteNames.LegalEntityEmployer_Get)]
        public async Task<IActionResult> LegalEntityAndEmployer(VacancyRouteModel vrm, [FromQuery]string searchTerm, [FromQuery]int? page, [FromQuery] string wizard = "true")
        {
            var vm = await _orchestrator.GetLegalEntityAndEmployerViewModelAsync(vrm, searchTerm, page);

            if (vm.HasOnlyOneOrganisation)
            {
                var model = new LegalEntityAndEmployerEditModel();
                {
                    model.SelectedOrganisationId = vm.Organisations.FirstOrDefault()?.Id;
                }
                
                //TODO not the correct flow
                return RedirectToRoute(_serviceParameters.VacancyType == VacancyType.Traineeship 
                    ? RouteNames.TraineeSector_Get 
                    : RouteNames.Training_Get, new {Wizard = wizard, vrm.Ukprn, vrm.VacancyId});
            }

            return View(vm);
        }

        [HttpPost("employer-legal-entity", Name = RouteNames.LegalEntityEmployer_Post)]
        public async Task<IActionResult> LegalEntityAndEmployer(LegalEntityAndEmployerEditModel m, VacancyRouteModel vacancyRouteModel)
        {
            if (string.IsNullOrWhiteSpace(m.SelectedOrganisationId))
            {
                ModelState.AddModelError(nameof(m.SelectedOrganisationId), ValidationMessages.EmployerSelectionMessages.EmployerMustBeSelectedMessage);
            }

            var vm = await _orchestrator.GetLegalEntityAndEmployerViewModelAsync(m, m.SearchTerm, m.Page);
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            return RedirectToRoute(RouteNames.ConfirmLegalEntityEmployer_Get, new { selectedId = m.SelectedOrganisationId, vacancyRouteModel.Ukprn});
        }

        [HttpGet("confirm-employer-legal-entity", Name = RouteNames.ConfirmLegalEntityEmployer_Get)]
        public async Task<IActionResult> ConfirmEmployerLegalEntitySelection(VacancyRouteModel vacancyRouteModel,[FromQuery] string selectedId)
        {
            var employerAccountLegalEntityId = selectedId.Split('|')[0];
            var employerAccountId = selectedId.Split('|')[1];
            var viewModel = await _orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, employerAccountId, employerAccountLegalEntityId);
            
            return View(viewModel);
        }

        [HttpPost("confirm-employer-legal-entity", Name = RouteNames.ConfirmLegalEntityEmployer_Post)]
        public async Task<IActionResult> ConfirmEmployerLegalEntitySelection(ConfirmLegalEntityAndEmployerEditModel model, VacancyRouteModel vacancyRouteModel)
        {

            if (!ModelState.IsValid)
            {
                return View(new ConfirmLegalEntityAndEmployerViewModel
                {
                    EmployerName = model.EmployerName,
                    EmployerAccountId = model.EmployerAccountId,
                    AccountLegalEntityName = model.AccountLegalEntityName,
                    AccountLegalEntityPublicHashedId = model.AccountLegalEntityPublicHashedId,
                    Ukprn = vacancyRouteModel.Ukprn
                    
                });
            }

            if (model.HasConfirmedEmployer.HasValue && !model.HasConfirmedEmployer.Value)
            {
                return RedirectToRoute(RouteNames.LegalEntityEmployer_Get, new {ukprn = vacancyRouteModel.Ukprn});
            }

            return RedirectToRoute(RouteNames.ProviderTaskListCreateGet,
                    new
                            {
                                employerAccountId = model.EmployerAccountId,
                                accountLegalEntityPublicHashedId = model.AccountLegalEntityPublicHashedId, 
                                vacancyRouteModel.Ukprn
                            });
        }
    }
}