using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateLiveVacancyOnVacancyChange : INotificationHandler<VacancyApprovedEvent>, INotificationHandler<VacancyPublishedEvent>
    {
        private readonly IVacancyRepository _repository;
        private readonly ILogger<UpdateLiveVacancyOnVacancyChange> _logger;
        private readonly IMessaging _messaging;
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly IQueryStoreWriter _queryStoreWriter;
        private readonly ITimeProvider _timeProvider;

        public UpdateLiveVacancyOnVacancyChange(IQueryStoreWriter queryStoreWriter, ILogger<UpdateLiveVacancyOnVacancyChange> logger, 
            IVacancyRepository repository, IMessaging messaging, IReferenceDataReader referenceDataReader, ITimeProvider timeProvider)
        {
            _queryStoreWriter = queryStoreWriter;
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
            _referenceDataReader = referenceDataReader;
            _timeProvider = timeProvider;
        }
        
        public Task Handle(VacancyApprovedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling {notificationType} for vacancyId: {vacancyId}", notification?.GetType().Name, notification?.VacancyId);
            
            //For now approved vacancies are immediately made Live
            return _messaging.SendCommandAsync(new PublishVacancyCommand
            {
                VacancyId = notification.VacancyId
            });
        }

        public async Task Handle(VacancyPublishedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling VacancyPublishedEvent vacancy {vacancyId}.", notification.VacancyId);

            var vacancyTask = _repository.GetVacancyAsync(notification.VacancyId);
            var programmeTask = _referenceDataReader.GetReferenceData<ApprenticeshipProgrammes>();

            await Task.WhenAll(vacancyTask, programmeTask);

            var vacancy = vacancyTask.Result;
            var programme = vacancy.VacancyType.GetValueOrDefault() == VacancyType.Apprenticeship ? programmeTask.Result.Data.Single(p => p.Id == vacancy.ProgrammeId) : null;

            var liveVacancy = vacancy.ToVacancyProjectionBase<LiveVacancy>(programme, () => QueryViewType.LiveVacancy.GetIdValue(vacancy.VacancyReference.ToString()), _timeProvider);
            _logger.LogInformation("Updating LiveVacancy in query store for vacancy {vacancyId} reference {vacancyReference}.", liveVacancy.VacancyId, liveVacancy.VacancyReference);

            try
            {
                await _queryStoreWriter.UpdateLiveVacancyAsync(liveVacancy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling VacancyPublishedEvent vacancy {vacancyId}.", notification.VacancyId);
                throw;
            }
        }
    }
}
