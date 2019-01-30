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
        private const int NoOfApprenticeshipSavedSearchAlertsColumnIndex = 2;
        private const int NoOfApprenticeshipSavedColumnIndex = 3;
        private const int NoOfApprenticeshipDetailsViewsColumnIndex = 4;
        private const int NoOfApprenticeshipApplicationsCreatedColumnIndex = 5;
        private const int NoOfApprenticeshipApplicationsSubmittedColumnIndex = 6;

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

                        using (var reader = await RetryPolicy.ExecuteAsync(async context =>
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
                                NoOfApprenticeshipSavedSearchAlerts = reader.GetInt32(NoOfApprenticeshipSavedSearchAlertsColumnIndex),
                                NoOfApprenticeshipSaved = reader.GetInt32(NoOfApprenticeshipSavedColumnIndex),
                                NoOfApprenticeshipDetailsViews = reader.GetInt32(NoOfApprenticeshipDetailsViewsColumnIndex),
                                NoOfApprenticeshipApplicationsCreated = reader.GetInt32(NoOfApprenticeshipApplicationsCreatedColumnIndex),
                                NoOfApprenticeshipApplicationsSubmitted = reader.GetInt32(NoOfApprenticeshipApplicationsSubmittedColumnIndex)
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
                    .WaitAndRetryAsync(new[]
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