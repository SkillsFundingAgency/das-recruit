using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateUserAccountCommandHandler : IRequestHandler<UpdateUserAccountCommand, Unit>
    {
        private readonly IRecruitVacancyClient _client;
        private readonly IUserRepository _userRepository;
        public UpdateUserAccountCommandHandler(IRecruitVacancyClient client, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _client = client;
        }

        public async Task<Unit> Handle(UpdateUserAccountCommand message, CancellationToken cancellationToken)
        {
            var accounts = await _client.GetEmployerIdentifiersAsync(message.IdamsUserId, string.Empty);

            var user = await _client.GetUsersDetailsAsync(message.IdamsUserId);

            user.EmployerAccountIds = accounts.UserAccounts.Select(c=>c.AccountId).ToList();

            await _userRepository.UpsertUserAsync(user);
            
            return Unit.Value;
        }
    }
}