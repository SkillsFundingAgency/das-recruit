using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.BlockedEmployers
{
    public class BlockedEmployersProvider : IBlockedEmployersProvider
    {
        private readonly IConfigurationReader _configurationReader;

        public BlockedEmployersProvider(IConfigurationReader configurationReader)
        {
            _configurationReader = configurationReader;
        }

        public async Task<List<string>> GetBlockedEmployerAccountIdsAsync()
        {
            var blockedEmployers = await _configurationReader.GetAsync<Application.Configuration.BlockedEmployers>("BlockedEmployers");

            return blockedEmployers.EmployerAccountIds.Select(s => s.ToUpper()).ToList();
        }
    }
}
