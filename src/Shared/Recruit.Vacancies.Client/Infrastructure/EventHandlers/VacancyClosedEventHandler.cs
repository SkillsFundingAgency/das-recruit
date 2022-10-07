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
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA;
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
        private readonly IFaaService _faaService;
        private readonly ICommunicationQueueService _communicationQueueService;

        public VacancyClosedEventHandler(
            ILogger<VacancyClosedEventHandler> logger, IQueryStoreWriter queryStore,
            IVacancyRepository repository, IReferenceDataReader referenceDataReader, ITimeProvider timeProvider,
            IFaaService faaService, ICommunicationQueueService communicationQueueService)
        {
            _logger = logger;
            _queryStore = queryStore;
            _repository = repository;
            _referenceDataReader = referenceDataReader;
            _timeProvider = timeProvider;
            _faaService = faaService;
            _communicationQueueService = communicationQueueService;
        }

        public async Task Handle(VacancyClosedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting LiveVacancy {vacancyReference} from query store.",
                    notification.VacancyReference);
            await NotifyFaaVacancyHasClosed(notification);
            await _queryStore.DeleteLiveVacancyAsync(notification.VacancyReference);
            await CreateClosedVacancyProjection(notification.VacancyId);
        }

        private Task NotifyFaaVacancyHasClosed(VacancyClosedEvent notification)
        {
            var message = new FaaVacancyStatusSummary(notification.VacancyReference, FaaVacancyStatuses.Expired, _timeProvider.Now);
            return _faaService.PublishVacancyStatusSummaryAsync(message);
        }

        private async Task CreateClosedVacancyProjection(Guid vacancyId)
        {
            var vacancyTask = _repository.GetVacancyAsync(vacancyId);
            var programmeTask = _referenceDataReader.GetReferenceData<ApprenticeshipProgrammes>();

            await Task.WhenAll(vacancyTask, programmeTask);

            var vacancy = vacancyTask.Result;
            var programme = vacancy.VacancyType.GetValueOrDefault() == VacancyType.Apprenticeship ? programmeTask.Result.Data.SingleOrDefault(p => p.Id == vacancy.ProgrammeId) : null;

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
