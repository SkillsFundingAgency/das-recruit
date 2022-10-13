using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.BlockedOrganisations;
using Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    public class QueryStoreClient : IQueryStoreReader, IQueryStoreWriter
    {
        private readonly IQueryStore _queryStore;
        private readonly ITimeProvider _timeProvider;

        public QueryStoreClient(IQueryStore queryStore, ITimeProvider timeProvider)
        {
            _queryStore = queryStore;
            _timeProvider = timeProvider;
        }

        public Task<EmployerDashboard> GetEmployerDashboardAsync(string employerAccountId)
        {
            var key = QueryViewType.EmployerDashboard.GetIdValue(employerAccountId);

            return _queryStore.GetAsync<EmployerDashboard>(QueryViewType.EmployerDashboard.TypeName, key);
        }

        public Task<ProviderDashboard> GetProviderDashboardAsync(long ukprn, VacancyType vacancyType)
        {
            var key = string.Empty;
            var typeName = string.Empty;

            if (vacancyType == VacancyType.Apprenticeship)
            {
                key = QueryViewType.ProviderDashboard.GetIdValue(ukprn);
                typeName = QueryViewType.ProviderDashboard.TypeName;
            }
            else if (vacancyType == VacancyType.Traineeship)
            {
                key = QueryViewType.ProviderTraineeshipDashboard.GetIdValue(ukprn);
                typeName = QueryViewType.ProviderDashboard.TypeName;
            }

            return _queryStore.GetAsync<ProviderDashboard>(typeName, key);
        }

        public Task<VacancyAnalyticsSummary> GetVacancyAnalyticsSummaryAsync(long vacancyReference)
        {
            var key = QueryViewType.VacancyAnalyticsSummary.GetIdValue(vacancyReference);

            return _queryStore.GetAsync<VacancyAnalyticsSummary>(QueryViewType.VacancyAnalyticsSummary.TypeName, key);
        }

        public Task<BlockedProviderOrganisations> GetBlockedProviders()
        {
            var key = QueryViewType.BlockedProviderOrganisations.GetIdValue();

            return _queryStore.GetAsync<BlockedProviderOrganisations>(key);
        }

        public Task UpdateEmployerDashboardAsync(string employerAccountId, IEnumerable<VacancySummary> vacancySummaries)
        {
            var dashboardItem = new EmployerDashboard
            {
                Id = QueryViewType.EmployerDashboard.GetIdValue(employerAccountId),
                Vacancies = vacancySummaries,
                LastUpdated = _timeProvider.Now
            };

            return _queryStore.UpsertAsync(dashboardItem);
        }

        public Task UpdateProviderDashboardAsync(long ukprn, IEnumerable<VacancySummary> vacancySummaries, IEnumerable<ProviderDashboardTransferredVacancy> transferredVacancies, VacancyType vacancyType)
        {
            var dashboardItem = new ProviderDashboard
            {
                Id = vacancyType == VacancyType.Apprenticeship ? QueryViewType.ProviderDashboard.GetIdValue(ukprn) :  QueryViewType.ProviderTraineeshipDashboard.GetIdValue(ukprn),
                Vacancies = vacancySummaries,
                TransferredVacancies = transferredVacancies,
                LastUpdated = _timeProvider.Now
            };

            return _queryStore.UpsertAsync(dashboardItem);
        }

        public Task UpdateEmployerVacancyDataAsync(string employerAccountId, IEnumerable<LegalEntity> legalEntities)
        {
            var employerVacancyDataItem = new EmployerEditVacancyInfo
            {
                Id = QueryViewType.EditVacancyInfo.GetIdValue(employerAccountId),
                LegalEntities = legalEntities,
                LastUpdated = _timeProvider.Now
            };

            return _queryStore.UpsertAsync(employerVacancyDataItem);
        }

        public Task UpdateProviderVacancyDataAsync(long ukprn, IEnumerable<EmployerInfo> employers, bool hasAgreement)
        {
            var providerVacancyDataItem = new ProviderEditVacancyInfo
            {
                Id = QueryViewType.EditVacancyInfo.GetIdValue(ukprn),
                Employers = employers,
                HasProviderAgreement = hasAgreement,
                LastUpdated = _timeProvider.Now
            };

            return _queryStore.UpsertAsync(providerVacancyDataItem);
        }

        public Task<EmployerEditVacancyInfo> GetEmployerVacancyDataAsync(string employerAccountId)
        {
            var key = QueryViewType.EditVacancyInfo.GetIdValue(employerAccountId);

            return _queryStore.GetAsync<EmployerEditVacancyInfo>(QueryViewType.EditVacancyInfo.TypeName, key);
        }

        public Task<ProviderEditVacancyInfo> GetProviderVacancyDataAsync(long ukprn)
        {
            var key = QueryViewType.EditVacancyInfo.GetIdValue(ukprn);

            return _queryStore.GetAsync<ProviderEditVacancyInfo>(QueryViewType.EditVacancyInfo.TypeName, key);
        }

        public async Task<EmployerInfo> GetProviderEmployerVacancyDataAsync(long ukprn, string employerAccountId)
        {
            var key = QueryViewType.EditVacancyInfo.GetIdValue(ukprn);
            var providerInfo = await _queryStore.GetAsync<ProviderEditVacancyInfo>(QueryViewType.EditVacancyInfo.TypeName, key);
            return providerInfo?.Employers.FirstOrDefault(e => e.EmployerAccountId == employerAccountId);
        }

        public Task<VacancyApplications> GetVacancyApplicationsAsync(string vacancyReference)
        {
            var key = QueryViewType.VacancyApplications.GetIdValue(vacancyReference);

            return _queryStore.GetAsync<VacancyApplications>(QueryViewType.VacancyApplications.TypeName, key);
        }

        public Task UpdateLiveVacancyAsync(LiveVacancy vacancy)
        {
            return _queryStore.UpsertAsync(vacancy);
        }

        public Task DeleteLiveVacancyAsync(long vacancyReference)
        {
            var liveVacancyId = GetLiveVacancyId(vacancyReference);
            return _queryStore.DeleteAsync<LiveVacancy>(QueryViewType.LiveVacancy.TypeName, liveVacancyId);
        }

        public Task<ClosedVacancy> GetClosedVacancy(long vacancyReference)
        {
            var key = QueryViewType.ClosedVacancy.GetIdValue(vacancyReference.ToString());
            return _queryStore.GetAsync<ClosedVacancy>(QueryViewType.ClosedVacancy.TypeName, key);
        }

        public Task<long> DeleteAllLiveVacancies()
        {
            return _queryStore.DeleteAllAsync<LiveVacancy>(QueryViewType.LiveVacancy.TypeName);
        }

        public Task<long> DeleteAllClosedVacancies()
        {
            return _queryStore.DeleteAllAsync<ClosedVacancy>(QueryViewType.ClosedVacancy.TypeName);
        }

        public Task<IEnumerable<LiveVacancy>> GetLiveExpiredVacancies(DateTime closingDate)
        {
            return _queryStore.GetAllLiveExpired(closingDate);
        }

        public Task UpdateVacancyApplicationsAsync(VacancyApplications vacancyApplications)
        {
            vacancyApplications.Id = QueryViewType.VacancyApplications.GetIdValue(vacancyApplications.VacancyReference.ToString());
            vacancyApplications.LastUpdated = _timeProvider.Now;

            return _queryStore.UpsertAsync(vacancyApplications);
        }

        public Task<QaDashboard> GetQaDashboardAsync()
        {
            var key = QueryViewType.QaDashboard.GetIdValue();

            return _queryStore.GetAsync<QaDashboard>(QueryViewType.QaDashboard.TypeName, key);
        }

        public Task UpdateQaDashboardAsync(QaDashboard qaDashboard)
        {
            qaDashboard.Id = QueryViewType.QaDashboard.GetIdValue();
            qaDashboard.LastUpdated = _timeProvider.Now;

            return _queryStore.UpsertAsync(qaDashboard);
        }

        public Task UpdateClosedVacancyAsync(ClosedVacancy closedVacancy)
        {
            return _queryStore.UpsertAsync(closedVacancy);
        }

        public Task<long> RemoveOldEmployerDashboards(DateTime oldestLastUpdatedDate)
        {
            return _queryStore.DeleteManyLessThanAsync<EmployerDashboard, DateTime>(QueryViewType.EmployerDashboard.TypeName, x => x.LastUpdated, oldestLastUpdatedDate);
        }

        public Task<long> RemoveOldProviderDashboards(DateTime oldestLastUpdatedDate)
        {
            //TODO
            return _queryStore.DeleteManyLessThanAsync<ProviderDashboard, DateTime>(QueryViewType.ProviderDashboard.TypeName, x => x.LastUpdated, oldestLastUpdatedDate);
        }

        public async Task UpsertVacancyAnalyticSummaryAsync(VacancyAnalyticsSummary summary)
        {
            summary.Id = QueryViewType.VacancyAnalyticsSummary.GetIdValue(summary.VacancyReference.ToString());
            summary.LastUpdated = _timeProvider.Now;

            await _queryStore.UpsertAsync(summary);
        }

        private string GetLiveVacancyId(long vacancyReference)
        {
            return QueryViewType.LiveVacancy.GetIdValue(vacancyReference.ToString());
        }

        public Task UpdateBlockedProviders(IEnumerable<BlockedOrganisationSummary> blockedProviders)
        {
            var blockedProvidersDoc = new BlockedProviderOrganisations
            {
                Id = QueryViewType.BlockedProviderOrganisations.GetIdValue(),
                Data = blockedProviders,
                LastUpdated = _timeProvider.Now
            };

            return _queryStore.UpsertAsync(blockedProvidersDoc);
        }

        public Task UpdateBlockedEmployers(IEnumerable<BlockedOrganisationSummary> blockedEmployers)
        {
            var blockedEmployersDoc = new BlockedEmployerOrganisations
            {
                Id = QueryViewType.BlockedEmployerOrganisations.GetIdValue(),
                Data = blockedEmployers,
                LastUpdated = _timeProvider.Now
            };

            return _queryStore.UpsertAsync(blockedEmployersDoc);
        }

        public Task<BlockedProviderOrganisations> GetBlockedProvidersAsync()
        {
            return _queryStore.GetAsync<BlockedProviderOrganisations>(QueryViewType.BlockedProviderOrganisations.GetIdValue());
        }
    }
}