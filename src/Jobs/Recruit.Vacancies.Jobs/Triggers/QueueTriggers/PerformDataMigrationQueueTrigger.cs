using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Azure.WebJobs;
using System.Linq;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System;
using SFA.DAS.EAS.Account.Api.Types;
using Polly;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers
{
    public class PerformDataMigrationQueueTrigger
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IEmployerAccountProvider _employerAccountProvider;
        private readonly ILogger<PerformDataMigrationQueueTrigger> _logger;
        public PerformDataMigrationQueueTrigger(
            IVacancyRepository vacancyRepository, 
            IEmployerAccountProvider employerAccountProvider,
            ILogger<PerformDataMigrationQueueTrigger> logger)
        {
            _vacancyRepository = vacancyRepository;
            _employerAccountProvider = employerAccountProvider;
            _logger = logger;
        }

        public async Task ExecuteAsync(
            [QueueTrigger(QueueNames.DataMigrationQueueName, Connection = "QueueStorage")] DataMigrationQueueMessage message, 
            TextWriter log)
        {
            await PerformVacancyALEIdMigration(message);
        }

        private async Task PerformVacancyALEIdMigration(DataMigrationQueueMessage message)
        {
            var vacancyId = message.VacancyId;

            _logger.LogInformation($"{message.SerialNumber}: Carrying out ALEId migration on vacancy {vacancyId} ");
            Vacancy vacancy;
            try
            {
                vacancy = await _vacancyRepository.GetVacancyAsync(vacancyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{message.SerialNumber}: Error fetching vacancy - Bypassing vacancy {vacancyId}");
                return;
            }

            if (vacancy == null)
            {
                _logger.LogWarning($"{message.SerialNumber}: Not found - Bypassing vacancy {vacancyId}");
                return;
            }

            if (vacancy.AccountLegalEntityPublicHashedId == "0") 
            {
                _logger.LogWarning($"{message.SerialNumber}: Missing AccountLegalEntityPublicHashedId - Bypassing vacancy {vacancyId}");
                return;
            }

            if (string.IsNullOrWhiteSpace(vacancy.AccountLegalEntityPublicHashedId) == false)
            {
                _logger.LogWarning($"{message.SerialNumber}: Already updated - Bypassing vacancy {vacancyId}");
                return;
            }

            LegalEntityViewModel selectedLegalEntity;
            try
            {
                var retryPolicy = GetApiRetryPolicy();
                var legalEntities = await retryPolicy.Execute(context => 
                    _employerAccountProvider.GetLegalEntitiesConnectedToAccountAsync(vacancy.EmployerAccountId), 
                    new Dictionary<string, object>() {{ "apiCall", "employer details" }});
                selectedLegalEntity = legalEntities.FirstOrDefault(l => l.AccountLegalEntityPublicHashedId == vacancy.AccountLegalEntityPublicHashedId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{message.SerialNumber}: Error fetching legal entity details - Bypassing vacancy {vacancyId}");
                throw;
            }

            if (selectedLegalEntity == null)
            {
                _logger.LogError($"Unable to find legal entity for vacancy {vacancyId}");
                return;
            }

            _logger.LogInformation($"Updating vacancy: {vacancyId} setting AccountLegalEntityPublicHashedId: {selectedLegalEntity.AccountLegalEntityPublicHashedId}");
            vacancy.AccountLegalEntityPublicHashedId = selectedLegalEntity.AccountLegalEntityPublicHashedId;
            await _vacancyRepository.UpdateAsync(vacancy);
            _logger.LogInformation($"Successfully updated vacancy: {vacancyId} with AccountLegalEntityPublicHashedId: {selectedLegalEntity.AccountLegalEntityPublicHashedId}");
        }

        private Polly.Retry.RetryPolicy GetApiRetryPolicy()
        {
            return Policy
                    .Handle<Exception>()
                    .WaitAndRetry(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(15)
                    }, (exception, timeSpan, retryCount, context) => {
                        _logger.LogWarning(exception, $"Error connecting to Apprenticeships Api for {context["apiCall"]}. Retrying in {timeSpan.Seconds} secs...attempt: {retryCount}");
                    });
        }
    }
}