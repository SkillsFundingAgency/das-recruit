using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntity;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class LegalEntityOrchestrator
    {
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly ILogger<LegalEntityOrchestrator> _logger;

        private const int MaxLegalEntitiesPerPage = 25;

        public LegalEntityOrchestrator(
            IProviderVacancyClient providerVacancyClient,
            IRecruitVacancyClient recruitVacancyClient,
            ILogger<LegalEntityOrchestrator> logger)
        {
            _providerVacancyClient = providerVacancyClient;
            _recruitVacancyClient = recruitVacancyClient;
            _logger = logger;
        }

        public async Task<LegalEntityViewModel> GetLegalEntityViewModelAsync(VacancyRouteModel vrm, long ukprn, string searchTerm, int? requestedPageNo, string selectedAccountLegalEntityPublicHashedId)
        {
            const int NotFoundIndex = -1;
            var setPage = requestedPageNo.HasValue ? requestedPageNo.Value : 1;

            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_providerVacancyClient, _recruitVacancyClient, vrm, RouteNames.LegalEntity_Get);
            var legalEntities = (await GetLegalEntityViewModelsAsync(ukprn, vacancy.EmployerAccountId)).ToList();

            var vm = new LegalEntityViewModel
            {
                TotalNumberOfLegalEntities = legalEntities.Count(),
                SelectedOrganisationId = vacancy.AccountLegalEntityPublicHashedId,
                PageInfo = Utility.GetPartOnePageInfo(vacancy),
                SearchTerm = searchTerm
            };

            if (!string.IsNullOrEmpty(vacancy.AccountLegalEntityPublicHashedId) && string.IsNullOrEmpty(selectedAccountLegalEntityPublicHashedId))
            {
                selectedAccountLegalEntityPublicHashedId = vacancy.AccountLegalEntityPublicHashedId;
            }

            vm.IsPreviouslySelectedLegalEntityStillValid = !string.IsNullOrEmpty(selectedAccountLegalEntityPublicHashedId) && legalEntities.Any(le => le.Id == selectedAccountLegalEntityPublicHashedId);

            var filteredLegalEntities = legalEntities
                .Where(le => string.IsNullOrEmpty(searchTerm) || le.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderBy(v => v.Name)
                .ToList();

            var filteredLegalEntitiesTotal = filteredLegalEntities.Count();

            var totalNumberOfPages = PagingHelper.GetTotalNoOfPages(MaxLegalEntitiesPerPage, filteredLegalEntitiesTotal);
            var indexOfSelectedLegalEntity = !string.IsNullOrEmpty(selectedAccountLegalEntityPublicHashedId)
                                            ? filteredLegalEntities.FindIndex(le => le.Id == selectedAccountLegalEntityPublicHashedId) + 1
                                            : NotFoundIndex;

            setPage = GetPageNo(requestedPageNo, setPage, totalNumberOfPages, indexOfSelectedLegalEntity);

            SetFilteredOrganisationsForPage(setPage, vm, filteredLegalEntities);
            SetPager(searchTerm, setPage, vm, filteredLegalEntitiesTotal);

            vm.VacancyEmployerInfoModel = new VacancyEmployerInfoModel
            {
                VacancyId = vacancy.Id,
                AccountLegalEntityPublicHashedId = vacancy.AccountLegalEntityPublicHashedId
            };

            if (string.IsNullOrEmpty(vm.VacancyEmployerInfoModel.AccountLegalEntityPublicHashedId) && vm.HasOnlyOneOrganisation)
            {
                vm.VacancyEmployerInfoModel.AccountLegalEntityPublicHashedId = vm.Organisations.First().Id;
            }

            if (vacancy.EmployerNameOption.HasValue)
            {
                vm.VacancyEmployerInfoModel.EmployerIdentityOption = vacancy.EmployerNameOption.Value.ConvertToModelOption();
                vm.VacancyEmployerInfoModel.AnonymousName = vacancy.IsAnonymous ? vacancy.EmployerName : null;
                vm.VacancyEmployerInfoModel.AnonymousReason = vacancy.IsAnonymous ? vacancy.AnonymousReason : null;
            }

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

        private void SetPager(string searchTerm, int page, LegalEntityViewModel vm, int filteredLegalEntitiesTotal)
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

        private void SetFilteredOrganisationsForPage(int page, LegalEntityViewModel vm, List<OrganisationViewModel> filteredLegalEntities)
        {
            var skip = (page - 1) * MaxLegalEntitiesPerPage;

            vm.Organisations = filteredLegalEntities
                .Skip(skip)
                .Take(MaxLegalEntitiesPerPage)
                .ToList();
        }

        private OrganisationViewModel ConvertToOrganisationViewModel(LegalEntity data)
        {
            return new OrganisationViewModel { Id = data.AccountLegalEntityPublicHashedId, Name = data.Name };
        }

        private async Task<List<OrganisationViewModel>> GetLegalEntityViewModelsAsync(long ukprn, string employerAccountId)
        {
            var info = await _providerVacancyClient.GetProviderEmployerVacancyDataAsync(ukprn, employerAccountId);

            if (info == null || !info.LegalEntities.Any())
            {
                _logger.LogWarning("No legal entities found for {employerAccountId}", employerAccountId);
                return new List<OrganisationViewModel>();
            }

            return info.LegalEntities.Select(ConvertToOrganisationViewModel).ToList();
        }
    }
}