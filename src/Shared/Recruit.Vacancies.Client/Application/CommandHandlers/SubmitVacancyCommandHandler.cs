using System;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class SubmitVacancyCommandHandler : IRequestHandler<SubmitVacancyCommand>
    {
        private readonly ILogger<SubmitVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;

        public SubmitVacancyCommandHandler(
            ILogger<SubmitVacancyCommandHandler> logger,
            IVacancyRepository vacancyRepository, 
            IMessaging messaging, 
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _messaging = messaging;
            _timeProvider = timeProvider;
        }

        public async Task Handle(SubmitVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Submitting vacancy {vacancyId}.", message.VacancyId);

            var vacancy = await _vacancyRepository.GetVacancyAsync(message.VacancyId);
            
            if (vacancy == null || vacancy.CanSubmit == false)
            {
                _logger.LogWarning($"Unable to submit vacancy {{vacancyId}} due to vacancy having a status of {vacancy?.Status}.", message.VacancyId);
                return;
            }
            
            var now = _timeProvider.Now;

            vacancy.EmployerDescription = message.EmployerDescription;
            vacancy.Status = VacancyStatus.Submitted;
            vacancy.SubmittedDate = now;
            vacancy.SubmittedByUser = message.User;
            vacancy.LastUpdatedDate = now;
            vacancy.LastUpdatedByUser = message.User;

            if (vacancy.VacancyReference.HasValue == false)
                throw new Exception("Cannot submit vacancy without a vacancy reference");

            await _vacancyRepository.UpdateAsync(vacancy);

            await _messaging.PublishEvent(new VacancySubmittedEvent
            {
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyId = vacancy.Id,
                VacancyReference = vacancy.VacancyReference.Value
            });
        }
    }
}
