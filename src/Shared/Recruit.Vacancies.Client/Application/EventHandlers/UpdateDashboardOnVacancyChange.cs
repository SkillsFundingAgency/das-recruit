using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class UpdateDashboardOnVacancyChange : INotificationHandler<VacancyCreatedEvent>,
                                                    INotificationHandler<VacancyUpdatedEvent>,
                                                    INotificationHandler<VacancySubmittedEvent>,
                                                    INotificationHandler<VacancyDeletedEvent>
    {
        private readonly ICreateDashboards _dashboardService;
        private readonly ILogger<UpdateDashboardOnVacancyChange> _logger;

        public UpdateDashboardOnVacancyChange(ICreateDashboards dashboardService, ILogger<UpdateDashboardOnVacancyChange> logger)
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

        public async Task Handle(VacancyCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling VacancyCreatedEvent for accountId: {employerAccountId}", notification?.EmployerAccountId);
            await ReBuildDashboard(notification.EmployerAccountId);
        }

        public async Task Handle(VacancyUpdatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling VacancyUpdatedEvent for accountId: {employerAccountId}", notification?.EmployerAccountId);
            await ReBuildDashboard(notification.EmployerAccountId);
        }

        public async Task Handle(VacancySubmittedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling VacancySubmittedEvent for accountId: {employerAccountId}", notification?.EmployerAccountId);
            await ReBuildDashboard(notification.EmployerAccountId);
        }

        public async Task Handle(VacancyDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling VacancyDeletedEvent for accountId: {employerAccountId}", notification?.EmployerAccountId);
            await ReBuildDashboard(notification.EmployerAccountId);
        }

        private Task ReBuildDashboard(string employerAccountId)
        {
            return _dashboardService.BuildDashboard(employerAccountId);
        }
    }
}
