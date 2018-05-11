using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class UpdateVacancyEventHandler : INotificationHandler<VacancyReviewReferredEvent>
    {
        private readonly IMessaging _messaging;

        public UpdateVacancyEventHandler(IMessaging messaging)
        {
            _messaging = messaging;
        }

        public async Task Handle(VacancyReviewReferredEvent notification, CancellationToken cancellationToken)
        {
            await _messaging.SendCommandAsync(new ReferVacancyCommand
            {
                VacancyReference = notification.VacancyReference
            });
        }
    }
}
