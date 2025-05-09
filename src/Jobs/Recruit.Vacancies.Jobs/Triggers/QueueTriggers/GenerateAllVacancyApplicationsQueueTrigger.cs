using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class GenerateAllVacancyApplicationsQueueTrigger
    {
        private readonly IVacancyApplicationsProjectionService _projectionService;
        private readonly ILogger<GenerateAllVacancyApplicationsQueueTrigger> _logger;
        private readonly IApplicationReviewQuery _query;

        private string JobName => GetType().Name;

        public GenerateAllVacancyApplicationsQueueTrigger(ILogger<GenerateAllVacancyApplicationsQueueTrigger> logger, IVacancyApplicationsProjectionService projectionService, IApplicationReviewQuery query)
        {
            _logger = logger;
            _projectionService = projectionService;
            _query = query;
        }

        public async Task GenerateAllVacancyApplicationsAsync([QueueTrigger(QueueNames.GenerateAllVacancyApplicationsQueueName, Connection = "QueueStorage")]
            string message, TextWriter log)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    _logger.LogInformation($"Start {JobName}");

                    var allVacancyReferences = (await _query.GetAllVacancyReferencesAsync()).ToList();

                    _logger.LogInformation($"Regenerating {allVacancyReferences.Count()} VacancyApplications queryStore documents for:\n{string.Join(Environment.NewLine, allVacancyReferences)}");

                    foreach (var vacancyReference in allVacancyReferences)
                    {
                        await _projectionService.UpdateVacancyApplicationsAsync(vacancyReference);
                    }

                    _logger.LogInformation($"Finished {JobName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to run {JobName}.");
                throw;
            }
        }
    }
}
