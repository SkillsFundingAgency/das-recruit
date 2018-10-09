using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Application
{
    public class ApplicationWithdrawnHandler : DomainEventHandler, IDomainEventHandler<ApplicationWithdrawnEvent>
    {
        private readonly ILogger<ApplicationWithdrawnEvent> _logger;
        private readonly IJobsVacancyClient _client;

        public ApplicationWithdrawnHandler(ILogger<ApplicationWithdrawnEvent> logger, IJobsVacancyClient client) : base(logger)
        {
            _logger = logger;
            _client = client;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<ApplicationWithdrawnEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(ApplicationWithdrawnEvent)} for vacancy: {{VacancyReference}} and candidate: {{CandidateId}}", @event.VacancyReference, @event.CandidateId);

                await _client.WithdrawApplicationAsync(@event.VacancyReference, @event.CandidateId);

                _logger.LogInformation($"Finished Processing {nameof(ApplicationWithdrawnEvent)} for vacancy: {{VacancyReference}} and candidate: {{CandidateId}}", @event.VacancyReference, @event.CandidateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }
    }
}
