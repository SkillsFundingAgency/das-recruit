using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Shared.Web.Services;

public class TrainingProviderAgreementService(IGetProviderStatusClient providerStatusClient)
    : ITrainingProviderAgreementService
{
    public async Task<bool> HasAgreementAsync(long ukprn)
    {
        //Agreement may have been signed since the projection was created. Check PAS.
        var providerAccountResponse = await providerStatusClient.GetProviderStatus(ukprn);

        return providerAccountResponse.CanAccessService;
    }
}