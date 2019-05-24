using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BlockedEmployers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Jobs.Configuration;
using Esfa.Recruit.Vacancies.Jobs.UpdateBlockedEmployers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.QueueTriggers
{
    public class GenerateBlockedEmployersQueueTrigger
    {
        private readonly ILogger<GenerateBlockedEmployersQueueTrigger> _logger;
        private readonly RecruitWebJobsSystemConfiguration _jobsConfig;
        private readonly AccountsReader _accountsReader;
        private readonly IReferenceDataWriter _referenceDataWriter;

        public GenerateBlockedEmployersQueueTrigger(ILogger<GenerateBlockedEmployersQueueTrigger> logger, RecruitWebJobsSystemConfiguration jobsConfig, AccountsReader accountsReader, IReferenceDataWriter referenceDataWriter)
        {
            _logger = logger;
            _jobsConfig = jobsConfig;
            _accountsReader = accountsReader;
            _referenceDataWriter = referenceDataWriter;
        }

        public async Task GenerateBlockedEmployersAsync([QueueTrigger(QueueNames.GenerateBlockedEmployersQueueName, Connection = "QueueStorage")] string message, TextWriter log)
        {
            if (_jobsConfig.DisabledJobs.Contains(this.GetType().Name))
            {
                _logger.LogDebug($"{this.GetType().Name} is disabled, skipping ...");
                return;
            }

            _logger.LogInformation("Starting rebuilding blocked employers reference data.");

            var accountsTask = _accountsReader.GetEmployerAccountsAsync();
            var levyPayersTask = _accountsReader.GetLevyPayerAccountIdsAsync();

            await Task.WhenAll(accountsTask, levyPayersTask);

            var accounts = accountsTask.Result;
            var levyPayers = levyPayersTask.Result;

            const string id = "2";
            if (levyPayers.Contains(id) == false)
            {
                levyPayers.Add(id);
            }

            var nonLevyPayers = accounts.Where(x => levyPayers.Contains(x.AccountId) == false)
                                        .Select(x => x.HashedAccountId)
                                        .ToList();

            await _referenceDataWriter.UpsertReferenceData<BlockedEmployers>(new BlockedEmployers
            {
                Id = nameof(BlockedEmployers),
                EmployerAccountIds = nonLevyPayers
            });

            _logger.LogInformation("Finished rebuilding blocked employers reference data.");
        }
    }
}