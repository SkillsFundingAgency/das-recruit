﻿using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class DeleteVacancyCommandHandler : IRequestHandler<DeleteVacancyCommand>
    {
        private readonly ILogger<DeleteVacancyCommandHandler> _logger;
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;

        public DeleteVacancyCommandHandler(
            ILogger<DeleteVacancyCommandHandler> logger,
            IVacancyRepository repository, 
            IMessaging messaging, 
            ITimeProvider timeProvider)
        {
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
            _timeProvider = timeProvider;
        }

        public async Task Handle(DeleteVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting vacancy {vacancyId}", message.VacancyId);
            
            var vacancy = await _repository.GetVacancyAsync(message.VacancyId);

            if (vacancy == null || vacancy.CanDelete == false)
            {
                _logger.LogWarning($"Unable to delete vacancy {{vacancyId}} due to vacancy having a status of {vacancy?.Status}.", message.VacancyId);
                return;
            }

            var now = _timeProvider.Now;

            vacancy.IsDeleted = true;
            vacancy.DeletedDate = now;
            vacancy.DeletedByUser = message.User;
            vacancy.LastUpdatedDate = now;
            vacancy.LastUpdatedByUser = message.User;

            await _repository.UpdateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyDeletedEvent
            {
                VacancyId = vacancy.Id
            });
        }
    }
}
