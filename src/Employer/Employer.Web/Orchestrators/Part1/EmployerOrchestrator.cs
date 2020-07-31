using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Shared.Web.Helpers;
using System;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class EmployerOrchestrator
    {
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly ILogger<EmployerOrchestrator> _logger;

        private const int MaxLegalEntitiesPerPage = 25;

        public EmployerOrchestrator(
            IEmployerVacancyClient client,
            IRecruitVacancyClient vacancyClient,
            ILogger<EmployerOrchestrator> logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _logger = logger;
        }

        public async Task<EmployerViewModel> GetEmployerViewModelAsync(VacancyRouteModel vrm, string searchTerm, int? requestedPageNo, string accountLegalEntityPublicHashedId)
        {
            const int NotFoundIndex = -1;
            var setPage = requestedPageNo.HasValue ? requestedPageNo.Value : 1;

            var getEmployerDataTask = _client.GetEditVacancyInfoAsync(vrm.EmployerAccountId);
            var getVacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Employer_Get);
            await Task.WhenAll(getEmployerDataTask, getVacancyTask);
            var employerData = getEmployerDataTask.Result;
            var vacancy = getVacancyTask.Result;

            var legalEntities = BuildLegalEntityViewModels(employerData, vrm.EmployerAccountId);

            var vm = new EmployerViewModel
            {
                TotalNumberOfLegalEntities = legalEntities.Count(),
                PageInfo = Utility.GetPartOnePageInfo(vacancy),
                SearchTerm = searchTerm,
                AccountLegalEntityPublicHashedId = vacancy.AccountLegalEntityPublicHashedId
            };

            //if (vacancy.LegalEntityId != 0 && (selectedLegalEntityId.HasValue == false || selectedLegalEntityId == 0))
            //{
            //    selectedLegalEntityId = vacancy.LegalEntityId;
            //}

            vm.IsPreviouslySelectedLegalEntityStillValid = !string.IsNullOrEmpty(accountLegalEntityPublicHashedId) && legalEntities.Any(le => le.Id == accountLegalEntityPublicHashedId);

            var filteredLegalEntities = legalEntities
                .Where(le => string.IsNullOrEmpty(searchTerm) || le.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderBy(v => v.Name)
                .ToList();

            var filteredLegalEntitiesTotal = filteredLegalEntities.Count();

            var totalNumberOfPages = PagingHelper.GetTotalNoOfPages(MaxLegalEntitiesPerPage, filteredLegalEntitiesTotal);
            var indexOfSelectedLegalEntity = !string.IsNullOrEmpty(accountLegalEntityPublicHashedId)
                                                ? filteredLegalEntities.FindIndex(le => le.Id == accountLegalEntityPublicHashedId) + 1
                                                : NotFoundIndex;

            setPage = GetPageNo(requestedPageNo, setPage, totalNumberOfPages, indexOfSelectedLegalEntity);

            SetFilteredOrganisationsForPage(setPage, vm, filteredLegalEntities);
            SetPager(searchTerm, setPage, vm, filteredLegalEntitiesTotal);
            vm.Page = setPage;

            vm.VacancyEmployerInfoModel = new VacancyEmployerInfoModel()
            {
                VacancyId = vacancy.Id,
                AccountLegalEntityPublicHashedId = vacancy.AccountLegalEntityPublicHashedId 
            };

            //if (vm.VacancyEmployerInfoModel.AccountLegalEntityPublicHashedId == null && vm.HasOnlyOneOrganisation)
            //{
            //    vm.VacancyEmployerInfoModel.AccountLegalEntityPublicHashedId = vm.Organisations.First().Id;
            //}

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

        private void SetPager(string searchTerm, int page, EmployerViewModel vm, int filteredLegalEntitiesTotal)
        {
            var pager = new PagerViewModel(
                            filteredLegalEntitiesTotal,
                            MaxLegalEntitiesPerPage,
                            page,
                            "Showing {0} to {1} of {2} organisations",
                            RouteNames.Employer_Get,
                            new Dictionary<string, string>
                            {
                                { nameof(searchTerm), searchTerm }
                            });

            vm.Pager = pager;
        }

        private void SetFilteredOrganisationsForPage(int page, EmployerViewModel vm, List<OrganisationViewModel> filteredLegalEntities)
        {
            var skip = (page - 1) * MaxLegalEntitiesPerPage;

            vm.Organisations = filteredLegalEntities
                .Skip(skip)
                .Take(MaxLegalEntitiesPerPage)
                .ToList();
        }

        private IEnumerable<OrganisationViewModel> BuildLegalEntityViewModels(EmployerEditVacancyInfo info, string employerAccountId)
        {
            if (info == null || !info.LegalEntities.Any())
            {
                _logger.LogError("No legal entities found for {employerAccountId}", employerAccountId);
                return new List<OrganisationViewModel>();
            }

            return info.LegalEntities.Select(ConvertToOrganisationViewModel).ToList();
        }

        private OrganisationViewModel ConvertToOrganisationViewModel(LegalEntity data)
        {
            return new OrganisationViewModel { Id = data.AccountLegalEntityPublicHashedId, Name = data.Name};
        }
    }
}