using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class DeleteStaleVacanciesQueueTrigger
    {
        public const int DefaultDraftStaleByDays = 180;
        public const int DefaultReferredStaleByDays = 90;
        private readonly ILogger<DeleteStaleVacanciesQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly ITimeProvider _timeProvider;
        private readonly IVacancyQuery _vacancyQuery;
        private readonly IMessaging _messaging;

        private string JobName => GetType().Name;

        public DeleteStaleVacanciesQueueTrigger(ILogger<DeleteStaleVacanciesQueueTrigger> logger, 
            RecruitWebJobsSystemConfiguration jobsConfig,
            ITimeProvider timeProvider,
            IVacancyQuery vacancyQuery,
            IMessaging messaging)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _timeProvider = timeProvider;
            _vacancyQuery = vacancyQuery;
            _messaging = messaging;
        }

        public async Task DeleteStaleVacanciesAsync([QueueTrigger(QueueNames.DeleteStaleVacanciesQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            try
            {
                if (_jobsConfig.DisabledJobs.Contains(JobName))
                {
                    _logger.LogDebug($"{JobName} is disabled, skipping ...");
                    return;
                }

                var payload = JsonConvert.DeserializeObject<DeleteStaleVacanciesQueueMessage>(message);

                var targetDate = payload?.CreatedByScheduleDate ?? _timeProvider.Today;

                _logger.LogInformation($"Begining to delete vacancies that have not been updated since {targetDate.ToShortDateString()}");

                var draftStaleByDate = targetDate.AddDays((_jobsConfig.DraftVacanciesStaleByDays ?? DefaultDraftStaleByDays) * -1);
                var referredStaleByDate = targetDate.AddDays((_jobsConfig.ReferredVacanciesStaleByDays ?? DefaultReferredStaleByDays) * -1);

                var staleDraftVacanciesTask = _vacancyQuery.GetDraftVacanciesCreatedBeforeAsync<VacancyIdentifier>(draftStaleByDate);
                var staleReferredVacanciesTask = _vacancyQuery.GetReferredVacanciesSubmittedBeforeAsync<VacancyIdentifier>(referredStaleByDate);

                await Task.WhenAll(staleDraftVacanciesTask, staleReferredVacanciesTask);

                var staleVacancies = staleDraftVacanciesTask.Result.Concat(staleReferredVacanciesTask.Result);

                _logger.LogInformation($"Found {staleDraftVacanciesTask.Result.Count()} {VacancyStatus.Draft} vacancies that have been created on or before {draftStaleByDate}");
                _logger.LogInformation($"Found {staleReferredVacanciesTask.Result.Count()} {VacancyStatus.Referred} vacancies that have been submitted on or before {referredStaleByDate}");

                await Task.WhenAll(GetDeleteVacancyTasks(staleVacancies));

                _logger.LogInformation($"Finished deleting vacancies that have not been updated since {targetDate.ToShortDateString()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to run {JobName}");
                throw;
            }
        }

        private List<Task> GetDeleteVacancyTasks(IEnumerable<VacancyIdentifier> vacancies)
        {
            var tasks = new List<Task>();

            foreach(var v in vacancies) 
            {
                var cmd = new DeleteVacancyCommand() { VacancyId = v.Id };
                tasks.Add(_messaging.SendCommandAsync(cmd));
            }

            return tasks;
        }
    }
}