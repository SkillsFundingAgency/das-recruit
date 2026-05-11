using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateUserAccountCommandHandler(IRecruitVacancyClient client, IUserWriteRepository userRepository)
        : IRequestHandler<UpdateUserAccountCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateUserAccountCommand message, CancellationToken cancellationToken)
        {
            var accounts = await client.GetEmployerIdentifiersAsync(message.IdamsUserId, string.Empty);

            var user = await client.GetUsersDetailsAsync(message.IdamsUserId);

            user.EmployerAccountIds = accounts.UserAccounts.Select(c=>c.AccountId).ToList();

            await userRepository.UpsertUserAsync(user);
            
            return Unit.Value;
        }
    }
}