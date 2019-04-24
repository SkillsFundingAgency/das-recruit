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
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class VacancyClosedEventHandler : INotificationHandler<VacancyClosedEvent>
    {
        private readonly ILogger<VacancyClosedEventHandler> _logger;
        private readonly IQueryStoreWriter _queryStore;
        private readonly IVacancyRepository _repository;
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly ITimeProvider _timeProvider;

        public VacancyClosedEventHandler(
            ILogger<VacancyClosedEventHandler> logger, IQueryStoreWriter queryStore,
            IVacancyRepository repository, IReferenceDataReader referenceDataReader, ITimeProvider timeProvider)
        {
            _logger = logger;
            _queryStore = queryStore;
            _repository = repository;
            _referenceDataReader = referenceDataReader;
            _timeProvider = timeProvider;
        }

        public async Task Handle(VacancyClosedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting LiveVacancy {vacancyReference} from query store.", notification.VacancyReference);
            await _queryStore.DeleteLiveVacancyAsync(notification.VacancyReference);
            await CreateClosedVacancyProjection(notification.VacancyId);
        }

        private async Task CreateClosedVacancyProjection(Guid vacancyId)
        {
            var vacancyTask = _repository.GetVacancyAsync(vacancyId);
            var programmeTask = _referenceDataReader.GetReferenceData<ApprenticeshipProgrammes>();

            await Task.WhenAll(vacancyTask, programmeTask);

            var vacancy = vacancyTask.Result;
            var programme = programmeTask.Result.Data.Single(p => p.Id == vacancy.ProgrammeId);

            await _queryStore.UpdateClosedVacancyAsync(vacancy.ToVacancyProjectionBase<ClosedVacancy>(programme, () => QueryViewType.ClosedVacancy.GetIdValue(vacancy.VacancyReference.ToString()), _timeProvider));
        }
    }
}
