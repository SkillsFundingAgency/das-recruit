using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateLiveVacancyOnVacancyChange : INotificationHandler<VacancyApprovedEvent>, INotificationHandler<VacancyLiveEvent>
    {
        private readonly IVacancyRepository _repository;
        private readonly ILogger<UpdateDashboardOnVacancyChange> _logger;
        private readonly IMessaging _messaging;
        private readonly IQueryStoreWriter _queryStoreWriter;

        public UpdateLiveVacancyOnVacancyChange(IQueryStoreWriter queryStoreWriter, ILogger<UpdateDashboardOnVacancyChange> logger, 
            IVacancyRepository repository, IMessaging messaging)
        {
            _logger = logger;
            _messaging = messaging;
            _queryStoreWriter = queryStoreWriter;
            _repository = repository;
        }
        
        public Task Handle(VacancyApprovedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling {notificationType} for vacancyId: {vacancyId}", notification.GetType().Name, notification?.VacancyId);
            
            //For now approved vacancies are immediately made Live
            return _messaging.SendCommandAsync(new LiveVacancyCommand
            {
                VacancyId = notification.VacancyId
            });
        }

        public async Task Handle(VacancyLiveEvent notification, CancellationToken cancellationToken)
        {
            var vacancy = await _repository.GetVacancyAsync(notification.VacancyId);
            await _queryStoreWriter.UpdateLiveVacancyAsync(vacancy.ToLiveVacancyProjection());
        }
    }
}
