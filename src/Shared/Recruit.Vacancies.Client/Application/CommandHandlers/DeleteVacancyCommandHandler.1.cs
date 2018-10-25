using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;
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
            _logger.LogInformation($"Updating user with id: {{userId}} with Legacy declaration of {message.DeclaringAsLevyEmployer}", message.UserId);
            
            var user = await _repository.GetAsync(message.UserId);

            user.DeclaredAsLevyPayer = message.DeclaringAsLevyEmployer;

            await _repository.UpsertUserAsync(user);
        }
    }
}
