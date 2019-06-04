using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateFaaOnLiveVacancyClosingDateChanged : INotificationHandler<LiveVacancyClosingDateChangedEvent>
    {
        private readonly IFaaService _faaService;
        private readonly ILogger<UpdateFaaOnLiveVacancyClosingDateChanged> _logger;

        public UpdateFaaOnLiveVacancyClosingDateChanged(
            IFaaService faaService,
            ILogger<UpdateFaaOnLiveVacancyClosingDateChanged> logger)
        {
            _faaService = faaService;
            _logger = logger;
        }

        public Task Handle(LiveVacancyClosingDateChangedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling {notificationType} for vacancy id {vacancyId}",
                nameof(LiveVacancyClosingDateChangedEvent), notification.VacancyId);

            var message = new FaaVacancyStatusSummary(notification.VacancyReference, FaaVacancyStatuses.Live, notification.NewClosingDate);
            return _faaService.PublishVacancyStatusSummaryAsync(message);

        }
    }
}
