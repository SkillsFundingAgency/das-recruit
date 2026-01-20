using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;

public interface IQueryStoreReader
{
    Task<EmployerEditVacancyInfo> GetEmployerVacancyDataAsync(string employerAccountId);
    Task<ProviderEditVacancyInfo> GetProviderVacancyDataAsync(long ukprn);
    Task<EmployerInfo> GetProviderEmployerVacancyDataAsync(long ukprn, string employerAccountId);
    Task<IEnumerable<EmployerInfo>> GetProviderEmployerVacancyDatasAsync(long ukprn, IList<string> employerAccountIds);
    Task<VacancyAnalyticsSummary> GetVacancyAnalyticsSummaryAsync(long vacancyReference);
    Task<IEnumerable<LiveVacancy>> GetLiveExpiredVacancies(DateTime closingDate);
    Task<ClosedVacancy> GetClosedVacancy(long vacancyReference);
    Task<IEnumerable<LiveVacancy>> GetAllLiveVacancies(int vacanciesToSkip, int vacanciesToGet);
    Task<IEnumerable<LiveVacancy>> GetAllLiveVacanciesOnClosingDate(int vacanciesToSkip, int vacanciesToGet, DateTime closingDate);
    Task<long> GetAllLiveVacanciesCount();
    Task<long> GetAllLiveVacanciesOnClosingDateCount(DateTime closingDate);
    Task<long> GetTotalPositionsAvailableCount();
    Task<LiveVacancy> GetLiveVacancy(long vacancyReference);
    Task<VacancyAnalyticsSummaryV2> GetVacancyAnalyticsSummaryV2Async(string vacancyReference);
}