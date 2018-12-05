using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    public interface IQueryStoreWriter
    {
        Task UpdateEmployerDashboardAsync(string employerAccountId, IEnumerable<VacancySummary> vacancySummaries);
        Task UpdateProviderDashboardAsync(long ukprn, IEnumerable<VacancySummary> vacancySummaries);
        Task UpdateEmployerVacancyDataAsync(string employerAccountId, IEnumerable<LegalEntity> legalEntities);
        Task UpdateProviderVacancyDataAsync(long ukprn, IEnumerable<EmployerInfo> employers);
        Task UpdateLiveVacancyAsync(LiveVacancy vacancy);
        Task DeleteLiveVacancyAsync(long vacancyReference);
        Task RecreateLiveVacancies(IEnumerable<LiveVacancy> liveVacancies);
        Task RecreateClosedVacancies(IEnumerable<ClosedVacancy> closedVacancies);
        Task UpdateVacancyApplicationsAsync(VacancyApplications vacancyApplications);
        Task UpdateQaDashboardAsync(QaDashboard qaDashboard);
        Task UpdateClosedVacancyAsync(ClosedVacancy closedVacancy);
        Task<long> RemoveOldEmployerDashboards(DateTime oldestLastUpdatedDate);
        Task<long> RemoveOldProviderDashboards(DateTime oldestLastUpdatedDate);
        Task UpsertVacancyAnalyticSummaryAsync(VacancyAnalyticsSummary summary);
    }
}