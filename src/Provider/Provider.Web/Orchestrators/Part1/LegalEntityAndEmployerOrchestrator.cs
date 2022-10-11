using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using EmployerViewModel = Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer.EmployerViewModel;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class LegalEntityAndEmployerOrchestrator : VacancyValidatingOrchestrator<LegalEntityAndEmployerEditModel>
    {
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly ILogger<LegalEntityOrchestrator> _logger;
        private readonly IUtility _utility;
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.None;
        private const int MaxLegalEntitiesPerPage = 25;

        public LegalEntityAndEmployerOrchestrator(
            IProviderVacancyClient providerVacancyClient,
            IRecruitVacancyClient vacancyClient,
            ILogger<LegalEntityOrchestrator> logger,
            IUtility utility): base(logger)
        {
            _providerVacancyClient = providerVacancyClient;
            _vacancyClient = vacancyClient;
            _logger = logger;
            _utility = utility;
        }

        public async Task<LegalEntityandEmployerViewModel> GetLegalEntityAndEmployerViewModelAsync(VacancyRouteModel vrm, long ukprn, string searchTerm, int? requestedPageNo, string selectedAccountLegalEntityPublicHashedId)
        {
            var editVacancyInfo = await _providerVacancyClient.GetProviderEditVacancyInfoAsync(vrm.Ukprn);

            if (editVacancyInfo?.Employers == null || editVacancyInfo.Employers.Any() == false)
            {
                throw new MissingPermissionsException(string.Format(RecruitWebExceptionMessages.ProviderMissingPermission, vrm.Ukprn));
            }

            var accountLegalEntities = await _providerVacancyClient.GetProviderEmployerVacancyDatasAsync(vrm.Ukprn, editVacancyInfo.Employers.Select(info => info.EmployerAccountId).ToList());

            const int NotFoundIndex = -1;
            var setPage = requestedPageNo.HasValue ? requestedPageNo.Value : 1;


            var vm = new LegalEntityandEmployerViewModel
            {
                Employers = editVacancyInfo.Employers.Select(e => new EmployerViewModel { Id = e.EmployerAccountId, Name = e.Name}),
                Organisations = GetLegalEntityAndEmployerViewModels(accountLegalEntities),
                //TotalNumberOfLegalEntities = legalEntities.Count(),
                SearchTerm = searchTerm,
                VacancyId = vrm.VacancyId,
                Ukprn = vrm.Ukprn
            };

            vm.IsPreviouslySelectedLegalEntityStillValid = !string.IsNullOrEmpty(selectedAccountLegalEntityPublicHashedId) && vm.Organisations.Any(le => le.Id == selectedAccountLegalEntityPublicHashedId);

            var filteredLegalEntities = vm.Organisations
                .Where(le => string.IsNullOrEmpty(searchTerm) || le.AccountLegalEntityName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderBy(v => v.AccountLegalEntityName)
                .ToList();

            var filteredLegalEntitiesTotal = filteredLegalEntities.Count();

            var totalNumberOfPages = PagingHelper.GetTotalNoOfPages(MaxLegalEntitiesPerPage, filteredLegalEntitiesTotal);
            var indexOfSelectedLegalEntity = !string.IsNullOrEmpty(selectedAccountLegalEntityPublicHashedId)
                                            ? filteredLegalEntities.FindIndex(le => le.Id == selectedAccountLegalEntityPublicHashedId) + 1
                                            : NotFoundIndex;

            setPage = GetPageNo(requestedPageNo, setPage, totalNumberOfPages, indexOfSelectedLegalEntity);

            //SetFilteredOrganisationsForPage(setPage, vm, filteredLegalEntities);
            SetPager(searchTerm, setPage, vm, filteredLegalEntitiesTotal);


            return vm;
        }

        private int GetPageNo(int? requestedPageNo, int page, int totalNumberOfPages, int indexOfSelectedLegalEntity)
        {
            if (indexOfSelectedLegalEntity > MaxLegalEntitiesPerPage && requestedPageNo.HasValue == false)
                page = PagingHelper.GetPageNoOfSelectedItem(totalNumberOfPages, MaxLegalEntitiesPerPage, indexOfSelectedLegalEntity);
            else
                page = page > totalNumberOfPages ? 1 : page;
            return page;
        }

        private void SetPager(string searchTerm, int page, LegalEntityandEmployerViewModel vm, int filteredLegalEntitiesTotal)
        {
            var pager = new PagerViewModel(
                            filteredLegalEntitiesTotal,
                            MaxLegalEntitiesPerPage,
                            page,
                            "Showing {0} to {1} of {2} organisations",
                            RouteNames.LegalEntity_Get,
                            new Dictionary<string, string>
                            {
                                { nameof(searchTerm), searchTerm }
                            });

            vm.Pager = pager;
        }

        //private void SetFilteredOrganisationsForPage(int page, LegalEntityandEmployerViewModel vm, List<OrganisationsViewModel> filteredLegalEntities)
        //{
        //    var skip = (page - 1) * MaxLegalEntitiesPerPage;

        //    vm.Organisations = filteredLegalEntities
        //        .Skip(skip)
        //        .Take(MaxLegalEntitiesPerPage)
        //        .ToList();
        //}

        private OrganisationsViewModel ConvertToOrganisationViewModel(TempOrganisationViewModel data)
        {
            return new OrganisationsViewModel { Id = data.LegalEntity.AccountLegalEntityPublicHashedId, AccountLegalEntityName = data.LegalEntity.Name, EmployerName = data.EmployerName };
        }

        private IEnumerable<OrganisationsViewModel> GetLegalEntityAndEmployerViewModels(IEnumerable<EmployerInfo> accountLegalEntities)
        {
            List<OrganisationsViewModel> legalEntities = new List<OrganisationsViewModel>();
            foreach (var employerId in accountLegalEntities)
            {

                if (!employerId.LegalEntities.Any())
                {
                    _logger.LogWarning("No legal entities found for {employerAccountId}", employerId.EmployerAccountId);
                    return new List<OrganisationsViewModel>();
                }

                legalEntities.AddRange(employerId.LegalEntities.Select(x => ConvertToOrganisationViewModel(new TempOrganisationViewModel{LegalEntity = x, EmployerName = employerId.Name})));
                
            }
            return legalEntities;

        }

        public async Task SetAccountLegalEntityPublicId(VacancyRouteModel vacancyRouteModel, LegalEntityAndEmployerEditModel vacancyEmployerInfoModel, VacancyUser vacancyUser)
        {
            
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.LegalEntityEmployer_Get);
            var employerVacancyInfo = await _providerVacancyClient.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, vacancy.EmployerAccountId);
            vacancy.AccountLegalEntityPublicHashedId = vacancyEmployerInfoModel.SelectedOrganisationId;
            
            var selectedOrganisation = employerVacancyInfo.LegalEntities.Single(l => l.AccountLegalEntityPublicHashedId == vacancyEmployerInfoModel.SelectedOrganisationId);
            vacancy.LegalEntityName = selectedOrganisation.Name;
            
            await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, ValidationRules),
                async v =>
                {
                    await _vacancyClient.UpdateDraftVacancyAsync(vacancy, vacancyUser);
                });
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, LegalEntityAndEmployerEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, LegalEntityAndEmployerEditModel>
            {
                {
                    e=>e.AccountLegalEntityPublicHashedId, vm => vm.SelectedOrganisationId
                }
            };

            return mappings;
        }
    }

    public class TempOrganisationViewModel
    {
        public LegalEntity LegalEntity { get; set; }
        public string EmployerName { get; set; }
    }
}