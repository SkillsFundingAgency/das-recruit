using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.VacancyReview
{
    public class VacancyReviewCreatedHandler : DomainEventHandler, IDomainEventHandler<VacancyReviewCreatedEvent>
    {
        private readonly ILogger<VacancyReviewCreatedHandler> _logger;
        private readonly IJobsVacancyClient _client;

        public VacancyReviewCreatedHandler(ILogger<VacancyReviewCreatedHandler> logger, IJobsVacancyClient client) 
            : base(logger)
        {
            _logger = logger;
            _client = client;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<VacancyReviewCreatedEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Begin processing rules for vacancy {@event.VacancyReference} on review {@event.ReviewId}");
                await _client.PerformRulesCheckAsync(@event.ReviewId);
                _logger.LogInformation($"Finished processing rules for vacancy {@event.VacancyReference} on review {@event.ReviewId}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred trying to execute rules on vacancy {@event.VacancyReference} on review {@event.ReviewId}");
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
