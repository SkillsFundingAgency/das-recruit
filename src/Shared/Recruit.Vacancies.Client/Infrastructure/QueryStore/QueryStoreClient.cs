using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Dashboard;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.LiveVacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Models;

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

        public Task<Dashboard> GetDashboardAsync(string employerAccountId)
        {
            var key = QueryViewType.Dashboard.GetIdValue(employerAccountId);

            return _queryStore.GetAsync<Dashboard>(key);
        }

        public Task UpdateDashboardAsync(string employerAccountId, IEnumerable<VacancySummary> vacancySummaries)
        {
            var dashboardItem = new Dashboard
            {
                Id = QueryViewType.Dashboard.GetIdValue(employerAccountId),
                ViewType = QueryViewType.Dashboard.TypeName,
                Vacancies = vacancySummaries,
                LastUpdated = _timeProvider.Now
            };

            return _queryStore.UpsertAsync(dashboardItem);
        }

        public Task UpdateApprenticeshipProgrammesAsync(IEnumerable<ApprenticeshipProgramme> programmes)
        {
            var programmesItem = new ApprenticeshipProgrammes
            {
                Id = QueryViewType.ApprenticeshipProgrammes.GetIdValue(),
                ViewType = QueryViewType.ApprenticeshipProgrammes.TypeName,
                Programmes = programmes,
                LastUpdated = _timeProvider.Now
            };
            
            return _queryStore.UpsertAsync(programmesItem);
        }

        public async Task<ApprenticeshipProgrammes> GetApprenticeshipProgrammesAsync()
        {
            var key = QueryViewType.ApprenticeshipProgrammes.GetIdValue();

            var storeItem = await _queryStore.GetAsync<ApprenticeshipProgrammes>(key);

            return storeItem;
        }

        public Task UpdateEmployerVacancyDataAsync(string employerAccountId, IEnumerable<LegalEntity> legalEntities)
        {
            var employerVacancyDataItem = new EditVacancyInfo
            {
                Id = QueryViewType.EditVacancyInfo.GetIdValue(employerAccountId),
                ViewType = QueryViewType.EditVacancyInfo.TypeName,
                LegalEntities = legalEntities,
                LastUpdated = _timeProvider.Now
            };

            return _queryStore.UpsertAsync(employerVacancyDataItem);
        }
        
        public Task<EditVacancyInfo> GetEmployerVacancyDataAsync(string employerAccountId)
        {
            var key = QueryViewType.EditVacancyInfo.GetIdValue(employerAccountId);

            return _queryStore.GetAsync<EditVacancyInfo>(key);
        }

        public Task<VacancyApplications> GetVacancyApplicationsAsync(string vacancyReference)
        {
            var key = QueryViewType.VacancyApplications.GetIdValue(vacancyReference);

            return _queryStore.GetAsync<VacancyApplications>(key);
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
            return _queryStore.DeleteAsync<LiveVacancy>(liveVacancyId);
        }

        public Task RefreshLiveVacancies(IEnumerable<LiveVacancy> liveVacancies)
        {
            return _queryStore.RefreshAllAsync(liveVacancies);
        }

        public Task UpdateVacancyApplicationsAsync(VacancyApplications vacancyApplications)
        {
            vacancyApplications.Id = QueryViewType.VacancyApplications.GetIdValue(vacancyApplications.VacancyReference.ToString());
            vacancyApplications.ViewType = QueryViewType.VacancyApplications.TypeName;
            vacancyApplications.LastUpdated = _timeProvider.Now;

            return _queryStore.UpsertAsync(vacancyApplications);
        }

        public Task<QaDashboard> GetQaDashboardAsync()
        {
            var key = QueryViewType.QaDashboard.GetIdValue();

            return _queryStore.GetAsync<QaDashboard>(key);
        }

        public Task UpdateQaDashboardAsync(QaDashboard qaDashboard)
        {
            qaDashboard.Id = QueryViewType.QaDashboard.GetIdValue();
            qaDashboard.ViewType = QueryViewType.QaDashboard.TypeName;
            qaDashboard.LastUpdated = _timeProvider.Now;

            return _queryStore.UpsertAsync(qaDashboard);
        }

        private string GetLiveVacancyId(long vacancyReference)
        {
            return QueryViewType.LiveVacancy.GetIdValue(vacancyReference.ToString());
        }
    }
}