using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class TransferProviderVacancyToLegalEntityCommandHandler : IRequestHandler<TransferVacancyToLegalEntityCommand, Unit>
    {
        private readonly ILogger<TransferProviderVacancyToLegalEntityCommandHandler> _logger;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IVacancyTransferService _vacancyTransferService;
        private readonly IVacancyReviewTransferService _vacancyReviewTransferService;
        private readonly ITimeProvider _timeProvider;
        private readonly IMessaging _messaging;

        public TransferProviderVacancyToLegalEntityCommandHandler(ILogger<TransferProviderVacancyToLegalEntityCommandHandler> logger,
                                                            IVacancyRepository vacancyRepository,
                                                            IVacancyTransferService vacancyTransferService,
                                                            IVacancyReviewTransferService vacancyReviewTransferService,
                                                            ITimeProvider timeProvider, IMessaging messaging)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _vacancyTransferService = vacancyTransferService;
            _vacancyReviewTransferService = vacancyReviewTransferService;
            _timeProvider = timeProvider;
            _messaging = messaging;
        }

        public async Task<Unit> Handle(TransferVacancyToLegalEntityCommand message, CancellationToken cancellationToken)
        {
            var vacancy = await _vacancyRepository.GetVacancyAsync(message.VacancyReference);

            if (vacancy.OwnerType == OwnerType.Provider)
            {
                var vacancyUser = new VacancyUser
                {
                    UserId = message.UserRef.ToString(),
                    Email = message.UserEmailAddress,
                    Name = message.UserName
                };

                await ProcessTransferringVacancy(vacancy, vacancyUser, message.TransferReason);
            }
            return Unit.Value;
        }

        private async Task ProcessTransferringVacancy(Vacancy vacancy, VacancyUser user, TransferReason transferReason)
        {
            _logger.LogInformation($"Starting transfer of vacancy {vacancy.VacancyReference.Value} to Legal Entity. Transfer reason: {transferReason.ToString()}");
            var originalStatus = vacancy.Status;

            await _vacancyTransferService.TransferVacancyToLegalEntityAsync(vacancy, user, transferReason);

            switch (originalStatus)
            {
                case VacancyStatus.Submitted:
                    await _vacancyReviewTransferService.CloseVacancyReview(vacancy.VacancyReference.GetValueOrDefault(), transferReason);
                    break;
                case VacancyStatus.Approved:
                case VacancyStatus.Live:
                    await _messaging.PublishEvent(new VacancyClosedEvent
                    {
                        VacancyReference = vacancy.VacancyReference.Value,
                        VacancyId = vacancy.Id
                    });
                    break;
                default:
                    break;
            }

            await _messaging.PublishEvent(new VacancyTransferredEvent
            {
                VacancyId = vacancy.Id,
                VacancyReference = vacancy.VacancyReference.GetValueOrDefault()
            });

            _logger.LogInformation($"Finished transfer of vacancy {vacancy.VacancyReference.Value} to Legal Entity. Transfer reason: {transferReason.ToString()}");
        }
    }
}