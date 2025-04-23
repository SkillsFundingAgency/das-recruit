using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers {
    public class UpdateUserAccountQueueTrigger {
        private readonly ILogger<UpdateUserAccountQueueTrigger> _logger;
        private readonly IJobsVacancyClient _client;

        private string JobName => GetType().Name;
        public UpdateUserAccountQueueTrigger (
            ILogger<UpdateUserAccountQueueTrigger> logger,
            IJobsVacancyClient client) 
        {
            _client = client;
            _logger = logger;
        }

        public async Task UpdateUserAccountAsync (
            [QueueTrigger (QueueNames.UpdateEmployerUserAccountQueueName, Connection = "QueueStorage")] string payload, TextWriter log) 
        {
            var message = JsonConvert.DeserializeObject<UpdateEmployerUserAccountQueueMessage>(payload);

            _logger.LogInformation("Starting update user account");

            await _client.UpdateUserAccountAsync(message.IdamsUserId);

            _logger.LogInformation("Finished update user account");

        }
    }
}