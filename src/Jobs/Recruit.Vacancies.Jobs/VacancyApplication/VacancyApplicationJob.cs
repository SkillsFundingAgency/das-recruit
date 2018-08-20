using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
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

        public async Task VacancyApplicationSubmitted([QueueTrigger(QueueNames.ApplicationSubmittedQueueName, Connection = "EventQueueConnectionString")] string message, TextWriter log)
        {
            try
            {
                var command = JsonConvert.DeserializeObject<ApplicationSubmittedEvent>(message);

                _logger.LogInformation("Start {JobName} for vacancyId: {vacancyReference} for candidateId: {candidateId} ", JobName, command.Application.VacancyReference, command.Application.CandidateId);

                await _handler.Handle(new CreateApplicationReviewCommand { Application = command.Application });

                _logger.LogInformation("Finished {JobName} for vacancyId: {vacancyReference} for candidateId: {candidateId} ", JobName, command.Application.VacancyReference, command.Application.CandidateId);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Unable to deserialise event: {eventBody}", message);
                throw;
            }
        }
    }
}
