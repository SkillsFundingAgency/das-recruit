using System;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UserSignedInCommandHandler(
        ILogger<UserSignedInCommandHandler> logger,
        IUserWriteRepository userWriteRepository,
        IUserRepository userRepository,
        IUserNotificationPreferencesRepository userNotificationPreferencesRepository,
        ITimeProvider timeProvider,
        IRecruitQueueService queueService)
        : IRequestHandler<UserSignedInCommand, Unit>
    {
        public async Task<Unit> Handle(UserSignedInCommand message, CancellationToken cancellationToken)
        {
            await UpsertUserAsync(message.User, message.UserType);
            return Unit.Value;
        }

        private async Task UpsertUserAsync(VacancyUser user, UserType userType)
        {
            logger.LogInformation("Upserting user {name} of type {userType}.", user.Name, userType.ToString());

            if (userType == UserType.Provider)
            {
                logger.LogInformation("Provider {dfEUserId}", user.DfEUserId);
            }
            
            var now = timeProvider.Now;

            var existingUser = userType == UserType.Provider
                ? await userRepository.GetByDfEUserId(user.DfEUserId) ?? await userRepository.GetUserByEmail(user.Email, UserType.Provider)
                : await userRepository.GetAsync(user.UserId);
            
            var userEntity = existingUser ?? new User
            {
                Id = Guid.NewGuid(),
                IdamsUserId = user.UserId,
                UserType = userType,
                CreatedDate = now,
                DfEUserId = user.DfEUserId
            };

            var userNotificationPreferences = userType == UserType.Provider 
                ? await userNotificationPreferencesRepository.GetByDfeUserId(userEntity.DfEUserId) ?? await userNotificationPreferencesRepository.GetAsync(userEntity.IdamsUserId)
                : await userNotificationPreferencesRepository.GetAsync(userEntity.IdamsUserId);

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

            await userWriteRepository.UpsertUserAsync(userEntity);
            await userNotificationPreferencesRepository.UpsertAsync(userNotificationPreferences);

            if (userType == UserType.Employer)
                await queueService.AddMessageAsync(new UpdateEmployerUserAccountQueueMessage { IdamsUserId = user.UserId });

            logger.LogInformation("Finished upserting user {name}.", user.Name);
        }
    }
}