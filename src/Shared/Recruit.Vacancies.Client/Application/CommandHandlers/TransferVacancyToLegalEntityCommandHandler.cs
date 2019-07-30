using System;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Mappers;
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
    public class TransferVacancyToLegalEntityCommandHandler : IRequestHandler<TransferVacancyToLegalEntityCommand>
    {
        private readonly ILogger<TransferVacancyToLegalEntityCommandHandler> _logger;
        private readonly IVacancyRepository _vacancyRepository;

        private readonly IUserRepository _userRepository;
        private readonly IBlockedOrganisationQuery _blockedOrganisationQuery;
        private readonly IVacancyTransferService _vacancyTransferService;
        private readonly IVacancyReviewTransferService _vacancyReviewTransferService;
        private readonly ITimeProvider _timeProvider;
        private readonly IMessaging _messaging;

        public TransferVacancyToLegalEntityCommandHandler(ILogger<TransferVacancyToLegalEntityCommandHandler> logger,
                                                            IVacancyRepository vacancyRepository,
                                                            IUserRepository userRepository,
                                                            IBlockedOrganisationQuery blockedOrganisationQuery,
                                                            IVacancyTransferService vacancyTransferService,
                                                            IVacancyReviewTransferService vacancyReviewTransferService,
                                                            ITimeProvider timeProvider, IMessaging messaging)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _userRepository = userRepository;
            _blockedOrganisationQuery = blockedOrganisationQuery;
            _vacancyTransferService = vacancyTransferService;
            _vacancyReviewTransferService = vacancyReviewTransferService;
            _timeProvider = timeProvider;
            _messaging = messaging;
        }

        public async Task Handle(TransferVacancyToLegalEntityCommand message, CancellationToken cancellationToken)
        {
            var vacancyTask = _vacancyRepository.GetVacancyAsync(message.VacancyReference);
            var userTask = _userRepository.GetAsync(message.UserRef.ToString());

            await Task.WhenAll(vacancyTask, userTask);

            var user = userTask.Result;
            var vacancy = vacancyTask.Result;

            if (vacancy.OwnerType == OwnerType.Provider)
            {
                if (user == null)
                {
                    user = await SaveUnknownUser(message);
                }

                await ProcessTransferringVacancy(vacancy, user);
            }
        }

        private async Task<User> SaveUnknownUser(TransferVacancyToLegalEntityCommand message)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                IdamsUserId = message.UserRef.ToString(),
                UserType = UserType.Employer,
                Email = message.UserEmailAddress,
                Name = message.UserName,
                CreatedDate = _timeProvider.Now
            };

            await _userRepository.UpsertUserAsync(user);

            return user;
        }

        private async Task ProcessTransferringVacancy(Vacancy vacancy, User user)
        {
            var originalStatus = vacancy.Status;
            var vacancyUser = VacancyUserMapper.MapFromUser(user);
            var blockedOrganisationEntry = await _blockedOrganisationQuery.GetByOrganisationIdAsync(vacancy.TrainingProvider.Ukprn.Value.ToString());
            var isProviderBlocked = blockedOrganisationEntry != null && blockedOrganisationEntry.BlockedStatus == BlockedStatus.Blocked;

            await _vacancyTransferService.TransferVacancyToLegalEntityAsync(vacancy, vacancyUser, isProviderBlocked);

            await _vacancyRepository.UpdateAsync(vacancy);

            switch (originalStatus)
            {
                case VacancyStatus.Submitted:
                    await _vacancyReviewTransferService.CloseVacancyReview(vacancy.VacancyReference.GetValueOrDefault(), isProviderBlocked);
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
        }
    }
}