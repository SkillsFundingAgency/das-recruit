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
    public class ApproveVacancyEventHandler : INotificationHandler<VacancyReviewApprovedEvent>
    {
        private readonly IMessaging _messaging;

        public ApproveVacancyEventHandler(IMessaging messaging)
        {
            _messaging = messaging;
        }

        public async Task Handle(VacancyReviewApprovedEvent notification, CancellationToken cancellationToken)
        {
           await _messaging.SendCommandAsync(new ApproveVacancyCommand
           {
               VacancyReference = notification.VacancyReference
           });
        }
    }
}
