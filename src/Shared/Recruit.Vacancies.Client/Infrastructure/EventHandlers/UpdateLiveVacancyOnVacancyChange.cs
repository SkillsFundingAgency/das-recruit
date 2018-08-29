using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
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

        public UpdateLiveVacancyOnVacancyChange(IQueryStoreWriter queryStoreWriter, ILogger<UpdateLiveVacancyOnVacancyChange> logger, 
            IVacancyRepository repository, IMessaging messaging, IReferenceDataReader referenceDataReader)
        {
            _queryStoreWriter = queryStoreWriter;
            _logger = logger;
            _repository = repository;
            _messaging = messaging;
            _referenceDataReader = referenceDataReader;
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
            var vacancyTask = _repository.GetVacancyAsync(notification.VacancyId);
            var programmeTask = _referenceDataReader.GetReferenceData<ApprenticeshipProgrammes>();

            await Task.WhenAll(vacancyTask, programmeTask);

            var vacancy = vacancyTask.Result;
            var programme = programmeTask.Result.Data.Single(p => p.Id == vacancy.ProgrammeId);

            await _queryStoreWriter.UpdateLiveVacancyAsync(vacancy.ToLiveVacancyProjection(programme));
        }
    }
}
