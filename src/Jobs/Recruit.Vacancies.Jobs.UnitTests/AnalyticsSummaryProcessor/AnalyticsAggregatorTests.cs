using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using Esfa.Recruit.Vacancies.Jobs.AnalyticsSummaryProcessor;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Recruit.Vacancies.Jobs.UnitTests.AnalyticsSummaryProcessor;

public class AnalyticsAggregatorTests
{
    [Test, MoqAutoData]
    public async Task Then_The_Vacancy_Metrics_Are_Returned_And_Aggregated_To_Existing(
        long vacancyReference,
        VacancyAnalyticsV2QueueMessage metricData,
        VacancyAnalytics summary,
        [Frozen] Mock<IQueryStoreReader> queryStoreReader,
        [Frozen] Mock<IQueryStoreWriter> queryStoreWriter,
        [Frozen] Mock<ITimeProvider> dateTimeService,
        [Frozen] Mock<IOuterApiClient> apiClient,
        AnalyticsAggregator analyticsAggregator)
    {
        metricData.VacancyReference = vacancyReference.ToString();
        summary.AnalyticsDate = new DateTime(2024, 11, 30);
        dateTimeService.Setup(x => x.Now)
            .Returns(new DateTime(2024, 11, 30, 12, 00, 00));
        
        queryStoreReader.Setup(x => x.GetVacancyAnalyticsSummaryV2Async(vacancyReference.ToString())).ReturnsAsync(
            new VacancyAnalyticsSummaryV2
            {
                VacancyReference = vacancyReference.ToString(),
                VacancyAnalytics = [summary]
            });

        var actual = await analyticsAggregator.GetVacancyAnalyticEventSummaryAsync(metricData);

        queryStoreWriter.Verify(
            x => x.UpsertVacancyAnalyticSummaryV2Async(It.Is<VacancyAnalyticsSummaryV2>(c =>
                c.VacancyAnalytics.First(a => a.AnalyticsDate == new DateTime(2024, 11, 30)).ViewsCount == summary.ViewsCount
                && c.VacancyAnalytics.First(a => a.AnalyticsDate == new DateTime(2024, 11, 30)).ApplicationStartedCount == summary.ApplicationStartedCount
                && c.VacancyAnalytics.First(a => a.AnalyticsDate == new DateTime(2024, 11, 30)).ApplicationSubmittedCount == summary.ApplicationSubmittedCount
                && c.VacancyAnalytics.First(a => a.AnalyticsDate == new DateTime(2024, 11, 30)).SearchResultsCount == summary.SearchResultsCount)), Times.Once);

        actual.VacancyReference.Should().Be(vacancyReference);
        actual.NoOfApprenticeshipSaved.Should().Be(0);
        actual.NoOfApprenticeshipSavedSearchAlerts.Should().Be(0);
        actual.NoOfApprenticeshipSearches.Should().Be(summary.SearchResultsCount);
        actual.NoOfApprenticeshipApplicationsCreated.Should().Be(summary.ApplicationStartedCount);
        actual.NoOfApprenticeshipApplicationsSubmitted.Should().Be(summary.ApplicationSubmittedCount);  
        actual.NoOfApprenticeshipDetailsViews.Should().Be(summary.ViewsCount);
    }
    
