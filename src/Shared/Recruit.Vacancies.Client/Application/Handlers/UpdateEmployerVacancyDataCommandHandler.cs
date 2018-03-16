using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Handlers
{
    public class UpdateEmployerVacancyDataCommandHandler : IRequestHandler<UpdateEmployerVacancyDataCommand>
    {
        private readonly IMessaging _messaging;

        public UpdateEmployerVacancyDataCommandHandler(IMessaging messaging)
        {
            _messaging = messaging;
        }

        public async Task Handle(UpdateEmployerVacancyDataCommand message, CancellationToken cancellationToken)
        {
            await _messaging.PublishEvent(new UserSignedInEvent
            {
                SourceCommandId = message.CommandId.ToString(),
                EmployerAccountId = message.EmployerAccountId
            });
        }
    }
}