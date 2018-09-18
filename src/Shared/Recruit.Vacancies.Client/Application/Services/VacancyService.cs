using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public class VacancyService : IVacancyService
    {
        private readonly ILogger<VacancyService> _logger;
        private readonly IVacancyRepository _repository;
        private readonly ITimeProvider _timeProvider;
        private readonly IMessaging _messaging;
        

        public VacancyService(ILogger<VacancyService> logger, IVacancyRepository repository, ITimeProvider timeProvider, IMessaging messaging)
        {
            _logger = logger;
            _repository = repository;
            _timeProvider = timeProvider;
            _messaging = messaging;
        }

        public async Task CloseVacancy(Guid vacancyId)
        {
            _logger.LogInformation("Closing vacancy {vacancyId}.", vacancyId);

            var vacancy = await _repository.GetVacancyAsync(vacancyId);

            vacancy.ClosedDate = _timeProvider.Now;
            vacancy.Status = VacancyStatus.Closed;

            await _repository.UpdateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyClosedEvent
            {
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyReference = vacancy.VacancyReference.Value,
                VacancyId = vacancy.Id
            });
        }
    }
}