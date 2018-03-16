using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class EntryPointOrchestrator
    {
        private readonly ILogger<EntryPointOrchestrator> _logger;
        private readonly IVacancyClient _client;

        public EntryPointOrchestrator(ILogger<EntryPointOrchestrator> logger, IVacancyClient client)
        {
            _logger = logger;
            _client = client;
        }

        public Task RecordUserSignIn(string accountId)
        {
            return _client.RecordEmployerAccountSignIn(accountId);
        }
    }
}