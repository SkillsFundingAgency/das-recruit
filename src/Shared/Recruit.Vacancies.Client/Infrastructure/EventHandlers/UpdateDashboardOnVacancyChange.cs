using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
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
                                                    INotificationHandler<VacancyLiveEvent>,
                                                    INotificationHandler<VacancyClosedEvent>
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

        public Task Handle(VacancyCreatedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyUpdatedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancySubmittedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyDeletedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyLiveEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }

        public Task Handle(VacancyClosedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification);
        }
        
        private Task Handle(IVacancyEvent notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification), "Should not be null");
            
            _logger.LogInformation($"Handling {notification.GetType().Name} for accountId: {{employerAccountId}} and vacancyId: {notification.VacancyId}", notification.EmployerAccountId);
            return ReBuildDashboard(notification.EmployerAccountId);
        }

        private async Task ReBuildDashboard(string employerAccountId)
        {
            var vacancySummaries = await _repository.GetVacanciesByEmployerAccountAsync<VacancySummary>(employerAccountId);

            var activeVacancySummaries = vacancySummaries.Where(v => v.IsDeleted == false).ToList();

            foreach(var summary in activeVacancySummaries)
            {
                // PendingReview shows as Submitted in Dashboard.
                if (summary.Status == VacancyStatus.PendingReview)
                {
                    summary.Status = VacancyStatus.Submitted;
                }
            }

            await _queryStoreWriter.UpdateDashboardAsync(employerAccountId, activeVacancySummaries.OrderBy(v => v.CreatedDate));

            _logger.LogDebug("Update dashboard with {count} summary records for account: {employerAccountId}", activeVacancySummaries.Count, employerAccountId);
        }
    }
}
