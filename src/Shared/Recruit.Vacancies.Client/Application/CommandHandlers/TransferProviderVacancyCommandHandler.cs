using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class TransferProviderVacancyCommandHandler : IRequestHandler<TransferProviderVacancyCommand, Unit>
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly ILogger<TransferProviderVacancyCommandHandler> _logger;
        private readonly IMessaging _messaging;
        public TransferProviderVacancyCommandHandler(
            IVacancyRepository vacancyRepository,
            IMessaging messaging,
            ILogger<TransferProviderVacancyCommandHandler> logger)
        {
            _logger = logger;
            _messaging = messaging;
            _vacancyRepository = vacancyRepository;
        }
        
        public async Task<Unit> Handle(TransferProviderVacancyCommand message, CancellationToken cancellationToken)
        {
            var vacancy = await _vacancyRepository.GetVacancyAsync(message.VacancyId);
            if (vacancy.OwnerType == OwnerType.Employer)
            {
                _logger.LogInformation($"Cannot transfer vacancy {vacancy.VacancyReference} as it is owned by {vacancy.OwnerType}.");
                return Unit.Value;
            }

            _logger.LogInformation($"Transferring the vacancy {vacancy.VacancyReference} to the employer as the provider {vacancy.TrainingProvider.Ukprn} is blocked");

            vacancy.TransferInfo = new TransferInfo()
            {
                Ukprn = vacancy.TrainingProvider.Ukprn.GetValueOrDefault(),
                ProviderName = vacancy.TrainingProvider.Name,
                LegalEntityName = vacancy.LegalEntityName,
                TransferredByUser = message.TransferredByUser,
                TransferredDate = message.TransferredDate,
                Reason = message.Reason
            };
            vacancy.OwnerType = OwnerType.Employer;

            await _vacancyRepository.UpdateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyTransferredEvent
            {
                VacancyId = vacancy.Id,
                VacancyReference = vacancy.VacancyReference.GetValueOrDefault()
            });
            return Unit.Value;
        }
    }
}