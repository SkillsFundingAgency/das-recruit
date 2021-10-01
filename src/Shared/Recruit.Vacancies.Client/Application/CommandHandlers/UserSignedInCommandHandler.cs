using System;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UserSignedInCommandHandler : IRequestHandler<UserSignedInCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IRecruitQueueService _queueService;

        public UserSignedInCommandHandler(
            IUserRepository userRepository, ITimeProvider timeProvider, IRecruitQueueService queueService)
        {
            _userRepository = userRepository;
            _timeProvider = timeProvider;
            _queueService = queueService;
        }

        public async Task<Unit> Handle(UserSignedInCommand message, CancellationToken cancellationToken)
        {
            await UpsertUserAsync(message.User, message.UserType);
            return Unit.Value;
        }

        private async Task UpsertUserAsync(VacancyUser user, UserType userType)
        {
            var now = _timeProvider.Now;

            var userEntity = await _userRepository.GetAsync(user.UserId) ?? new User
            {
                Id = Guid.NewGuid(),
                IdamsUserId = user.UserId,
                UserType = userType,
                CreatedDate = now
            };

            userEntity.Name = user.Name;
            userEntity.LastSignedInDate = now;
            userEntity.Email = user.Email;

            if (userType == UserType.Provider)
                userEntity.Ukprn = user.Ukprn;

            await _userRepository.UpsertUserAsync(userEntity);

            if (userType == UserType.Employer)
                await _queueService.AddMessageAsync(new UpdateEmployerUserAccountQueueMessage { IdamsUserId = user.UserId });
        }
    }
}