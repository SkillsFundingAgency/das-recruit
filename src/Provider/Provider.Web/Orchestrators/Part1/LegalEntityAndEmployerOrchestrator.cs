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
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Microsoft.Extensions.Logging;
using EmployerViewModel = Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer.EmployerViewModel;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class LegalEntityAndEmployerOrchestrator(
        IProviderVacancyClient providerVacancyClient,
        ILogger<LegalEntityOrchestrator> logger,
        IProviderRelationshipsService providerRelationshipsService,
        IEmployerAccountProvider employerAccountProvider,
        IRecruitVacancyClient recruitVacancyClient,
        IUtility utility,
        IFeature feature)
        : VacancyValidatingOrchestrator<LegalEntityAndEmployerEditModel>(logger)
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.None;
        private const int MaxLegalEntitiesPerPage = 25;


        public async Task<LegalEntityAndEmployerViewModel> GetLegalEntityAndEmployerViewModelAsync(VacancyRouteModel vrm, string searchTerm, int? requestedPageNo, SortOrder? sortOrder = null, SortByType? sortByType = null)
        {
            var editVacancyInfo = await providerVacancyClient.GetProviderEditVacancyInfoAsync(vrm.Ukprn);

            if (editVacancyInfo?.Employers == null || editVacancyInfo.Employers.Any() == false)
            {
                throw new MissingPermissionsException(string.Format(RecruitWebExceptionMessages.ProviderMissingPermission, vrm.Ukprn));
            }

            LegalEntityAndEmployerViewModel vm;
            int filteredLegalEntitiesTotal;

            var setPage = requestedPageNo.HasValue
                ? requestedPageNo.Value > 0
                    ? requestedPageNo.Value : 1 : 1;

            if (feature.IsFeatureEnabled(FeatureNames.MongoMigration))
            {
                var sortColumn = sortByType switch
                {
                    SortByType.LegalEntityName => "PublicHashedId",
                    _ => "Name"
                };
                var isAscending = sortOrder == SortOrder.Ascending;

                var employerAccountIds = editVacancyInfo.Employers.Select(info => info.EmployerAccountId).ToList();
                var accountLegal = await employerAccountProvider.GetAllLegalEntitiesConnectedToAccountAsync(
                    employerAccountIds,
                    searchTerm,
                    setPage,
                    MaxLegalEntitiesPerPage,
                    sortColumn,
                    isAscending);

                vm = new LegalEntityAndEmployerViewModel
                {
                    Employers = editVacancyInfo.Employers.Select(e => new EmployerViewModel { Id = e.EmployerAccountId, Name = e.Name }),
                    Organisations = accountLegal.LegalEntities.Select(x => new OrganisationsViewModel
                    {
                        AccountLegalEntityName = x.Name,
                        EmployerName = x.AccountName,
                        Id = x.AccountLegalEntityPublicHashedId,
                        EmployerAccountId = x.DasAccountId
                    }),
                    TotalNumberOfLegalEntities = MaxLegalEntitiesPerPage,
                    SearchTerm = searchTerm,
                    VacancyId = vrm.VacancyId,
                    SortByNameType = sortByType,
                    SortByAscDesc = sortOrder,
                    Ukprn = vrm.Ukprn,
                    NoOfSearchResults = accountLegal.PageInfo.TotalCount
                };
                filteredLegalEntitiesTotal = accountLegal.PageInfo.TotalCount;
            }
            else
            {
                //#TODO: Remove this when the feature MongoMigration goes LIVE
                var accountLegalEntities = await providerVacancyClient.GetProviderEmployerVacancyDatasAsync(vrm.Ukprn, editVacancyInfo.Employers.Select(info => info.EmployerAccountId).ToList());
                vm = new LegalEntityAndEmployerViewModel
                {
                    Employers = editVacancyInfo.Employers.Select(e => new EmployerViewModel { Id = e.EmployerAccountId, Name = e.Name }),
                    Organisations = GetLegalEntityAndEmployerViewModels(accountLegalEntities).OrderBy(a => a.EmployerName),
                    TotalNumberOfLegalEntities = accountLegalEntities.Sum(c => c.LegalEntities.Count),
                    SearchTerm = searchTerm,
                    VacancyId = vrm.VacancyId,
                    SortByNameType = sortByType,
                    SortByAscDesc = sortOrder,
                    Ukprn = vrm.Ukprn
                };

                var filterOrgs = vm.Organisations
                    .Where(le => string.IsNullOrEmpty(searchTerm) ||
                                 le.EmployerName.Replace(" ", "").Contains(searchTerm.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) ||
                                 le.AccountLegalEntityName.Replace(" ", "").Contains(searchTerm.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));

                List<OrganisationsViewModel> filterAndOrdered;
                if (sortOrder is SortOrder.Ascending)
                {
                    vm.SortByAscDesc = SortOrder.Descending;
                    filterAndOrdered = filterOrgs.OrderBy(c => sortByType is SortByType.LegalEntityName ? c.AccountLegalEntityName : c.EmployerName).ToList();
                }
                else
                {
                    vm.SortByAscDesc = SortOrder.Ascending;
                    filterAndOrdered = filterOrgs.OrderByDescending(c => sortByType is SortByType.LegalEntityName ? c.AccountLegalEntityName : c.EmployerName).ToList();
                }


                vm.NoOfSearchResults = filterAndOrdered.Count;
                filteredLegalEntitiesTotal = filterAndOrdered.Count;
                var totalNumberOfPages = PagingHelper.GetTotalNoOfPages(MaxLegalEntitiesPerPage, filteredLegalEntitiesTotal);

                vm.SortByNameType = sortByType;

                setPage = GetPageNo(setPage, totalNumberOfPages);

                SetFilteredOrganisationsForPage(setPage, vm, filterAndOrdered);
            }

            var routeParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(searchTerm))
                routeParams.Add(nameof(searchTerm), searchTerm);

            routeParams.Add("sortOrder", sortOrder.ToString());
            routeParams.Add("sortByType", sortByType.ToString());
            routeParams.Add("ukprn", vrm.Ukprn.ToString());
            var routeName = RouteNames.LegalEntityEmployer_Get;

            if (vrm.VacancyId != null)
            {
                routeName = RouteNames.LegalEntityEmployerChange_Get;
                routeParams.Add("vacancyId", vrm.VacancyId.ToString());    
            }

            SetPager(routeParams, setPage, vm, filteredLegalEntitiesTotal, routeName);
            return vm;
        }

        public async Task<ConfirmLegalEntityAndEmployerViewModel> GetConfirmLegalEntityViewModel(VacancyRouteModel vacancyRouteModel,string employerAccountId, string employerAccountLegalEntityPublicHashedId)
        {
            Guid? vacancyId = null;
            var taskListCompleted = false;
            if (vacancyRouteModel.VacancyId != null)
            {
                var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel,
                    RouteNames.ConfirmLegalEntityEmployer_Get);
                if (string.IsNullOrEmpty(employerAccountId))
                {
                    employerAccountId = vacancy.EmployerAccountId;    
                }

                if (string.IsNullOrEmpty(employerAccountLegalEntityPublicHashedId))
                {
                    employerAccountLegalEntityPublicHashedId = vacancy.AccountLegalEntityPublicHashedId;    
                }

                taskListCompleted = utility.IsTaskListCompleted(vacancy);
                
                vacancyId = vacancy.Id;
            }
            
            var employerVacancyInfo = await providerVacancyClient.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, employerAccountId);
            if (employerVacancyInfo?.LegalEntities == null || employerVacancyInfo.LegalEntities.Any() == false)
            {
                throw new MissingPermissionsException(string.Format(RecruitWebExceptionMessages.ProviderMissingPermission, vacancyRouteModel.Ukprn));
            }
            
            var selectedOrganisation = employerVacancyInfo.LegalEntities.SingleOrDefault(l => l.AccountLegalEntityPublicHashedId == employerAccountLegalEntityPublicHashedId);

            if (selectedOrganisation == null)
            {
                throw new MissingPermissionsException(string.Format(RecruitWebExceptionMessages.ProviderMissingPermission, vacancyRouteModel.Ukprn));
            }


            if (!await providerRelationshipsService.HasProviderGotEmployersPermissionAsync(vacancyRouteModel.Ukprn, employerAccountId, employerAccountLegalEntityPublicHashedId, OperationType.Recruitment))
            {
                if (!await providerRelationshipsService.HasProviderGotEmployersPermissionAsync(vacancyRouteModel.Ukprn,
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
                Ukprn = vacancyRouteModel.Ukprn,
                VacancyId = vacancyId,
                CancelLinkRoute = taskListCompleted ? RouteNames.ProviderCheckYourAnswersGet : (vacancyId == null ? RouteNames.Dashboard_Get : RouteNames.ProviderTaskListGet),
                BackLinkRoute = taskListCompleted ? RouteNames.ProviderCheckYourAnswersGet : (vacancyId == null ? RouteNames.LegalEntityEmployer_Get : RouteNames.ProviderTaskListGet)
            };
        }
        public async Task<OrchestratorResponse<Tuple<Guid, bool>>> PostConfirmAccountLegalEntityModel(ConfirmLegalEntityAndEmployerEditModel model, VacancyUser user)
        {
            if (model.VacancyId != null)
            {
                var vacancy = await utility.GetAuthorisedVacancyForEditAsync(new VacancyRouteModel
                    {
                        Ukprn = model.Ukprn,
                        VacancyId = model.VacancyId
                    },
                    RouteNames.ConfirmLegalEntityEmployer_Get);
                vacancy.EmployerAccountId = model.EmployerAccountId;
                vacancy.AccountLegalEntityPublicHashedId = model.AccountLegalEntityPublicHashedId;
                vacancy.LegalEntityName = model.AccountLegalEntityName;

                 await ValidateAndExecute(
                    vacancy,
                    v => recruitVacancyClient.Validate(v, ValidationRules),
                    async v => await recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user));
                 var isCompleted = utility.IsTaskListCompleted(vacancy);
                 return new OrchestratorResponse<Tuple<Guid, bool>>(new Tuple<Guid, bool>(vacancy.Id, isCompleted));
            }
            
            var newVacancy = new Vacancy
            {
                TrainingProvider = new TrainingProvider { Ukprn = user.Ukprn },
                EmployerAccountId = model.EmployerAccountId,
                AccountLegalEntityPublicHashedId = model.AccountLegalEntityPublicHashedId,
                LegalEntityName = model.AccountLegalEntityName,
                
            };

            var result =  await ValidateAndExecute(
                newVacancy,
                v => recruitVacancyClient.Validate(v, ValidationRules),
                async v => await providerVacancyClient.CreateVacancyAsync(
                    model.EmployerAccountId, user.Ukprn.Value, null, user, model.AccountLegalEntityPublicHashedId, model.AccountLegalEntityName));

            return new OrchestratorResponse<Tuple<Guid, bool>>(new Tuple<Guid, bool>(result.Data, false));
        }

        private int GetPageNo(int page, int totalNumberOfPages)
        {
            page = page > totalNumberOfPages ? 1 : page;
            return page;
        }

        private void SetPager(Dictionary<string, string> routeParams, int page, LegalEntityAndEmployerViewModel vm, int filteredLegalEntitiesTotal, string routeName)
        {
            var pager = new PagerViewModel(
                filteredLegalEntitiesTotal,
                MaxLegalEntitiesPerPage,
                page,
                "Showing {0} to {1} of {2} organisations",
                routeName,
                routeParams);

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
                    logger.LogWarning("No legal entities found for {employerAccountId}", employerId.EmployerAccountId);
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