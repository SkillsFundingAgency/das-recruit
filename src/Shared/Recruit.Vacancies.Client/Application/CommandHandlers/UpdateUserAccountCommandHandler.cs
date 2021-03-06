using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateUserAccountCommandHandler : IRequestHandler<UpdateUserAccountCommand>
    {
        private readonly IRecruitVacancyClient _client;
        private readonly IUserRepository _userRepository;
        public UpdateUserAccountCommandHandler(IRecruitVacancyClient client, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _client = client;
        }

        public async Task Handle(UpdateUserAccountCommand message, CancellationToken cancellationToken)
        {
            var accounts = await _client.GetEmployerIdentifiersAsync(message.IdamsUserId);

            var user = await _client.GetUsersDetailsAsync(message.IdamsUserId);

            user.EmployerAccountIds = accounts.ToList();

            await _userRepository.UpsertUserAsync(user);
        }
    }
}