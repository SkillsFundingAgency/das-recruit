using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class EntryPointOrchestrator
    {
        private readonly ILogger<EntryPointOrchestrator> _logger;
        private readonly IEmployerVacancyClient _client;

        public EntryPointOrchestrator(ILogger<EntryPointOrchestrator> logger, IEmployerVacancyClient client)
        {
            _logger = logger;
            _client = client;
        }

        public Task RecordUserSignInAsync(string accountId)
        {
            return _client.RecordEmployerAccountSignInAsync(accountId);
        }
    }
}