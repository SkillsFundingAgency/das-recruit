using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Jobs.VacancyStatus
{
    public class LiveVacancyStatusInspector
    {
        private readonly ILogger<LiveVacancyStatusInspector> _logger;
        private readonly IJobsVacancyClient _client;
        private readonly ITimeProvider _timeProvider;

        public LiveVacancyStatusInspector(ILogger<LiveVacancyStatusInspector> logger, IJobsVacancyClient client, ITimeProvider timeProvider)
        {
            _logger = logger;
            _client = client;
            _timeProvider = timeProvider;
        }

        internal async Task InspectAsync()
        {
            var vacancies = (await _client.GetLiveVacancies()).ToList();
            int numberClosed = 0;
            
            foreach (var vacancy in vacancies)
            {
                if (vacancy.ClosingDate <= _timeProvider.Now)
                {
                    _logger.LogInformation($"Closing vacancy {vacancy.VacancyReference} with closing date of {vacancy.ClosingDate}");
                    await _client.CloseVacancy(vacancy.VacancyId);
                    numberClosed++;
                }
            }
            
            _logger.LogInformation("Closed {closedCount} from {liveVacancyCount} live vacancies", numberClosed, vacancies.Count);

            await Task.CompletedTask;
        }
    }
}