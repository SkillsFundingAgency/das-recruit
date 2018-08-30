using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.LiveVacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

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
        
        public Task<EditVacancyInfo> GetEmployerVacancyDataAsync(string employerAccountId)
        {
            var key = QueryViewType.EditVacancyInfo.GetIdValue(employerAccountId);

            return _queryStore.GetAsync<EditVacancyInfo>(QueryViewType.EditVacancyInfo.TypeName, key);
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
            return _queryStore.RecreateAsync(liveVacancies.ToList());
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

        private string GetLiveVacancyId(long vacancyReference)
        {
            return QueryViewType.LiveVacancy.GetIdValue(vacancyReference.ToString());
        }
    }
}