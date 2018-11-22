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
        private const string GetVacancyAnalyticEventsSqlSproc = "[VACANCY].[Event_GET_EventsForConsumer]";
        private const string UpdateLastProcessedVacancyEventIdsqlSproc = "[EVENT_SYSTEM].[EventConsumer_UPDATE_LastProcessedVacancyEventId]";
        private const string ConsumerId = "DAS-RECRUIT";

        private readonly ILogger<AnalyticsEventStore> _logger;
        private readonly string _vacancyAnalyticEventsDbConnString;
        private RetryPolicy RetryPolicy { get; }

        public AnalyticsEventStore(ILogger<AnalyticsEventStore> logger, string vacancyAnalyticEventsDbConnString)
        {
            _logger = logger;
            _vacancyAnalyticEventsDbConnString = vacancyAnalyticEventsDbConnString;
            RetryPolicy = GetRetryPolicy();
        }

        public async Task<(List<VacancyAnalyticsSummary>, Guid)> GetVacancyAnalyticEventSummariesAsync()
        {
            Guid lastRetrievedEventId;
            var summaries = new List<VacancyAnalyticsSummary>();

            try
            {
                using (var conn = new SqlConnection(_vacancyAnalyticEventsDbConnString))
                {
                    var command = new SqlCommand(GetVacancyAnalyticEventsSqlSproc, conn);
                    command.CommandType = CommandType.StoredProcedure;

                    var outputParam = command.CreateParameter();
                    outputParam.ParameterName = "@LastVacancyEventToProcessId";
                    outputParam.DbType = DbType.Guid;
                    outputParam.Direction = ParameterDirection.Output;

                    var inputParam = command.CreateParameter();
                    inputParam.ParameterName = "@ConsumerId";
                    inputParam.DbType = DbType.String;
                    inputParam.Value = ConsumerId;
                    inputParam.Direction = ParameterDirection.Input;

                    command.Parameters.Add(inputParam);
                    command.Parameters.Add(outputParam);

                    var reader = await RetryPolicy.ExecuteAsync(async context => 
                    {
                        await conn.OpenAsync();
                        return await command.ExecuteReaderAsync();
                    }, new Context(nameof(GetVacancyAnalyticEventSummariesAsync)));

                    while (await reader.ReadAsync())
                    {
                        var summary = new VacancyAnalyticsSummary
                        {
                            VacancyReference = reader.GetInt64(0),
                            NoOfApprenticeshipSearches = reader.GetInt32(1),
                            NoOfApprenticeshipSavedSearchAlerts = reader.GetInt32(2),
                            NoOfApprenticeshipSaved = reader.GetInt32(3),
                            NoOfApprenticeshipDetailsViews = reader.GetInt32(4),
                            NoOfApprenticeshipApplicationsCreated = reader.GetInt32(5),
                            NoOfApprenticeshipApplicationsSubmitted = reader.GetInt32(6)
                        };
                        
                        summaries.Add(summary);
                    }

                    reader.Close();
                    lastRetrievedEventId = (Guid)command.Parameters["@LastVacancyEventToProcessId"].Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vacancy events from the Vacancy Analytic Events DB.");
                throw;
            }

            return (summaries, lastRetrievedEventId);
        }

        public async Task UpdateLastProcessedVacancyEventIdAsync(Guid id)
        {
            try
            {
                using (var conn = new SqlConnection(_vacancyAnalyticEventsDbConnString))
                {
                    var command = new SqlCommand(UpdateLastProcessedVacancyEventIdsqlSproc, conn);
                    command.CommandType = CommandType.StoredProcedure;

                    var inputParam = command.CreateParameter();
                    inputParam.ParameterName = "@ConsumerId";
                    inputParam.DbType = DbType.String;
                    inputParam.Value = ConsumerId;
                    inputParam.Direction = ParameterDirection.Input;

                    command.Parameters.AddWithValue("@ConsumerId", ConsumerId);
                    command.Parameters.AddWithValue("@LastProcessedVacancyEventId", id);

                    await RetryPolicy.ExecuteAsync(async context => 
                    {
                        await conn.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }, new Context(nameof(UpdateLastProcessedVacancyEventIdAsync)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating last processed vacancy event id {id.ToString()} for consumer {ConsumerId}.");
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
                    }, (exception, timeSpan, retryCount, context) => {
                        _logger.LogWarning($"Error executing SQL Command for method {context.OperationKey} Reason: {exception.Message}. Retrying in {timeSpan.Seconds} secs...attempt: {retryCount}");    
                    });
        }
    }
}