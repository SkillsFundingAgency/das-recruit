﻿using System;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UserSignedInCommandHandler : IRequestHandler<UserSignedInCommand, Unit>
    {
        private readonly ILogger<UserSignedInCommandHandler> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IUserNotificationPreferencesRepository _userNotificationPreferencesRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IRecruitQueueService _queueService;

        public UserSignedInCommandHandler(
            ILogger<UserSignedInCommandHandler> logger,
            IUserRepository userRepository, 
            IUserNotificationPreferencesRepository userNotificationPreferencesRepository,
            ITimeProvider timeProvider, 
            IRecruitQueueService queueService)
        {
            _userRepository = userRepository;
            _userNotificationPreferencesRepository = userNotificationPreferencesRepository;
            _timeProvider = timeProvider;
            _queueService = queueService;
            _logger = logger;
        }

        public async Task<Unit> Handle(UserSignedInCommand message, CancellationToken cancellationToken)
        {
            await UpsertUserAsync(message.User, message.UserType);
            return Unit.Value;
        }

        private async Task UpsertUserAsync(VacancyUser user, UserType userType)
        {
            _logger.LogInformation("Upserting user {name} of type {userType}.", user.Name, userType.ToString());

            if (userType == UserType.Provider)
            {
                _logger.LogInformation("Provider {dfEUserId}", user.DfEUserId);
            }
            
            var now = _timeProvider.Now;

            var existingUser = userType == UserType.Provider
                ? await _userRepository.GetByDfEUserId(user.DfEUserId) ?? await _userRepository.GetUserByEmail(user.Email, UserType.Provider)
                : await _userRepository.GetAsync(user.UserId);
            
            var userEntity = existingUser ?? new User
            {
                Id = Guid.NewGuid(),
                IdamsUserId = user.UserId,
                UserType = userType,
                CreatedDate = now,
                DfEUserId = user.DfEUserId
            };

            var userNotificationPreferences = userType == UserType.Provider 
                ? await _userNotificationPreferencesRepository.GetByDfeUserId(userEntity.DfEUserId) ?? await _userNotificationPreferencesRepository.GetAsync(userEntity.IdamsUserId)
                : await _userNotificationPreferencesRepository.GetAsync(userEntity.IdamsUserId);

            userNotificationPreferences ??= new UserNotificationPreferences
            {
                Id = userEntity.IdamsUserId,
                NotificationTypes = userEntity.UserType == UserType.Provider
                    ? NotificationTypes.VacancyRejectedByEmployer
                    : NotificationTypes.VacancySentForReview,
                NotificationScope = NotificationScope.OrganisationVacancies,
                DfeUserId = userEntity.DfEUserId
            };                                                        
                                                                      
            userEntity.Name = user.Name;
            userEntity.LastSignedInDate = now;
            userEntity.Email = user.Email;
            
            if (userType == UserType.Provider)
            {
                userEntity.Ukprn = user.Ukprn;
                if (string.IsNullOrEmpty(userEntity.DfEUserId) || userEntity.DfEUserId != user.DfEUserId)
                {
                    userEntity.DfEUserId = user.DfEUserId;    
                }

                if (string.IsNullOrEmpty(userNotificationPreferences.DfeUserId)|| userEntity.DfEUserId != user.DfEUserId)
                {
                    userNotificationPreferences.DfeUserId = user.DfEUserId;
                }
            }

            await _userRepository.UpsertUserAsync(userEntity);
            await _userNotificationPreferencesRepository.UpsertAsync(userNotificationPreferences);

            if (userType == UserType.Employer)
                await _queueService.AddMessageAsync(new UpdateEmployerUserAccountQueueMessage { IdamsUserId = user.UserId });

            _logger.LogInformation("Finished upserting user {name}.", user.Name);
        }
    }
}