    [Test]
    [MoqInlineAutoData("")]
    [MoqInlineAutoData("-1")]
    [MoqInlineAutoData("-123")]
    [MoqInlineAutoData("-123444")]
    public async Task Then_The_Vacancy_Metrics_Are_Returned_And_Aggregated_To_Existing_For_ML(
        string mlExtension,
        long vacancyReference,
        VacancyAnalyticsV2QueueMessage metricData,
        VacancyAnalytics summary,
        [Frozen] Mock<IQueryStoreReader> queryStoreReader,
        [Frozen] Mock<IQueryStoreWriter> queryStoreWriter,
        [Frozen] Mock<ITimeProvider> dateTimeService,
        [Frozen] Mock<IOuterApiClient> apiClient,
        AnalyticsAggregator analyticsAggregator)
    {
        metricData.VacancyReference = $"{vacancyReference}{mlExtension}";
        summary.AnalyticsDate = new DateTime(2024, 11, 30);
        dateTimeService.Setup(x => x.Now)
            .Returns(new DateTime(2024, 11, 30, 12, 00, 00));
        
        queryStoreReader.Setup(x => x.GetVacancyAnalyticsSummaryV2Async(vacancyReference.ToString())).ReturnsAsync(
            new VacancyAnalyticsSummaryV2
            {
                VacancyReference = vacancyReference.ToString(),
                VacancyAnalytics = [summary]
            });

        var actual = await analyticsAggregator.GetVacancyAnalyticEventSummaryAsync(metricData);

        queryStoreWriter.Verify(
            x => x.UpsertVacancyAnalyticSummaryV2Async(It.Is<VacancyAnalyticsSummaryV2>(c =>
                c.VacancyAnalytics.First(a => a.AnalyticsDate == new DateTime(2024, 11, 30)).ViewsCount == summary.ViewsCount
                && c.VacancyAnalytics.First(a => a.AnalyticsDate == new DateTime(2024, 11, 30)).ApplicationStartedCount == summary.ApplicationStartedCount
                && c.VacancyAnalytics.First(a => a.AnalyticsDate == new DateTime(2024, 11, 30)).ApplicationSubmittedCount == summary.ApplicationSubmittedCount
                && c.VacancyAnalytics.First(a => a.AnalyticsDate == new DateTime(2024, 11, 30)).SearchResultsCount == summary.SearchResultsCount)), Times.Once);

        actual.VacancyReference.Should().Be(vacancyReference);
        actual.NoOfApprenticeshipSaved.Should().Be(0);
        actual.NoOfApprenticeshipSavedSearchAlerts.Should().Be(0);
        actual.NoOfApprenticeshipSearches.Should().Be(summary.SearchResultsCount);
        actual.NoOfApprenticeshipApplicationsCreated.Should().Be(summary.ApplicationStartedCount);
        actual.NoOfApprenticeshipApplicationsSubmitted.Should().Be(summary.ApplicationSubmittedCount);  
        actual.NoOfApprenticeshipDetailsViews.Should().Be(summary.ViewsCount);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Vacancy_Metrics_Are_Returned_And_New_Created_If_Not_Exists(
        long vacancyReference,
        VacancyAnalyticsV2QueueMessage metricData,
        VacancyAnalytics summary,
        [Frozen] Mock<IQueryStoreReader> queryStoreReader,
        [Frozen] Mock<IQueryStoreWriter> queryStoreWriter,
        [Frozen] Mock<ITimeProvider> dateTimeService,
        [Frozen] Mock<IOuterApiClient> apiClient,
        AnalyticsAggregator analyticsAggregator)
    {
        metricData.VacancyReference = vacancyReference.ToString();
        summary.AnalyticsDate = new DateTime(2024, 11, 30);
        dateTimeService.Setup(x => x.Now)
            .Returns(new DateTime(2024, 11, 30, 12, 00, 00));
        queryStoreReader.Setup(x => x.GetVacancyAnalyticsSummaryV2Async(vacancyReference.ToString())).ReturnsAsync((VacancyAnalyticsSummaryV2)null);

        var actual = await analyticsAggregator.GetVacancyAnalyticEventSummaryAsync(metricData);

        queryStoreWriter.Verify(
            x => x.UpsertVacancyAnalyticSummaryV2Async(It.Is<VacancyAnalyticsSummaryV2>(c =>
                c.VacancyAnalytics.First(a=>a.AnalyticsDate == new DateTime(2024, 11, 30)).ViewsCount == metricData.ViewsCount
                && c.VacancyAnalytics.First(a=>a.AnalyticsDate == new DateTime(2024, 11, 30)).ApplicationStartedCount == metricData.ApplicationStartedCount
                && c.VacancyAnalytics.First(a=>a.AnalyticsDate == new DateTime(2024, 11, 30)).ApplicationSubmittedCount == metricData.ApplicationSubmittedCount
                && c.VacancyAnalytics.First(a=>a.AnalyticsDate == new DateTime(2024, 11, 30)).SearchResultsCount == metricData.SearchResultsCount)), Times.Once);

        actual.VacancyReference.Should().Be(vacancyReference);
        actual.NoOfApprenticeshipSaved.Should().Be(0);
        actual.NoOfApprenticeshipSavedSearchAlerts.Should().Be(0);
        actual.NoOfApprenticeshipSearches.Should().Be(metricData.SearchResultsCount);
        actual.NoOfApprenticeshipApplicationsCreated.Should().Be(metricData.ApplicationStartedCount);
        actual.NoOfApprenticeshipApplicationsSubmitted.Should().Be(metricData.ApplicationSubmittedCount);
        actual.NoOfApprenticeshipDetailsViews.Should().Be(metricData.ViewsCount);
    }   
    [Test, MoqAutoData]
    public async Task Then_The_VacancySummary_Returns_All_Totals(
        long vacancyReference,
        VacancyAnalyticsV2QueueMessage metricData,
        VacancyAnalytics summary1,
        VacancyAnalytics summary2,
        VacancyAnalytics summary3,
        VacancyAnalytics summary4,
        VacancyAnalytics summary5,
        VacancyAnalytics summary6,
        VacancyAnalytics summary7,
        [Frozen] Mock<IQueryStoreReader> queryStoreReader,
        [Frozen] Mock<IQueryStoreWriter> queryStoreWriter,
        [Frozen] Mock<ITimeProvider> dateTimeService,
        [Frozen] Mock<IOuterApiClient> apiClient,
        AnalyticsAggregator analyticsAggregator)
    {
        summary1.AnalyticsDate = new DateTime(2024, 11, 29);
        summary2.AnalyticsDate = new DateTime(2024, 11, 28);
        summary3.AnalyticsDate = new DateTime(2024, 11, 27);
        summary4.AnalyticsDate = new DateTime(2024, 11, 26);
        summary5.AnalyticsDate = new DateTime(2024, 11, 25);
        summary6.AnalyticsDate = new DateTime(2024, 11, 24);
        summary7.AnalyticsDate = new DateTime(2024, 11, 23);
        dateTimeService.Setup(x => x.Now)
            .Returns(new DateTime(2024, 11, 30, 12, 00, 00));
        metricData.VacancyReference = vacancyReference.ToString();
        metricData.ApplicationStartedCount = summary1.ApplicationStartedCount;
        metricData.ApplicationSubmittedCount = summary1.ApplicationSubmittedCount;
        metricData.ViewsCount = summary1.ViewsCount;
        metricData.SearchResultsCount = summary1.SearchResultsCount;
        List<VacancyAnalytics> vacancyAnalyticsList = [summary1, summary2, summary3, summary4, summary5, summary6, summary7];
        queryStoreReader.Setup(x => x.GetVacancyAnalyticsSummaryV2Async(vacancyReference.ToString())).ReturnsAsync(
            new VacancyAnalyticsSummaryV2
            {
                VacancyReference = vacancyReference.ToString(),
                VacancyAnalytics = vacancyAnalyticsList
            });

        var actual = await analyticsAggregator.GetVacancyAnalyticEventSummaryAsync(metricData);

        queryStoreWriter.Verify(
            x => x.UpsertVacancyAnalyticSummaryV2Async(It.Is<VacancyAnalyticsSummaryV2>(c =>
                c.VacancyAnalytics.First(a=>a.AnalyticsDate == new DateTime(2024, 11, 30)).ViewsCount == summary1.ViewsCount
                && c.VacancyAnalytics.First(a=>a.AnalyticsDate == new DateTime(2024, 11, 30)).ApplicationStartedCount == summary1.ApplicationStartedCount
                && c.VacancyAnalytics.First(a=>a.AnalyticsDate == new DateTime(2024, 11, 30)).ApplicationSubmittedCount == summary1.ApplicationSubmittedCount
                && c.VacancyAnalytics.First(a=>a.AnalyticsDate == new DateTime(2024, 11, 30)).SearchResultsCount == summary1.SearchResultsCount)), Times.Once);

        actual.VacancyReference.Should().Be(vacancyReference);
        actual.NoOfApprenticeshipSaved.Should().Be(0);
        actual.NoOfApprenticeshipSavedSearchAlerts.Should().Be(0);
        actual.NoOfApprenticeshipSearches.Should().Be(vacancyAnalyticsList.Sum(c => c.SearchResultsCount));
        actual.NoOfApprenticeshipDetailsViews.Should().Be(vacancyAnalyticsList.Sum(c => c.ViewsCount));
        actual.NoOfApprenticeshipApplicationsCreated.Should().Be(vacancyAnalyticsList.Sum(c => c.ApplicationStartedCount));
        actual.NoOfApprenticeshipApplicationsSubmitted.Should().Be(vacancyAnalyticsList.Sum(c => c.ApplicationSubmittedCount));
        
        actual.NoOfApprenticeshipSearchesOneDayAgo.Should().Be(summary1.SearchResultsCount);
        actual.NoOfApprenticeshipApplicationsCreatedOneDayAgo.Should().Be(summary1.ApplicationStartedCount);
        actual.NoOfApprenticeshipApplicationsSubmittedOneDayAgo.Should().Be(summary1.ApplicationSubmittedCount);
        actual.NoOfApprenticeshipDetailsViewsOneDayAgo.Should().Be(summary1.ViewsCount);
        actual.NoOfApprenticeshipSavedOneDayAgo.Should().Be(0);
        actual.NoOfApprenticeshipSavedSearchAlertsOneDayAgo.Should().Be(0);
        
        actual.NoOfApprenticeshipSearchesTwoDaysAgo.Should().Be(summary2.SearchResultsCount);
        actual.NoOfApprenticeshipApplicationsCreatedTwoDaysAgo.Should().Be(summary2.ApplicationStartedCount);
        actual.NoOfApprenticeshipApplicationsSubmittedTwoDaysAgo.Should().Be(summary2.ApplicationSubmittedCount);
        actual.NoOfApprenticeshipDetailsViewsTwoDaysAgo.Should().Be(summary2.ViewsCount);
        actual.NoOfApprenticeshipSavedTwoDaysAgo.Should().Be(0);
        actual.NoOfApprenticeshipSavedSearchAlertsTwoDaysAgo.Should().Be(0);
        
        actual.NoOfApprenticeshipSearchesThreeDaysAgo.Should().Be(summary3.SearchResultsCount);
        actual.NoOfApprenticeshipApplicationsCreatedThreeDaysAgo.Should().Be(summary3.ApplicationStartedCount);
        actual.NoOfApprenticeshipApplicationsSubmittedThreeDaysAgo.Should().Be(summary3.ApplicationSubmittedCount);
        actual.NoOfApprenticeshipDetailsViewsThreeDaysAgo.Should().Be(summary3.ViewsCount);
        actual.NoOfApprenticeshipSavedThreeDaysAgo.Should().Be(0);
        actual.NoOfApprenticeshipSavedSearchAlertsThreeDaysAgo.Should().Be(0);
        
        actual.NoOfApprenticeshipSearchesFourDaysAgo.Should().Be(summary4.SearchResultsCount);
        actual.NoOfApprenticeshipApplicationsCreatedFourDaysAgo.Should().Be(summary4.ApplicationStartedCount);
        actual.NoOfApprenticeshipApplicationsSubmittedFourDaysAgo.Should().Be(summary4.ApplicationSubmittedCount);
        actual.NoOfApprenticeshipDetailsViewsFourDaysAgo.Should().Be(summary4.ViewsCount);
        actual.NoOfApprenticeshipSavedFourDaysAgo.Should().Be(0);
        actual.NoOfApprenticeshipSavedSearchAlertsFourDaysAgo.Should().Be(0);
        
        actual.NoOfApprenticeshipSearchesFiveDaysAgo.Should().Be(summary5.SearchResultsCount);
        actual.NoOfApprenticeshipApplicationsCreatedFiveDaysAgo.Should().Be(summary5.ApplicationStartedCount);
        actual.NoOfApprenticeshipApplicationsSubmittedFiveDaysAgo.Should().Be(summary5.ApplicationSubmittedCount);
        actual.NoOfApprenticeshipDetailsViewsFiveDaysAgo.Should().Be(summary5.ViewsCount);
        actual.NoOfApprenticeshipSavedFiveDaysAgo.Should().Be(0);
        actual.NoOfApprenticeshipSavedSearchAlertsFiveDaysAgo.Should().Be(0);
        
        actual.NoOfApprenticeshipSearchesSixDaysAgo.Should().Be(summary6.SearchResultsCount);
        actual.NoOfApprenticeshipApplicationsCreatedSixDaysAgo.Should().Be(summary6.ApplicationStartedCount);
        actual.NoOfApprenticeshipApplicationsSubmittedSixDaysAgo.Should().Be(summary6.ApplicationSubmittedCount);
        actual.NoOfApprenticeshipDetailsViewsSixDaysAgo.Should().Be(summary6.ViewsCount);
        actual.NoOfApprenticeshipSavedSixDaysAgo.Should().Be(0);
        actual.NoOfApprenticeshipSavedSearchAlertsSixDaysAgo.Should().Be(0);
        
        actual.NoOfApprenticeshipSearchesSevenDaysAgo.Should().Be(summary7.SearchResultsCount);
        actual.NoOfApprenticeshipApplicationsCreatedSevenDaysAgo.Should().Be(summary7.ApplicationStartedCount);
        actual.NoOfApprenticeshipApplicationsSubmittedSevenDaysAgo.Should().Be(summary7.ApplicationSubmittedCount);
        actual.NoOfApprenticeshipDetailsViewsSevenDaysAgo.Should().Be(summary7.ViewsCount);
        actual.NoOfApprenticeshipSavedSevenDaysAgo.Should().Be(0);
        actual.NoOfApprenticeshipSavedSearchAlertsSevenDaysAgo.Should().Be(0);
    }

    [Test, MoqAutoData]
    public async Task Then_The_Vacancies_Are_Returned_With_Analytics_For_The_Past_Hour(
        GetVacanciesWithMetricsResponse apiResponse,
        [Frozen] Mock<ITimeProvider> dateTimeService,
        [Frozen] Mock<IOuterApiClient> apiClient,
        AnalyticsAggregator analyticsAggregator)
    {
        dateTimeService.Setup(x => x.Now)
            .Returns(new DateTime(2024, 11, 30, 12, 00, 00));
        var expectedGetUrl = new GetListOfVacanciesWithMetricsRequest( 
            new DateTime(2024, 11, 30, 11, 00, 00),
            new DateTime(2024, 11, 30, 12, 00, 00));
        apiClient.Setup(c =>
                c.Get<GetVacanciesWithMetricsResponse>(
                    It.Is<GetListOfVacanciesWithMetricsRequest>(x => x.GetUrl == expectedGetUrl.GetUrl)))
            .ReturnsAsync(apiResponse);

        var actual = await analyticsAggregator.GetVacanciesWithAnalyticsInThePastHour();

        actual.Should().BeEquivalentTo(apiResponse.VacancyMetrics);
    }
}