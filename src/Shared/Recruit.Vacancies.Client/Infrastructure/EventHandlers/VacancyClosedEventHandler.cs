using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class VacancyClosedEventHandler : INotificationHandler<VacancyClosedEvent>
    {
        private readonly ILogger<VacancyClosedEventHandler> _logger;
        private readonly IQueryStoreWriter _queryStore;

        public VacancyClosedEventHandler(ILogger<VacancyClosedEventHandler> logger, IQueryStoreWriter queryStore)
        {
            _logger = logger;
            _queryStore = queryStore;
        }

        public async Task Handle(VacancyClosedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting LiveVacancy {vacancyReference} from query store.", notification.VacancyReference);
            await _queryStore.DeleteLiveVacancyAsync(notification.VacancyReference);
        }
    }
}
