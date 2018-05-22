using System;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
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
        private readonly IUserRepository _userRepository;
        private readonly ITimeProvider _timeProvider;

        public UserSignedInCommandHandler(IUserRepository userRepository, ITimeProvider timeProvider)
        {
            _userRepository = userRepository;
            _timeProvider = timeProvider;
        }

        public async Task Handle(UserSignedInCommand message, CancellationToken cancellationToken)
        {
            await UpsertUserAsync(message.User);
        }

        private async Task UpsertUserAsync(VacancyUser user)
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