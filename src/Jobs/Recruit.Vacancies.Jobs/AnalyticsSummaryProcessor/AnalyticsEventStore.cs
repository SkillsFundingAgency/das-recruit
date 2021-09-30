using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Esfa.Recruit.Vacancies.Jobs.AnalyticsSummaryProcessor
{
    public class AnalyticsEventStore
    {
        private const string GetVacancyAnalyticEventsSqlSproc = "[VACANCY].[Event_GET_EventsSummaryForVacancy]";
        private const int VacancyReferenceColumnIndex = 0;

        private const int NoOfApprenticeshipSearchesColumnIndex = 1;
        private const int NoOfApprenticeshipSearchesSevenDaysAgoColumnIndex = 2;
        private const int NoOfApprenticeshipSearchesSixDaysAgoColumnIndex = 3;
        private const int NoOfApprenticeshipSearchesFiveDaysAgoColumnIndex = 4;
        private const int NoOfApprenticeshipSearchesFourDaysAgoColumnIndex = 5;
        private const int NoOfApprenticeshipSearchesThreeDaysAgoColumnIndex = 6;
        private const int NoOfApprenticeshipSearchesTwoDaysAgoColumnIndex = 7;
        private const int NoOfApprenticeshipSearchesOneDayAgoColumnIndex = 8;

        private const int NoOfApprenticeshipSavedSearchAlertsColumnIndex = 9;
        private const int NoOfApprenticeshipSavedSearchAlertsSevenDaysAgoColumnIndex = 10;
        private const int NoOfApprenticeshipSavedSearchAlertsSixDaysAgoColumnIndex = 11;
        private const int NoOfApprenticeshipSavedSearchAlertsFiveDaysAgoColumnIndex = 12;
        private const int NoOfApprenticeshipSavedSearchAlertsFourDaysAgoColumnIndex = 13;
        private const int NoOfApprenticeshipSavedSearchAlertsThreeDaysAgoColumnIndex = 14;
        private const int NoOfApprenticeshipSavedSearchAlertsTwoDaysAgoColumnIndex = 15;
        private const int NoOfApprenticeshipSavedSearchAlertsOneDayAgoColumnIndex = 16;

        private const int NoOfApprenticeshipSavedColumnIndex = 17;
        private const int NoOfApprenticeshipSavedSevenDaysAgoColumnIndex = 18;
        private const int NoOfApprenticeshipSavedSixDaysAgoColumnIndex = 19;
        private const int NoOfApprenticeshipSavedFiveDaysAgoColumnIndex = 20;
        private const int NoOfApprenticeshipSavedFourDaysAgoColumnIndex = 21;
        private const int NoOfApprenticeshipSavedThreeDaysAgoColumnIndex = 22;
        private const int NoOfApprenticeshipSavedTwoDaysAgoColumnIndex = 23;
        private const int NoOfApprenticeshipSavedOneDayAgoColumnIndex = 24;

        private const int NoOfApprenticeshipDetailsViewsColumnIndex = 25;
        private const int NoOfApprenticeshipDetailsViewsSevenDaysAgoColumnIndex = 26;
        private const int NoOfApprenticeshipDetailsViewsSixDaysAgoColumnIndex = 27;
        private const int NoOfApprenticeshipDetailsViewsFiveDaysAgoColumnIndex = 28;
        private const int NoOfApprenticeshipDetailsViewsFourDaysAgoColumnIndex = 29;
        private const int NoOfApprenticeshipDetailsViewsThreeDaysAgoColumnIndex = 30;
        private const int NoOfApprenticeshipDetailsViewsTwoDaysAgoColumnIndex = 31;
        private const int NoOfApprenticeshipDetailsViewsOneDayAgoColumnIndex = 32;

        private const int NoOfApprenticeshipApplicationsCreatedColumnIndex = 33;
        private const int NoOfApprenticeshipApplicationsCreatedSevenDaysAgoColumnIndex = 34;
        private const int NoOfApprenticeshipApplicationsCreatedSixDaysAgoColumnIndex = 35;
        private const int NoOfApprenticeshipApplicationsCreatedFiveDaysAgoColumnIndex = 36;
        private const int NoOfApprenticeshipApplicationsCreatedFourDaysAgoColumnIndex = 37;
        private const int NoOfApprenticeshipApplicationsCreatedThreeDaysAgoColumnIndex = 38;
        private const int NoOfApprenticeshipApplicationsCreatedTwoDaysAgoColumnIndex = 39;
        private const int NoOfApprenticeshipApplicationsCreatedOneDayAgoColumnIndex = 40;

        private const int NoOfApprenticeshipApplicationsSubmittedColumnIndex = 41;
        private const int NoOfApprenticeshipApplicationsSubmittedSevenDaysAgoColumnIndex = 42;
        private const int NoOfApprenticeshipApplicationsSubmittedSixDaysAgoColumnIndex = 43;
        private const int NoOfApprenticeshipApplicationsSubmittedFiveDaysAgoColumnIndex = 44;
        private const int NoOfApprenticeshipApplicationsSubmittedFourDaysAgoColumnIndex = 45;
        private const int NoOfApprenticeshipApplicationsSubmittedThreeDaysAgoColumnIndex = 46;
        private const int NoOfApprenticeshipApplicationsSubmittedTwoDaysAgoColumnIndex = 47;
        private const int NoOfApprenticeshipApplicationsSubmittedOneDayAgoColumnIndex = 48;

        private readonly ILogger<AnalyticsEventStore> _logger;
        private readonly string _vacancyAnalyticEventsDbConnString;
        private RetryPolicy RetryPolicy { get; }

        public AnalyticsEventStore(ILogger<AnalyticsEventStore> logger, string vacancyAnalyticEventsDbConnString)
        {
            _logger = logger;
            _vacancyAnalyticEventsDbConnString = vacancyAnalyticEventsDbConnString;
            RetryPolicy = GetRetryPolicy();
        }

        public async Task<VacancyAnalyticsSummary> GetVacancyAnalyticEventSummaryAsync(long vacancyReference)
        {
            try
            {
                using (var conn = new SqlConnection(_vacancyAnalyticEventsDbConnString))
                {
                    using (var command = new SqlCommand(GetVacancyAnalyticEventsSqlSproc, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        var inputParam = command.CreateParameter();
                        inputParam.ParameterName = "@VacancyReference";
                        inputParam.DbType = DbType.Int64;
                        inputParam.Value = vacancyReference;
                        inputParam.Direction = ParameterDirection.Input;

                        command.Parameters.Add(inputParam);

                        using (var reader = await RetryPolicy.Execute(async context =>
                                            {
                                                await conn.OpenAsync();
                                                return await command.ExecuteReaderAsync();
                                            }, new Context(nameof(GetVacancyAnalyticEventSummaryAsync))))
                        {
                            await reader.ReadAsync();

                            var summary = new VacancyAnalyticsSummary
                            {
                                VacancyReference = reader.GetInt64(VacancyReferenceColumnIndex),

                                NoOfApprenticeshipSearches = reader.GetInt32(NoOfApprenticeshipSearchesColumnIndex),
                                NoOfApprenticeshipSearchesSevenDaysAgo = reader.GetInt32(NoOfApprenticeshipSearchesSevenDaysAgoColumnIndex),
                                NoOfApprenticeshipSearchesSixDaysAgo = reader.GetInt32(NoOfApprenticeshipSearchesSixDaysAgoColumnIndex),
                                NoOfApprenticeshipSearchesFiveDaysAgo = reader.GetInt32(NoOfApprenticeshipSearchesFiveDaysAgoColumnIndex),
                                NoOfApprenticeshipSearchesFourDaysAgo = reader.GetInt32(NoOfApprenticeshipSearchesFourDaysAgoColumnIndex),
                                NoOfApprenticeshipSearchesThreeDaysAgo = reader.GetInt32(NoOfApprenticeshipSearchesThreeDaysAgoColumnIndex),
                                NoOfApprenticeshipSearchesTwoDaysAgo = reader.GetInt32(NoOfApprenticeshipSearchesTwoDaysAgoColumnIndex),
                                NoOfApprenticeshipSearchesOneDayAgo = reader.GetInt32(NoOfApprenticeshipSearchesOneDayAgoColumnIndex),

                                NoOfApprenticeshipSavedSearchAlerts = reader.GetInt32(NoOfApprenticeshipSavedSearchAlertsColumnIndex),
                                NoOfApprenticeshipSavedSearchAlertsSevenDaysAgo = reader.GetInt32(NoOfApprenticeshipSavedSearchAlertsSevenDaysAgoColumnIndex),
                                NoOfApprenticeshipSavedSearchAlertsSixDaysAgo = reader.GetInt32(NoOfApprenticeshipSavedSearchAlertsSixDaysAgoColumnIndex),
                                NoOfApprenticeshipSavedSearchAlertsFiveDaysAgo = reader.GetInt32(NoOfApprenticeshipSavedSearchAlertsFiveDaysAgoColumnIndex),
                                NoOfApprenticeshipSavedSearchAlertsFourDaysAgo = reader.GetInt32(NoOfApprenticeshipSavedSearchAlertsFourDaysAgoColumnIndex),
                                NoOfApprenticeshipSavedSearchAlertsThreeDaysAgo = reader.GetInt32(NoOfApprenticeshipSavedSearchAlertsThreeDaysAgoColumnIndex),
                                NoOfApprenticeshipSavedSearchAlertsTwoDaysAgo = reader.GetInt32(NoOfApprenticeshipSavedSearchAlertsTwoDaysAgoColumnIndex),
                                NoOfApprenticeshipSavedSearchAlertsOneDayAgo = reader.GetInt32(NoOfApprenticeshipSavedSearchAlertsOneDayAgoColumnIndex),

                                NoOfApprenticeshipSaved = reader.GetInt32(NoOfApprenticeshipSavedColumnIndex),
                                NoOfApprenticeshipSavedSevenDaysAgo = reader.GetInt32(NoOfApprenticeshipSavedSevenDaysAgoColumnIndex),
                                NoOfApprenticeshipSavedSixDaysAgo = reader.GetInt32(NoOfApprenticeshipSavedSixDaysAgoColumnIndex),
                                NoOfApprenticeshipSavedFiveDaysAgo = reader.GetInt32(NoOfApprenticeshipSavedFiveDaysAgoColumnIndex),
                                NoOfApprenticeshipSavedFourDaysAgo = reader.GetInt32(NoOfApprenticeshipSavedFourDaysAgoColumnIndex),
                                NoOfApprenticeshipSavedThreeDaysAgo = reader.GetInt32(NoOfApprenticeshipSavedThreeDaysAgoColumnIndex),
                                NoOfApprenticeshipSavedTwoDaysAgo = reader.GetInt32(NoOfApprenticeshipSavedTwoDaysAgoColumnIndex),
                                NoOfApprenticeshipSavedOneDayAgo = reader.GetInt32(NoOfApprenticeshipSavedOneDayAgoColumnIndex),

                                NoOfApprenticeshipDetailsViews = reader.GetInt32(NoOfApprenticeshipDetailsViewsColumnIndex),
                                NoOfApprenticeshipDetailsViewsSevenDaysAgo = reader.GetInt32(NoOfApprenticeshipDetailsViewsSevenDaysAgoColumnIndex),
                                NoOfApprenticeshipDetailsViewsSixDaysAgo = reader.GetInt32(NoOfApprenticeshipDetailsViewsSixDaysAgoColumnIndex),
                                NoOfApprenticeshipDetailsViewsFiveDaysAgo = reader.GetInt32(NoOfApprenticeshipDetailsViewsFiveDaysAgoColumnIndex),
                                NoOfApprenticeshipDetailsViewsFourDaysAgo = reader.GetInt32(NoOfApprenticeshipDetailsViewsFourDaysAgoColumnIndex),
                                NoOfApprenticeshipDetailsViewsThreeDaysAgo = reader.GetInt32(NoOfApprenticeshipDetailsViewsThreeDaysAgoColumnIndex),
                                NoOfApprenticeshipDetailsViewsTwoDaysAgo = reader.GetInt32(NoOfApprenticeshipDetailsViewsTwoDaysAgoColumnIndex),
                                NoOfApprenticeshipDetailsViewsOneDayAgo = reader.GetInt32(NoOfApprenticeshipDetailsViewsOneDayAgoColumnIndex),

                                NoOfApprenticeshipApplicationsCreated = reader.GetInt32(NoOfApprenticeshipApplicationsCreatedColumnIndex),
                                NoOfApprenticeshipApplicationsCreatedSevenDaysAgo = reader.GetInt32(NoOfApprenticeshipApplicationsCreatedSevenDaysAgoColumnIndex),
                                NoOfApprenticeshipApplicationsCreatedSixDaysAgo = reader.GetInt32(NoOfApprenticeshipApplicationsCreatedSixDaysAgoColumnIndex),
                                NoOfApprenticeshipApplicationsCreatedFiveDaysAgo = reader.GetInt32(NoOfApprenticeshipApplicationsCreatedFiveDaysAgoColumnIndex),
                                NoOfApprenticeshipApplicationsCreatedFourDaysAgo = reader.GetInt32(NoOfApprenticeshipApplicationsCreatedFourDaysAgoColumnIndex),
                                NoOfApprenticeshipApplicationsCreatedThreeDaysAgo = reader.GetInt32(NoOfApprenticeshipApplicationsCreatedThreeDaysAgoColumnIndex),
                                NoOfApprenticeshipApplicationsCreatedTwoDaysAgo = reader.GetInt32(NoOfApprenticeshipApplicationsCreatedTwoDaysAgoColumnIndex),
                                NoOfApprenticeshipApplicationsCreatedOneDayAgo = reader.GetInt32(NoOfApprenticeshipApplicationsCreatedOneDayAgoColumnIndex),

                                NoOfApprenticeshipApplicationsSubmitted = reader.GetInt32(NoOfApprenticeshipApplicationsSubmittedColumnIndex),
                                NoOfApprenticeshipApplicationsSubmittedSevenDaysAgo = reader.GetInt32(NoOfApprenticeshipApplicationsSubmittedSevenDaysAgoColumnIndex),
                                NoOfApprenticeshipApplicationsSubmittedSixDaysAgo = reader.GetInt32(NoOfApprenticeshipApplicationsSubmittedSixDaysAgoColumnIndex),
                                NoOfApprenticeshipApplicationsSubmittedFiveDaysAgo = reader.GetInt32(NoOfApprenticeshipApplicationsSubmittedFiveDaysAgoColumnIndex),
                                NoOfApprenticeshipApplicationsSubmittedFourDaysAgo = reader.GetInt32(NoOfApprenticeshipApplicationsSubmittedFourDaysAgoColumnIndex),
                                NoOfApprenticeshipApplicationsSubmittedThreeDaysAgo = reader.GetInt32(NoOfApprenticeshipApplicationsSubmittedThreeDaysAgoColumnIndex),
                                NoOfApprenticeshipApplicationsSubmittedTwoDaysAgo = reader.GetInt32(NoOfApprenticeshipApplicationsSubmittedTwoDaysAgoColumnIndex),
                                NoOfApprenticeshipApplicationsSubmittedOneDayAgo = reader.GetInt32(NoOfApprenticeshipApplicationsSubmittedOneDayAgoColumnIndex),
                            };

                            return summary;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vacancy events from the Vacancy Analytic Events DB.");
                throw;
            }
        }

        private Polly.Retry.RetryPolicy GetRetryPolicy()
        {
            return Policy
                    .Handle<SqlException>()
                    .Or<DbException>()
                    .WaitAndRetry(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(4)
                    }, (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Error executing SQL Command for method {context.OperationKey} Reason: {exception.Message}. Retrying in {timeSpan.Seconds} secs...attempt: {retryCount}");
                    });
        }
    }
}