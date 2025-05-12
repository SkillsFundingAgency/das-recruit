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
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FeatureNames = Esfa.Recruit.Employer.Web.Configuration.FeatureNames;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class EmployerOrchestrator : VacancyValidatingOrchestrator<VacancyEmployerInfoModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.None;
        private readonly IEmployerVacancyClient _employerVacancyClient;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly ILogger<EmployerOrchestrator> _logger;
        private readonly IUtility _utility;
        private readonly IFeature _feature;

        private const int MaxLegalEntitiesPerPage = 25;

        public EmployerOrchestrator(
            IEmployerVacancyClient employerVacancyClient,
            IRecruitVacancyClient vacancyClient,
            ILogger<EmployerOrchestrator> logger,
            IUtility utility,
            IFeature feature) : base(logger)
        {
            _employerVacancyClient = employerVacancyClient;
            _vacancyClient = vacancyClient;
            _logger = logger;
            _utility = utility;
            _feature = feature;
        }

        public async Task<EmployerViewModel> GetEmployerViewModelAsync(VacancyRouteModel vrm, string searchTerm, int? requestedPageNo, string selectedAccountLegalEntityPublicHashedId)
        {
            const int NotFoundIndex = -1;
            var setPage = requestedPageNo.HasValue ? requestedPageNo.Value : 1;

            var getEmployerDataTask = _employerVacancyClient.GetEditVacancyInfoAsync(vrm.EmployerAccountId);
            var getVacancyTask = _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Employer_Get);
            await Task.WhenAll(getEmployerDataTask, getVacancyTask);
            var employerData = getEmployerDataTask.Result;
            var vacancy = getVacancyTask.Result;

            if (employerData?.LegalEntities == null || !employerData.LegalEntities.Any())
            {
                await _employerVacancyClient.SetupEmployerAsync(vrm.EmployerAccountId);
                employerData = await _employerVacancyClient.GetEditVacancyInfoAsync(vrm.EmployerAccountId);
            }
            
            var legalEntities = BuildLegalEntityViewModels(employerData, vrm.EmployerAccountId);
            
            var vm = new EmployerViewModel
            {
                VacancyId = vrm.VacancyId,
                EmployerAccountId = vrm.EmployerAccountId,
                TotalNumberOfLegalEntities = legalEntities.Count(),
                PageInfo = _utility.GetPartOnePageInfo(vacancy),
                SearchTerm = searchTerm,
                SelectedOrganisationId = vacancy.AccountLegalEntityPublicHashedId
            };


            if (string.IsNullOrEmpty(selectedAccountLegalEntityPublicHashedId))
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
            vm.Page = setPage;

            vm.VacancyEmployerInfoModel = new VacancyEmployerInfoModel
            {
                VacancyId = vacancy.Id,
                EmployerAccountId = vrm.EmployerAccountId,
                AccountLegalEntityPublicHashedId = vacancy.AccountLegalEntityPublicHashedId 
            };

            if (vm.VacancyEmployerInfoModel.AccountLegalEntityPublicHashedId == null && vm.HasOnlyOneOrganisation)
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

        public async Task SetAccountLegalEntityPublicId(VacancyRouteModel vrm, VacancyEmployerInfoModel info, VacancyUser user)
        {
            Vacancy vacancy;
            
            if (_feature.IsFeatureEnabled(FeatureNames.MultipleLocations))
            {
                var vacancyTask = _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Employer_Get);
                var legalEntitiesTask = _employerVacancyClient.GetEmployerLegalEntitiesAsync(vrm.EmployerAccountId);
                await Task.WhenAll(vacancyTask, legalEntitiesTask);

                vacancy = vacancyTask.Result;
                vacancy.AccountLegalEntityPublicHashedId = info.AccountLegalEntityPublicHashedId;
                vacancy.LegalEntityName = legalEntitiesTask.Result?.FirstOrDefault(x => x.AccountLegalEntityPublicHashedId == vacancy.AccountLegalEntityPublicHashedId)?.Name;    
            }
            else
            {
                vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Employer_Get);
                vacancy.AccountLegalEntityPublicHashedId = info.AccountLegalEntityPublicHashedId;
            }
            
            await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, ValidationRules),
                async v =>
                {
                    await _vacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                });
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

        protected override EntityToViewModelPropertyMappings<Vacancy, VacancyEmployerInfoModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, VacancyEmployerInfoModel>
            {
                {
                    e=>e.AccountLegalEntityPublicHashedId, vm => vm.AccountLegalEntityPublicHashedId
                }
            };

            return mappings;
        }
    }
}