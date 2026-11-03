using System.Linq;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Orchestrators;

public class EmployerVacancyOrchestrator(
    IRecruitVacancyClient vacancyClient,
    ITrainingProviderSummaryProvider trainingProviderSummaryProvider,
    IEmployerVacancyClient employerVacancyClient)
{
    private readonly IEmployerVacancyClient _employerVacancyClient = employerVacancyClient;

    public async Task<TrainingProviderSummary> GetProviderUkprn(string ukprn)
    {
        if (long.TryParse(ukprn, out long validUkprn) == false)
            return null;
        var providers = await trainingProviderSummaryProvider.FindAllAsync();
        return providers.SingleOrDefault(p => p.Ukprn == validUkprn);
    }

    public async Task<IApprenticeshipProgramme> GetProgrammeId(string programmeId)
    {
        if (string.IsNullOrWhiteSpace(programmeId))
            return null;
        var programmes = await vacancyClient.GetActiveApprenticeshipProgrammesAsync(includePlaceholderProgramme:true);
        return programmes.SingleOrDefault(p => p.Id == programmeId);
    }
}