using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class VacancyClosedHandler : INotificationHandler<VacancyClosedEvent>
    {
        private readonly ILogger<VacancyClosedHandler> _logger;
        private readonly IQueryStoreWriter _queryStore;

        public VacancyClosedHandler(ILogger<VacancyClosedHandler> logger, IQueryStoreWriter queryStore)
        {
            _logger = logger;
            _queryStore = queryStore;
        }

        public async Task Handle(VacancyClosedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Deleting LiveVacancy {notification.VacancyReference} from query store.");
            await _queryStore.DeleteLiveVacancyAsync(notification.VacancyReference);
        }
    }
}
