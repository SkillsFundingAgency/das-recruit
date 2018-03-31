using Esfa.Recruit.Vacancies.Client.Application.QueryStore;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.EventHandlers
{
    public class UpdateDashboardOnVacancyChange :   INotificationHandler<VacancyCreatedEvent>,
                                                    INotificationHandler<VacancyUpdatedEvent>,
                                                    INotificationHandler<VacancySubmittedEvent>,
                                                    INotificationHandler<VacancyDeletedEvent>
    {
        private readonly IQueryStoreWriter _queryStore;
        private readonly IVacancyRepository _repository;

        public UpdateDashboardOnVacancyChange(IQueryStoreWriter queryStore, IVacancyRepository repository)
        {
            _queryStore = queryStore;
            _repository = repository;
        }

        public async Task Handle(VacancyUpdatedEvent notification, CancellationToken cancellationToken)
        {
            await ReBuildDashboard(notification.EmployerAccountId);
        }

        public async Task Handle(VacancyCreatedEvent notification, CancellationToken cancellationToken)
        {
            await ReBuildDashboard(notification.EmployerAccountId);
        }

        public async Task Handle(VacancySubmittedEvent notification, CancellationToken cancellationToken)
        {
            await ReBuildDashboard(notification.EmployerAccountId);
        }

        public async Task Handle(VacancyDeletedEvent notification, CancellationToken cancellationToken)
        {
            await ReBuildDashboard(notification.EmployerAccountId);
        }

        private async Task ReBuildDashboard(string employerAccountId)
        {
            var vacancySummaries = await _repository.GetVacanciesByEmployerAccountAsync<VacancySummary>(employerAccountId);

            await _queryStore.UpdateDashboardAsync(employerAccountId, vacancySummaries.OrderBy(v => v.CreatedDate));
        }
    }
}
