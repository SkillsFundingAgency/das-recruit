using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.VacancyReview
{
    public class VacancyReviewApprovedHandler : DomainEventHandler, IDomainEventHandler<VacancyReviewApprovedEvent>
    {
        private readonly ILogger<VacancyReviewApprovedHandler> _logger;
        private readonly IMessaging _messaging;

        public VacancyReviewApprovedHandler(ILogger<VacancyReviewApprovedHandler> logger, IMessaging messaging) 
            : base(logger)
        {
            _logger = logger;
            _messaging = messaging;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var @event = DeserializeEvent<VacancyReviewApprovedEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(VacancyReviewApprovedEvent)} for review: {{ReviewId}} vacancy: {{VacancyReference}}", @event.ReviewId, @event.VacancyReference);
                
                await _messaging.SendCommandAsync(new ApproveVacancyCommand
                {
                    VacancyReference = @event.VacancyReference
                });

                _logger.LogInformation($"Finished Processing {nameof(VacancyReviewApprovedEvent)} for review: {{ReviewId}} vacancy: {{VacancyReference}}", @event.ReviewId, @event.VacancyReference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", @event);
                throw;
            }
        }
    }
}

