using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
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
        private readonly IVacancyRepository _vacancyRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IMessaging _messaging;

        public VacancyService(
            ILogger<VacancyService> logger, IVacancyRepository vacancyRepository, 
            ITimeProvider timeProvider, IMessaging messaging)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _timeProvider = timeProvider;
            _messaging = messaging;
        }

        public async Task CloseExpiredVacancy(Guid vacancyId)
        {
            _logger.LogInformation("Closing vacancy {vacancyId}.", vacancyId);

            var vacancy = await _vacancyRepository.GetVacancyAsync(vacancyId);
            vacancy.ClosureReason = ClosureReason.Auto;

            vacancy.ClosedDate = _timeProvider.Now;
            vacancy.Status = VacancyStatus.Closed;

            await _vacancyRepository.UpdateAsync(vacancy);

            await _messaging.PublishEvent(new VacancyClosedEvent
            {
                VacancyReference = vacancy.VacancyReference.Value,
                VacancyId = vacancy.Id
            });
        }
    }
}