using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BlockedEmployers
{
    public class BlockedEmployersProvider : IBlockedEmployersProvider
    {
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly ILogger<BlockedEmployersProvider> _logger;

        public BlockedEmployersProvider(IReferenceDataReader referenceDataReader, ILogger<BlockedEmployersProvider> logger)
        {
            _referenceDataReader = referenceDataReader;
            _logger = logger;
        }

        public async Task<IList<string>> GetBlockedEmployerAccountIdsAsync()
        {
            var blockedEmployers = await _referenceDataReader.GetReferenceData<BlockedEmployers>();

            _logger.LogInformation($"Found {blockedEmployers.EmployerAccountIds.Count()} blocked employers.");

            return blockedEmployers.EmployerAccountIds.Select(s => s.ToUpper()).ToList();
        }
    }
}