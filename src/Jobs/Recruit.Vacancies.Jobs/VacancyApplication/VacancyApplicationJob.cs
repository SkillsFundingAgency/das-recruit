using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Events;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Jobs.VacancyApplication
{
    public class VacancyApplicationJob
    {
        private readonly ILogger<VacancyApplicationJob> _logger;
        private readonly VacancyApplicationCommandHandler _handler;
        private string JobName => GetType().Name;

        public VacancyApplicationJob(ILogger<VacancyApplicationJob> logger, VacancyApplicationCommandHandler handler)
        {
            _logger = logger;
            _handler = handler;
        }

        public Task VacancyApplicationSubmitted([QueueTrigger(QueueNames.ApplicationSubmittedQueueName, Connection = "EventQueueConnectionString")] string message, TextWriter log)
        {
            var command = JsonConvert.DeserializeObject<ApplicationSubmitCommand>(message);
            
            return _handler.Handle(command);
        }
    }
}
