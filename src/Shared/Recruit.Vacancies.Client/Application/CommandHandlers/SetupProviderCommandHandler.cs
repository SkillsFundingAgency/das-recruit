using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;
using System.Threading; 
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;  

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class SetupProviderCommandHandler : IRequestHandler<SetupProviderCommand, Unit>
    {
        private readonly IMessaging _messaging;

        public SetupProviderCommandHandler(IMessaging messaging)
        {
            _messaging = messaging;
        }

        public async Task<Unit> Handle(SetupProviderCommand message, CancellationToken cancellationToken)
        {
            await _messaging.PublishEvent(new SetupProviderEvent(message.Ukprn));
            return Unit.Value;
        }
    }
}