using System;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UserSignedInCommandHandler : IRequestHandler<UserSignedInCommand>
    {
        private readonly IMessaging _messaging;
        private readonly IUserRepository _userRepository;
        private readonly ITimeProvider _timeProvider;

        public UserSignedInCommandHandler(IMessaging messaging, IUserRepository userRepository, ITimeProvider timeProvider)
        {
            _messaging = messaging;
            _userRepository = userRepository;
            _timeProvider = timeProvider;
        }

        public async Task Handle(UserSignedInCommand message, CancellationToken cancellationToken)
        {
            await UpsertUserAsync(message.User);

            await _messaging.PublishEvent(new UserSignedInEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                EmployerAccountId = message.EmployerAccountId,
                User = message.User
            });
        }

        public async Task UpsertUserAsync(VacancyUser user)
        {
            var now = _timeProvider.Now;

            var userEntity = await _userRepository.GetAsync(user.UserId) ?? new User
            {
                Id = Guid.NewGuid(),
                IdamsUserId = user.UserId,
                UserType = UserType.Employer,
                CreatedDate = now
            };

            userEntity.Name = user.Name;
            userEntity.Email = user.Email;
            userEntity.LastSignedInDate = now;

            await _userRepository.UpsertUserAsync(userEntity);
        }
    }
}