using System;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class VacancyClosedEventHandler : INotificationHandler<VacancyClosedEvent>
    {
        private readonly ILogger<VacancyClosedEventHandler> _logger;
        private readonly IQueryStoreWriter _queryStore;
        private readonly IVacancyRepository _repository;
        private readonly IApprenticeshipProgrammeProvider _apprenticeshipProgrammeProvider;
        private readonly ITimeProvider _timeProvider;
        private readonly IQueryStoreReader _queryStoreReader;

        public VacancyClosedEventHandler(
            ILogger<VacancyClosedEventHandler> logger, IQueryStoreWriter queryStore,
            IVacancyRepository repository, IApprenticeshipProgrammeProvider apprenticeshipProgrammeProvider, ITimeProvider timeProvider, IQueryStoreReader queryStoreReader)
        {
            _logger = logger;
            _queryStore = queryStore;
            _repository = repository;
            _apprenticeshipProgrammeProvider = apprenticeshipProgrammeProvider;
            _timeProvider = timeProvider;
            _queryStoreReader = queryStoreReader;
        }

        public async Task Handle(VacancyClosedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting LiveVacancy {vacancyReference} from query store.",
                    notification.VacancyReference);
            
            await _queryStore.DeleteLiveVacancyAsync(notification.VacancyReference);
            await CreateClosedVacancyProjection(notification.VacancyId);
        }

        private async Task CreateClosedVacancyProjection(Guid vacancyId)
        {
            var vacancy = await _repository.GetVacancyAsync(vacancyId);
            
            var queryResult = await _queryStoreReader.GetClosedVacancy(vacancy.VacancyReference.Value);

            if (queryResult != null)
            {
                _logger.LogInformation($"Vacancy {vacancy.VacancyReference} already closed. Skipping notification.");
                return;
            }
            
            var programme = await _apprenticeshipProgrammeProvider.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);

            await _queryStore.UpdateClosedVacancyAsync(vacancy.ToVacancyProjectionBase<ClosedVacancy>((ApprenticeshipProgramme)programme, () => QueryViewType.ClosedVacancy.GetIdValue(vacancy.VacancyReference.ToString()), _timeProvider));
        }
    }
}
