using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class SetupEmployerHandler : IRequestHandler<SetupEmployerCommand>
    {
        private readonly IMessaging _messaging;

        public SetupEmployerHandler(IMessaging messaging)
        {
            _messaging = messaging;
        }

        public async Task Handle(SetupEmployerCommand message, CancellationToken cancellationToken)
        {
            await _messaging.PublishEvent(new SetupEmployerEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                EmployerAccountId = message.EmployerAccountId,
            });
        }
    }
}