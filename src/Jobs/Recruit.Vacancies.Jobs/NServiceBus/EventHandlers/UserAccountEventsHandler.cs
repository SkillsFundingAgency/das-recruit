using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using NServiceBus;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace Esfa.Recruit.Vacancies.Jobs.NServiceBus.EventHandlers
{
    public class UserAccountEventsHandler : IHandleMessages<UserJoinedEvent>, IHandleMessages<AccountUserRemovedEvent>
    {
        private readonly IRecruitQueueService _queueService;
        private readonly IUserRepository _userRepository;
        private readonly IUserNotificationPreferencesRepository _userNotificationPrefereneRepository;

        public UserAccountEventsHandler (IRecruitQueueService queueService, IUserRepository userRepository, IUserNotificationPreferencesRepository userNotificationPrefereneRepository)
        {
            _userRepository = userRepository;
            _userNotificationPrefereneRepository = userNotificationPrefereneRepository;
            _queueService = queueService;
        }

        public Task Handle (UserJoinedEvent message, IMessageHandlerContext context)
        {
            return UpdateUserAsync (message.UserRef);
        }

        public Task Handle (AccountUserRemovedEvent message, IMessageHandlerContext context)
        {
            return UpdateUserAsync (message.UserRef);
        }

        private async Task UpdateUserAsync (Guid userIdamsId)
        {
            var shouldUpdateUser = await DoesUserExistsAsync(userIdamsId);
            if (shouldUpdateUser)
            {
                await _queueService.AddMessageAsync (new UpdateUserAccountQueueMessage { IdamsUserId = userIdamsId.ToString () });
            }
        }

        private async Task<bool> DoesUserExistsAsync(Guid userIdamsId)
        {
            var user = await _userRepository.GetAsync(userIdamsId.ToString());
            return user != null;
        }
    }
}