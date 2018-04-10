using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
    {
        private readonly IMessaging _messaging;

        public UpdateUserCommandHandler(IMessaging messaging)
        {
            _messaging = messaging;
        }

        public async Task Handle(UpdateUserCommand message, CancellationToken cancellationToken)
        {
            await _messaging.PublishEvent(new UserSignedInEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                EmployerAccountId = message.EmployerAccountId
            });
        }
    }
}