﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System;

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

        public Task<ProviderDashboard> GetProviderDashboardAsync(long ukprn)
        {
            var key = QueryViewType.ProviderDashboard.GetIdValue(ukprn);

            return _queryStore.GetAsync<ProviderDashboard>(QueryViewType.ProviderDashboard.TypeName, key);
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

        public Task UpdateProviderDashboardAsync(long ukprn, IEnumerable<VacancySummary> vacancySummaries)
        {
            var dashboardItem = new ProviderDashboard
            {
                Id = QueryViewType.ProviderDashboard.GetIdValue(ukprn),
                Vacancies = vacancySummaries,
                LastUpdated = _timeProvider.Now
            };

            return _queryStore.UpsertAsync(dashboardItem);
        }

        public Task UpdateEmployerVacancyDataAsync(string employerAccountId, IEnumerable<LegalEntity> legalEntities)
        {
            var employerVacancyDataItem = new EditVacancyInfo
            {
                Id = QueryViewType.EditVacancyInfo.GetIdValue(employerAccountId),
                LegalEntities = legalEntities,
                LastUpdated = _timeProvider.Now
            };

            return _queryStore.UpsertAsync(employerVacancyDataItem);
        }

        public Task UpdateProviderVacancyDataAsync(long ukprn, IEnumerable<EmployerInfo> employers)
        {
            var providerVacancyDataItem = new ProviderEditVacancyInfo
            {
                Id = QueryViewType.EditVacancyInfo.GetIdValue(ukprn),
                Employers = employers,
                LastUpdated = _timeProvider.Now
            };

            return _queryStore.UpsertAsync(providerVacancyDataItem);
        }

        public Task<EditVacancyInfo> GetEmployerVacancyDataAsync(string employerAccountId)
        {
            var key = QueryViewType.EditVacancyInfo.GetIdValue(employerAccountId);

            return _queryStore.GetAsync<EditVacancyInfo>(QueryViewType.EditVacancyInfo.TypeName, key);
        }

        public Task<ProviderEditVacancyInfo> GetProviderVacancyDataAsync(long ukprn)
        {
            var key = QueryViewType.EditVacancyInfo.GetIdValue(ukprn);

            return _queryStore.GetAsync<ProviderEditVacancyInfo>(QueryViewType.EditVacancyInfo.TypeName, key);
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
        
        public Task<IEnumerable<LiveVacancy>> GetLiveVacancies()
        {
            return _queryStore.GetAllByTypeAsync<LiveVacancy>(QueryViewType.LiveVacancy.TypeName);
        }

        public Task DeleteLiveVacancyAsync(long vacancyReference)
        {
            var liveVacancyId = GetLiveVacancyId(vacancyReference);
            return _queryStore.DeleteAsync<LiveVacancy>(QueryViewType.LiveVacancy.TypeName, liveVacancyId);
        }

        public Task RecreateLiveVacancies(IEnumerable<LiveVacancy> liveVacancies)
        {
            return _queryStore.RecreateAsync(QueryViewType.LiveVacancy.TypeName, liveVacancies.ToList());
        }

        public Task RecreateClosedVacancies(IEnumerable<ClosedVacancy> closedVacancies)
        {
            return _queryStore.RecreateAsync(QueryViewType.ClosedVacancy.TypeName, closedVacancies.ToList());
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
            return _queryStore.DeleteManyAsync<EmployerDashboard, DateTime>(QueryViewType.EmployerDashboard.TypeName, x => x.LastUpdated, oldestLastUpdatedDate);
        }

        public Task<long> RemoveOldProviderDashboards(DateTime oldestLastUpdatedDate)
        {
            return _queryStore.DeleteManyAsync<ProviderDashboard, DateTime>(QueryViewType.ProviderDashboard.TypeName, x => x.LastUpdated, oldestLastUpdatedDate);
        }

        private string GetLiveVacancyId(long vacancyReference)
        {
            return QueryViewType.LiveVacancy.GetIdValue(vacancyReference.ToString());
        }
    }
}