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
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class VacancyClosedEventHandler : INotificationHandler<VacancyClosedEvent>
    {
        private readonly ILogger<VacancyClosedEventHandler> _logger;
        private readonly IQueryStoreWriter _queryStore;
        private readonly IVacancyRepository _repository;
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly ITimeProvider _timeProvider;
        private readonly ICommunicationQueueService _communicationQueueService;
        private readonly IQueryStoreReader _queryStoreReader;

        public VacancyClosedEventHandler(
            ILogger<VacancyClosedEventHandler> logger, IQueryStoreWriter queryStore,
            IVacancyRepository repository, IReferenceDataReader referenceDataReader, ITimeProvider timeProvider, ICommunicationQueueService communicationQueueService, IQueryStoreReader queryStoreReader)
        {
            _logger = logger;
            _queryStore = queryStore;
            _repository = repository;
            _referenceDataReader = referenceDataReader;
            _timeProvider = timeProvider;
            _communicationQueueService = communicationQueueService;
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
            var vacancyTask = _repository.GetVacancyAsync(vacancyId);
            var programmeTask = _referenceDataReader.GetReferenceData<ApprenticeshipProgrammes>();

            await Task.WhenAll(vacancyTask, programmeTask);
            var vacancy = vacancyTask.Result;
            
            var queryResult = await _queryStoreReader.GetClosedVacancy(vacancy.VacancyReference.Value);

            if (queryResult != null)
            {
                _logger.LogInformation($"Vacancy {vacancy.VacancyReference} already closed. Skipping notification.");
                return;
            }
            
            var programme = vacancy.VacancyType.GetValueOrDefault() == VacancyType.Apprenticeship ? programmeTask.Result.Data.FirstOrDefault(p => p.Id == vacancy.ProgrammeId) : null;

            await _queryStore.UpdateClosedVacancyAsync(vacancy.ToVacancyProjectionBase<ClosedVacancy>(programme, () => QueryViewType.ClosedVacancy.GetIdValue(vacancy.VacancyReference.ToString()), _timeProvider));

            if (vacancy.ClosureReason == ClosureReason.WithdrawnByQa)
            {
                _logger.LogInformation($"Queuing up withdrawn notification message for vacancy {vacancy.VacancyReference}");
                var communicationRequest = GetVacancyWithdrawnByQaCommunicationRequest(vacancy.VacancyReference.Value);
                await _communicationQueueService.AddMessageAsync(communicationRequest);
            }
        }

        private CommunicationRequest GetVacancyWithdrawnByQaCommunicationRequest(long vacancyReference)
        {
            var communicationRequest = new CommunicationRequest(
                CommunicationConstants.RequestType.VacancyWithdrawnByQa,
                CommunicationConstants.ParticipantResolverNames.VacancyParticipantsResolverName,
                CommunicationConstants.ServiceName);

            communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Vacancy, vacancyReference);
            communicationRequest.AddEntity(CommunicationConstants.EntityTypes.ApprenticeshipServiceUrl, vacancyReference);
            return communicationRequest;
        }

    }
}
