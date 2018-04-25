using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Dashboard;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateDashboardOnVacancyChange : INotificationHandler<VacancyCreatedEvent>,
                                                    INotificationHandler<VacancyUpdatedEvent>,
                                                    INotificationHandler<VacancySubmittedEvent>,
                                                    INotificationHandler<VacancyDeletedEvent>,
                                                    INotificationHandler<VacancyLiveEvent>
    {
        private readonly IQueryStoreWriter _queryStoreWriter;
        private readonly ILogger<UpdateDashboardOnVacancyChange> _logger;
        private readonly IVacancyRepository _repository;

        public UpdateDashboardOnVacancyChange(IVacancyRepository repository, IQueryStoreWriter queryStoreWriter, ILogger<UpdateDashboardOnVacancyChange> logger)
        {
            _repository = repository;
            _queryStoreWriter = queryStoreWriter;
            _logger = logger;
        }

        public async Task Handle(VacancyCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handling {notification?.GetType().Name} for accountId: {{employerAccountId}}", notification?.EmployerAccountId);
            await ReBuildDashboard(notification.EmployerAccountId);
        }

        public async Task Handle(VacancyUpdatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handling {notification?.GetType().Name} for accountId: {{employerAccountId}}", notification?.EmployerAccountId);
            await ReBuildDashboard(notification.EmployerAccountId);
        }

        public async Task Handle(VacancySubmittedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handling {notification?.GetType().Name} for accountId: {{employerAccountId}}", notification?.EmployerAccountId);
            await ReBuildDashboard(notification.EmployerAccountId);
        }

        public async Task Handle(VacancyDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handling {notification?.GetType().Name} for accountId: {{employerAccountId}}", notification?.EmployerAccountId);
            await ReBuildDashboard(notification.EmployerAccountId);
        }

        public async Task Handle(VacancyLiveEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handling {notification?.GetType().Name} for accountId: {{employerAccountId}}", notification?.EmployerAccountId);
            await ReBuildDashboard(notification.EmployerAccountId);
        }

        private async Task ReBuildDashboard(string employerAccountId)
        {
            var vacancySummaries = await _repository.GetVacanciesByEmployerAccountAsync<VacancySummary>(employerAccountId);

            var activeVacancySummaries = vacancySummaries.Where(v => v.IsDeleted == false).ToList();

            await _queryStoreWriter.UpdateDashboardAsync(employerAccountId, activeVacancySummaries.OrderBy(v => v.CreatedDate));

            _logger.LogDebug("Update dashboard with {count} summary records for account: {employerAccountId}", activeVacancySummaries.Count(), employerAccountId);
        }
    }
}
