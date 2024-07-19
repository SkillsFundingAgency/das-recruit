using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.BlockedOrganisations;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    public interface IQueryStoreReader
    {
        Task<EmployerDashboard> GetEmployerDashboardAsync(string employerAccountId);
        Task<EmployerEditVacancyInfo> GetEmployerVacancyDataAsync(string employerAccountId);
        Task<ProviderEditVacancyInfo> GetProviderVacancyDataAsync(long ukprn);
        Task<EmployerInfo> GetProviderEmployerVacancyDataAsync(long ukprn, string employerAccountId);
        Task<IEnumerable<EmployerInfo>> GetProviderEmployerVacancyDatasAsync(long ukprn, IList<string> employerAccountIds);
        Task<VacancyApplications> GetVacancyApplicationsAsync(string vacancyReference);
        Task<QaDashboard> GetQaDashboardAsync();
        Task<ProviderDashboard> GetProviderDashboardAsync(long ukprn, VacancyType vacancyType);
        Task<VacancyAnalyticsSummary> GetVacancyAnalyticsSummaryAsync(long vacancyReference);
        Task<BlockedProviderOrganisations> GetBlockedProvidersAsync();
        Task<IEnumerable<LiveVacancy>> GetLiveExpiredVacancies(DateTime closingDate);
        Task<ClosedVacancy> GetClosedVacancy(long vacancyReference);
        Task<IEnumerable<ClosedVacancy>> GetClosedVacancies(IList<long> vacancyReferences);
        Task<IEnumerable<LiveVacancy>> GetAllLiveVacancies(int vacanciesToSkip, int vacanciesToGet);
        Task<IEnumerable<LiveVacancy>> GetAllLiveVacanciesOnClosingDate(int vacanciesToSkip, int vacanciesToGet, DateTime closingDate);
        Task<long> GetAllLiveVacanciesCount();
        Task<long> GetAllLiveVacanciesOnClosingDateCount(DateTime closingDate);
        Task<long> GetTotalPositionsAvailableCount();
        Task<LiveVacancy> GetLiveVacancy(long vacancyReference);
        Task<LiveVacancy> GetLiveExpiredVacancy(long vacancyReference);
        Task<VacancyAnalyticsSummaryV2> GetVacancyAnalyticsSummaryV2Async(long vacancyReference);
    }
}