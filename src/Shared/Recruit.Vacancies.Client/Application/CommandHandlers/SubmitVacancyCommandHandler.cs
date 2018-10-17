using System;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class SubmitVacancyCommandHandler : IRequestHandler<SubmitVacancyCommand>
    {
        private readonly ILogger<SubmitVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;

        public SubmitVacancyCommandHandler(
            ILogger<SubmitVacancyCommandHandler> logger,
            IVacancyRepository repository, 
            IMessaging messaging, 
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeProvider;
        }

        public async Task Handle(SubmitVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Submitting vacancy {vacancyId}.", message.Vacancy.Id);

            if (message.Vacancy == null || message.Vacancy.CanSubmit == false)
            {
                _logger.LogWarning($"Unable to submit vacancy {{vacancyId}} due to vacancy having a status of {message.Vacancy?.Status}.", message.Vacancy?.Id);
                return;
            }
            
            var now = _timeProvider.Now;

            message.Vacancy.Status = VacancyStatus.Submitted;
            message.Vacancy.SubmittedDate = now;
            message.Vacancy.SubmittedByUser = message.User;
            message.Vacancy.LastUpdatedDate = now;
            message.Vacancy.LastUpdatedByUser = message.User;

            if (message.Vacancy.VacancyReference.HasValue == false)
                throw new Exception("Cannot submit vacancy without a vacancy reference");

            await _repository.UpdateAsync(message.Vacancy);

            await _messaging.PublishEvent(new VacancySubmittedEvent
            {
                EmployerAccountId = message.Vacancy.EmployerAccountId,
                VacancyId = message.Vacancy.Id,
                VacancyReference = message.Vacancy.VacancyReference.Value
            });
        }
    }
}
