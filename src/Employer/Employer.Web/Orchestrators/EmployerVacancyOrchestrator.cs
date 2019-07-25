using System.Linq;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class EmployerVacancyOrchestrator
    {
        private readonly IRecruitVacancyClient _vacancyClient;

        public EmployerVacancyOrchestrator(IRecruitVacancyClient vacancyClient)
        {
            _vacancyClient = vacancyClient;
        }
        public async Task<TrainingProviderSummary> GetProviderUkprn(string ukprn)
        {
            long.TryParse(ukprn, out long validUkprn);
            var providers = await _vacancyClient.GetAllTrainingProvidersAsync();
            return providers.SingleOrDefault(p => p.Ukprn == validUkprn);
        }

        public async Task<IApprenticeshipProgramme> GetProgrammeId(string programmeId)
        {
            var programmes = await _vacancyClient.GetActiveApprenticeshipProgrammesAsync();
            return programmes.SingleOrDefault(p => p.Id == programmeId);
        }
    }
}