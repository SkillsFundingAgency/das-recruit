using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Jobs.ApprenticeshipProgrammes
{
    public class LiveVacancyStatusInspector
    {
        private readonly ILogger<LiveVacancyStatusInspector> _logger;
        private readonly IJobsVacancyClient _client;

        public LiveVacancyStatusInspector(ILogger<LiveVacancyStatusInspector> logger, IJobsVacancyClient client)
        {
            _logger = logger;
            _client = client;
        }

        internal async Task InspectAsync()
        {
            var vacancies = await _client.GetLiveVacancies();

            foreach (var vacancy in vacancies)
            {
                if (vacancy.ClosingDate <= DateTime.Today.ToUniversalTime())
                {
                    _logger.LogInformation($"Closing vacancy {vacancy.VacancyReference} with closing date of {vacancy.ClosingDate}");
                    await _client.CloseVacancy(vacancy.VacancyId);
                }
            }

            await Task.CompletedTask;
        }
    }
}