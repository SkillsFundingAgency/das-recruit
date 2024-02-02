using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer;
using Newtonsoft.Json;
using StructureMap.Query;

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
        [HttpGet("{VacancyId}/change-employer-legal-entity", Name = RouteNames.LegalEntityEmployerChange_Get)]
        public async Task<IActionResult> LegalEntityAndEmployer(VacancyRouteModel vrm, [FromQuery]string searchTerm, [FromQuery]int? page, [FromQuery] string sortOrder, [FromQuery] string sortByType)
        {

            Enum.TryParse<SortOrder>(sortOrder, out var outputSort);
            Enum.TryParse<SortByType>(sortByType, out var outputSortByType);
            
            
            var vm = await _orchestrator.GetLegalEntityAndEmployerViewModelAsync(vrm, searchTerm, page, outputSort, outputSortByType);

            if (vm.HasOnlyOneOrganisation)
            {
                var result =
                    await _orchestrator.PostConfirmAccountLegalEntityModel(new ConfirmLegalEntityAndEmployerEditModel
                    {
                        AccountLegalEntityName = vm.Organisations.First().AccountLegalEntityName,
                        EmployerAccountId = vm.Organisations.First().EmployerAccountId,
                        AccountLegalEntityPublicHashedId = vm.Organisations.First().Id,
                        VacancyId = vm.VacancyId,
                        Ukprn = vm.Ukprn
                    }, HttpContext.User.ToVacancyUser());
                
                return RedirectToRoute(RouteNames.ProviderTaskListGet, new {vrm.Ukprn, VacancyId = result.Data.Item1});
            }

            return View(vm);
        }

        [HttpPost("employer-legal-entity", Name = RouteNames.LegalEntityEmployer_Post)]
        public async Task<IActionResult> LegalEntityAndEmployer(LegalEntityAndEmployerEditModel m)
        {
            if (string.IsNullOrWhiteSpace(m.SelectedOrganisationId))
            {
                ModelState.AddModelError(nameof(m.SelectedOrganisationId), ValidationMessages.EmployerSelectionMessages.EmployerMustBeSelectedMessage);
            }

            var vm = await _orchestrator.GetLegalEntityAndEmployerViewModelAsync(new VacancyRouteModel
            {
                Ukprn = m.Ukprn,
                VacancyId = m.VacancyId
            }, m.SearchTerm, m.Page);
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            if(m.VacancyId != null)
            {
                return RedirectToRoute(RouteNames.ConfirmSelectedLegalEntityEmployer_Get, new { selectedId = m.SelectedOrganisationId, m.Ukprn, m.VacancyId});    
            }

            return RedirectToRoute(RouteNames.ConfirmLegalEntityEmployer_Get, new { selectedId = m.SelectedOrganisationId, m.Ukprn});
        }

        [HttpGet("confirm-employer-legal-entity", Name = RouteNames.ConfirmLegalEntityEmployer_Get)]
        [HttpGet("{VacancyId}/confirm-selected-legal-entity", Name = RouteNames.ConfirmSelectedLegalEntityEmployer_Get)]
        public async Task<IActionResult> ConfirmEmployerLegalEntitySelection(VacancyRouteModel vacancyRouteModel,[FromQuery] string selectedId)
        {
            if (vacancyRouteModel.VacancyId == null && string.IsNullOrEmpty(selectedId) && TempData["TempRouteValue"] is string model)
            {
                var obj = JsonConvert.DeserializeObject<TempRouteValue>(model);
                selectedId = obj.SelectedId;
                vacancyRouteModel.VacancyId = obj.VacancyId;
            }

            var employerAccountLegalEntityId = selectedId?.Split('|')[0];
            var employerAccountId = selectedId?.Split('|')[1];
            TempData["TempRouteValue"] = JsonConvert.SerializeObject(new { VacancyId = vacancyRouteModel.VacancyId, SelectedId = selectedId });
            
            var viewModel = await _orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, employerAccountId, employerAccountLegalEntityId);
            
            return View(viewModel);
        }

        [HttpPost("confirm-employer-legal-entity", Name = RouteNames.ConfirmLegalEntityEmployer_Post)]
        public async Task<IActionResult> ConfirmEmployerLegalEntitySelection(ConfirmLegalEntityAndEmployerEditModel model)
        {
            if (model.HasConfirmedEmployer.HasValue && !model.HasConfirmedEmployer.Value)
            {
                var routeName = model.VacancyId != null
                    ? RouteNames.LegalEntityEmployerChange_Get
                    : RouteNames.LegalEntityEmployer_Get;
                return RedirectToRoute(routeName, new {ukprn = model.Ukprn, vacancyId = model.VacancyId});
            }

            var result =
                await _orchestrator.PostConfirmAccountLegalEntityModel( model, HttpContext.User.ToVacancyUser());

            if (result.Data.Item2)
            {
                return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet,
                    new
                    {
                        vacancyId = result.Data.Item1, 
                        model.Ukprn
                    });
            }
            
            return RedirectToRoute(RouteNames.ProviderTaskListGet,
                    new
                            {
                                vacancyId = result.Data.Item1, 
                                model.Ukprn
                            });
        }
    }
    public class TempRouteValue
    {
        public Guid? VacancyId { get; set; }
        public string SelectedId { get; set; }
    }
}