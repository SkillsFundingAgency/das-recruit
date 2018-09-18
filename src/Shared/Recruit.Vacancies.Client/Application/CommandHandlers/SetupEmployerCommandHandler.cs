using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;
using System.Threading; 
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;  

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
                EmployerAccountId = message.EmployerAccountId,
            });
        }
    }
}