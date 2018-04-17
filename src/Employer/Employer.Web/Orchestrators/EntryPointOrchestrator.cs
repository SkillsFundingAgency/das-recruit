using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class EntryPointOrchestrator
    {
        private readonly IEmployerVacancyClient _client;

        public EntryPointOrchestrator(IEmployerVacancyClient client)
        {
            _client = client;
        }

        public Task RecordUserSignInAsync(string accountId)
        {
            return _client.RecordEmployerAccountSignInAsync(accountId);
        }
    }
}