using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;

namespace Esfa.Recruit.Vacancies.Jobs.AnalyticsSummaryProcessor;

public interface IAnalyticsAggregator
{
    Task<VacancyAnalyticsSummary> GetVacancyAnalyticEventSummaryAsync(long vacancyReference);
    Task<List<long>> GetVacanciesWithAnalyticsInThePastHour();
}
public class AnalyticsAggregator(IOuterApiClient apiClient, ITimeProvider timeProvider, IQueryStoreReader queryStoreReader, IQueryStoreWriter queryStoreWriter) : IAnalyticsAggregator
{
    public async Task<VacancyAnalyticsSummary> GetVacancyAnalyticEventSummaryAsync(long vacancyReference)
    {
        var endDate = timeProvider.Now;
        var startDate = new DateTime(endDate.Year, endDate.Month, endDate.Day);
        var apiResponse = await apiClient.Get<GetVacancyMetricResponse>(
            new GetVacancyMetricsRequest(vacancyReference, startDate, endDate));

        var metrics = await queryStoreReader.GetVacancyAnalyticsSummaryV2Async(vacancyReference);
        if (metrics.VacancyAnalytics.FirstOrDefault(c=>c.AnalyticsDate == startDate)!= null)
        {
            metrics.VacancyAnalytics.First(c=>c.AnalyticsDate == startDate).ApplicationStartedCount = apiResponse.ApplicationStartedCount;
            metrics.VacancyAnalytics.First(c=>c.AnalyticsDate == startDate).ViewsCount = apiResponse.ViewsCount;
            metrics.VacancyAnalytics.First(c=>c.AnalyticsDate == startDate).ApplicationSubmittedCount = apiResponse.ApplicationSubmittedCount;
            metrics.VacancyAnalytics.First(c=>c.AnalyticsDate == startDate).SearchResultsCount = apiResponse.SearchResultsCount;
        }
        else
        {
            metrics.VacancyAnalytics.Add(new VacancyAnalytics
            {
                AnalyticsDate = startDate,
                ApplicationStartedCount = apiResponse.ApplicationStartedCount,
                ViewsCount = apiResponse.ViewsCount,
                ApplicationSubmittedCount = apiResponse.ApplicationSubmittedCount,
                SearchResultsCount = apiResponse.SearchResultsCount
            });
        }

        await queryStoreWriter.UpsertVacancyAnalyticSummaryV2Async(metrics);

        var oneDayAgoTotals = metrics.VacancyAnalytics.Where(c => c.AnalyticsDate == startDate.AddDays(-1)).ToList();
        var twoDaysAgoTotals = metrics.VacancyAnalytics.Where(c => c.AnalyticsDate == startDate.AddDays(-2)).ToList();
        var threeDaysAgoTotals = metrics.VacancyAnalytics.Where(c => c.AnalyticsDate == startDate.AddDays(-3)).ToList();
        var fourDaysAgoTotals = metrics.VacancyAnalytics.Where(c => c.AnalyticsDate == startDate.AddDays(-4)).ToList();
        var fiveDaysAgoTotals = metrics.VacancyAnalytics.Where(c => c.AnalyticsDate == startDate.AddDays(-5)).ToList();
        var sixDaysAgoTotals = metrics.VacancyAnalytics.Where(c => c.AnalyticsDate == startDate.AddDays(-6)).ToList();
        var sevenDaysAgoTotals = metrics.VacancyAnalytics.Where(c => c.AnalyticsDate == startDate.AddDays(-7)).ToList();
        
        return new VacancyAnalyticsSummary
        {
            VacancyReference = vacancyReference,
            NoOfApprenticeshipSearches = metrics.VacancyAnalytics.Sum(c=>c.SearchResultsCount),
            NoOfApprenticeshipSearchesSevenDaysAgo = sevenDaysAgoTotals.Sum(c=>c.SearchResultsCount),
            NoOfApprenticeshipSearchesSixDaysAgo = sixDaysAgoTotals.Sum(c=>c.SearchResultsCount),
            NoOfApprenticeshipSearchesFiveDaysAgo = fiveDaysAgoTotals.Sum(c=>c.SearchResultsCount),
            NoOfApprenticeshipSearchesFourDaysAgo = fourDaysAgoTotals.Sum(c=>c.SearchResultsCount),
            NoOfApprenticeshipSearchesThreeDaysAgo = threeDaysAgoTotals.Sum(c=>c.SearchResultsCount),
            NoOfApprenticeshipSearchesTwoDaysAgo = twoDaysAgoTotals.Sum(c=>c.SearchResultsCount),
            NoOfApprenticeshipSearchesOneDayAgo = oneDayAgoTotals.Sum(c=>c.SearchResultsCount),
            
            //TODO implemented post MVS
            NoOfApprenticeshipSavedSearchAlerts = 0,
            NoOfApprenticeshipSavedSearchAlertsSevenDaysAgo = 0,
            NoOfApprenticeshipSavedSearchAlertsSixDaysAgo = 0,
            NoOfApprenticeshipSavedSearchAlertsFiveDaysAgo = 0,
            NoOfApprenticeshipSavedSearchAlertsFourDaysAgo = 0,
            NoOfApprenticeshipSavedSearchAlertsThreeDaysAgo = 0,
            NoOfApprenticeshipSavedSearchAlertsTwoDaysAgo = 0,
            NoOfApprenticeshipSavedSearchAlertsOneDayAgo = 0,
            
            //TODO implement post MVS
            NoOfApprenticeshipSaved = 0, 
            NoOfApprenticeshipSavedSevenDaysAgo = 0,
            NoOfApprenticeshipSavedSixDaysAgo = 0,
            NoOfApprenticeshipSavedFiveDaysAgo = 0,
            NoOfApprenticeshipSavedFourDaysAgo = 0,
            NoOfApprenticeshipSavedThreeDaysAgo = 0,
            NoOfApprenticeshipSavedTwoDaysAgo = 0,
            NoOfApprenticeshipSavedOneDayAgo = 0,
            
            NoOfApprenticeshipDetailsViews = metrics.VacancyAnalytics.Sum(c=>c.ViewsCount),
            NoOfApprenticeshipDetailsViewsSevenDaysAgo = sevenDaysAgoTotals.Sum(c=>c.ViewsCount),
            NoOfApprenticeshipDetailsViewsSixDaysAgo = sixDaysAgoTotals.Sum(c=>c.ViewsCount),
            NoOfApprenticeshipDetailsViewsFiveDaysAgo = fiveDaysAgoTotals.Sum(c=>c.ViewsCount),
            NoOfApprenticeshipDetailsViewsFourDaysAgo = fourDaysAgoTotals.Sum(c=>c.ViewsCount),
            NoOfApprenticeshipDetailsViewsThreeDaysAgo = threeDaysAgoTotals.Sum(c=>c.ViewsCount),
            NoOfApprenticeshipDetailsViewsTwoDaysAgo = twoDaysAgoTotals.Sum(c=>c.ViewsCount),
            NoOfApprenticeshipDetailsViewsOneDayAgo = oneDayAgoTotals.Sum(c=>c.ViewsCount),
            
            NoOfApprenticeshipApplicationsCreated = metrics.VacancyAnalytics.Sum(c=>c.ApplicationStartedCount),
            NoOfApprenticeshipApplicationsCreatedSevenDaysAgo =  sevenDaysAgoTotals.Sum(c=>c.ApplicationStartedCount),
            NoOfApprenticeshipApplicationsCreatedSixDaysAgo = sixDaysAgoTotals.Sum(c=>c.ApplicationStartedCount),
            NoOfApprenticeshipApplicationsCreatedFiveDaysAgo = fiveDaysAgoTotals.Sum(c=>c.ApplicationStartedCount),
            NoOfApprenticeshipApplicationsCreatedFourDaysAgo = fourDaysAgoTotals.Sum(c=>c.ApplicationStartedCount),
            NoOfApprenticeshipApplicationsCreatedThreeDaysAgo = threeDaysAgoTotals.Sum(c=>c.ApplicationStartedCount),
            NoOfApprenticeshipApplicationsCreatedTwoDaysAgo = twoDaysAgoTotals.Sum(c=>c.ApplicationStartedCount),
            NoOfApprenticeshipApplicationsCreatedOneDayAgo = oneDayAgoTotals.Sum(c=>c.ApplicationStartedCount),
            
            NoOfApprenticeshipApplicationsSubmitted = metrics.VacancyAnalytics.Sum(c=>c.ApplicationSubmittedCount),
            NoOfApprenticeshipApplicationsSubmittedSevenDaysAgo = sevenDaysAgoTotals.Sum(c=>c.ApplicationSubmittedCount),
            NoOfApprenticeshipApplicationsSubmittedSixDaysAgo = sixDaysAgoTotals.Sum(c=>c.ApplicationSubmittedCount),
            NoOfApprenticeshipApplicationsSubmittedFiveDaysAgo = fiveDaysAgoTotals.Sum(c=>c.ApplicationSubmittedCount),
            NoOfApprenticeshipApplicationsSubmittedFourDaysAgo = fourDaysAgoTotals.Sum(c=>c.ApplicationSubmittedCount),
            NoOfApprenticeshipApplicationsSubmittedThreeDaysAgo = threeDaysAgoTotals.Sum(c=>c.ApplicationSubmittedCount),
            NoOfApprenticeshipApplicationsSubmittedTwoDaysAgo = twoDaysAgoTotals.Sum(c=>c.ApplicationSubmittedCount),
            NoOfApprenticeshipApplicationsSubmittedOneDayAgo = oneDayAgoTotals.Sum(c=>c.ApplicationSubmittedCount)
        };

    }

    public async Task<List<long>> GetVacanciesWithAnalyticsInThePastHour()
    {
        var endDate = timeProvider.Now;
        var startDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, endDate.Hour -1, 0,0);
        var apiResponse = await apiClient.Get<GetVacanciesWithMetricsResponse>(
            new GetListOfVacanciesWithMetricsRequest(startDate, endDate));

        return apiResponse.Vacancies;
    }
}