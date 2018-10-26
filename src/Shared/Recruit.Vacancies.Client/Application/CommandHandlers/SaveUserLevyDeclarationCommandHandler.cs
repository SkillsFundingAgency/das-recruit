using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class SaveUserLevyDeclarationCommandHandler : IRequestHandler<SaveUserLevyDeclarationCommand>
    {
        private readonly ILogger<SaveUserLevyDeclarationCommandHandler> _logger;
        private readonly IUserRepository _repository;
        public SaveUserLevyDeclarationCommandHandler(
            ILogger<SaveUserLevyDeclarationCommandHandler> logger,
            IUserRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task Handle(SaveUserLevyDeclarationCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating user with id: {userId} with Legacy declaration for account {employerAccountId}", message.UserId, message.EmployerAccountId);
            
            var user = await _repository.GetAsync(message.UserId);

            if (user.AccountsDeclaredAsLevyPayers.Contains(message.EmployerAccountId))
            {
                _logger.LogWarning($"The account {message.EmployerAccountId} was already in the list of declared levy payers for user: {message.UserId}");
                return;
            }

            user.AccountsDeclaredAsLevyPayers .Add(message.EmployerAccountId);

            await _repository.UpsertUserAsync(user);
        }
    }
}
