using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Microsoft.Extensions.Logging;
using EmployerViewModel = Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer.EmployerViewModel;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class LegalEntityAndEmployerOrchestrator : VacancyValidatingOrchestrator<LegalEntityAndEmployerEditModel>
    {
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly ILogger<LegalEntityOrchestrator> _logger;
        private readonly IProviderRelationshipsService _providerRelationshipsService;
        private const int MaxLegalEntitiesPerPage = 25;

        public LegalEntityAndEmployerOrchestrator(
            IProviderVacancyClient providerVacancyClient,
            ILogger<LegalEntityOrchestrator> logger,
            IProviderRelationshipsService providerRelationshipsService): base(logger)
        {
            _providerVacancyClient = providerVacancyClient;
            _logger = logger;
            _providerRelationshipsService = providerRelationshipsService;
        }

        public async Task<LegalEntityAndEmployerViewModel> GetLegalEntityAndEmployerViewModelAsync(VacancyRouteModel vrm, string searchTerm, int? requestedPageNo)
        {
            var editVacancyInfo = await _providerVacancyClient.GetProviderEditVacancyInfoAsync(vrm.Ukprn);

            if (editVacancyInfo?.Employers == null || editVacancyInfo.Employers.Any() == false)
            {
                throw new MissingPermissionsException(string.Format(RecruitWebExceptionMessages.ProviderMissingPermission, vrm.Ukprn));
            }

            var accountLegalEntities = await _providerVacancyClient.GetProviderEmployerVacancyDatasAsync(vrm.Ukprn, editVacancyInfo.Employers.Select(info => info.EmployerAccountId).ToList());

            const int NotFoundIndex = -1;
            var setPage = requestedPageNo.HasValue ? requestedPageNo.Value : 1;


            var vm = new LegalEntityAndEmployerViewModel
            {
                Employers = editVacancyInfo.Employers.Select(e => new EmployerViewModel { Id = e.EmployerAccountId, Name = e.Name}),
                Organisations = GetLegalEntityAndEmployerViewModels(accountLegalEntities).OrderBy(a => a.EmployerName),
                TotalNumberOfLegalEntities = accountLegalEntities.Count(),
                SearchTerm = searchTerm,
                VacancyId = vrm.VacancyId,
                Ukprn = vrm.Ukprn
            };

            var filteredLegalEntities = vm.Organisations
                .Where(le => string.IsNullOrEmpty(searchTerm) || le.EmployerName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || le.AccountLegalEntityName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderBy(v => v.EmployerName)
                .ToList();

            vm.NoOfSearchResults = filteredLegalEntities.Count();

            var filteredLegalEntitiesTotal = filteredLegalEntities.Count();
            var totalNumberOfPages = PagingHelper.GetTotalNoOfPages(MaxLegalEntitiesPerPage, filteredLegalEntitiesTotal);

            setPage = GetPageNo(requestedPageNo, setPage, totalNumberOfPages);

            SetFilteredOrganisationsForPage(setPage, vm, filteredLegalEntities);
            SetPager(searchTerm, setPage, vm, filteredLegalEntitiesTotal);

            return vm;
        }

        public async Task<ConfirmLegalEntityAndEmployerViewModel> GetConfirmLegalEntityViewModel(VacancyRouteModel vacancyRouteModel,string employerAccountId, string employerAccountLegalEntityPublicHashedId)
        {
            var employerVacancyInfo = await _providerVacancyClient.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, employerAccountId);
            if (employerVacancyInfo?.LegalEntities == null || employerVacancyInfo.LegalEntities.Any() == false)
            {
                throw new MissingPermissionsException(string.Format(RecruitWebExceptionMessages.ProviderMissingPermission, vacancyRouteModel.Ukprn));
            }
            
            var selectedOrganisation = employerVacancyInfo.LegalEntities.SingleOrDefault(l => l.AccountLegalEntityPublicHashedId == employerAccountLegalEntityPublicHashedId);

            if (selectedOrganisation == null)
            {
                throw new MissingPermissionsException(string.Format(RecruitWebExceptionMessages.ProviderMissingPermission, vacancyRouteModel.Ukprn));
            }


            if (!await _providerRelationshipsService.HasProviderGotEmployersPermissionAsync(vacancyRouteModel.Ukprn, employerAccountId, employerAccountLegalEntityPublicHashedId, OperationType.Recruitment))
            {
                if (!await _providerRelationshipsService.HasProviderGotEmployersPermissionAsync(vacancyRouteModel.Ukprn,
                        employerAccountId, employerAccountLegalEntityPublicHashedId,
                        OperationType.RecruitmentRequiresReview))
                {
                    throw new MissingPermissionsException(string.Format(RecruitWebExceptionMessages.ProviderMissingPermission, vacancyRouteModel.Ukprn));    
                }
            }
            
            return new ConfirmLegalEntityAndEmployerViewModel
            {
                AccountLegalEntityPublicHashedId = selectedOrganisation?.AccountLegalEntityPublicHashedId,
                EmployerName = employerVacancyInfo.Name,
                EmployerAccountId = employerVacancyInfo.EmployerAccountId,
                AccountLegalEntityName = selectedOrganisation?.Name,
                Ukprn = vacancyRouteModel.Ukprn
            };
        }

        private int GetPageNo(int? requestedPageNo, int page, int totalNumberOfPages)
        {
            page = page > totalNumberOfPages ? 1 : page;
            return page;
        }

        private void SetPager(string searchTerm, int page, LegalEntityAndEmployerViewModel vm, int filteredLegalEntitiesTotal)
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

        private void SetFilteredOrganisationsForPage(int page, LegalEntityAndEmployerViewModel vm, List<OrganisationsViewModel> filteredLegalEntities)
        {
            var skip = (page - 1) * MaxLegalEntitiesPerPage;

            vm.Organisations = filteredLegalEntities
                .Skip(skip)
                .Take(MaxLegalEntitiesPerPage)
                .ToList();
        }

        private OrganisationsViewModel ConvertToOrganisationViewModel(LegalEntityEmployerViewModel data)
        {
            return new OrganisationsViewModel { 
                Id = data.LegalEntity.AccountLegalEntityPublicHashedId, 
                AccountLegalEntityName = data.LegalEntity.Name, 
                EmployerName = data.EmployerName,
                EmployerAccountId = data.EmployerAccountId
            };
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

                legalEntities.AddRange(employerId.LegalEntities.Select(x => ConvertToOrganisationViewModel(new LegalEntityEmployerViewModel{LegalEntity = x, EmployerName = employerId.Name, EmployerAccountId = employerId.EmployerAccountId})));
                
            }
            return legalEntities;

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

    public class LegalEntityEmployerViewModel
    {
        public LegalEntity LegalEntity { get; set; }
        public string EmployerName { get; set; }
        public string EmployerAccountId { get; set; }
    }
}