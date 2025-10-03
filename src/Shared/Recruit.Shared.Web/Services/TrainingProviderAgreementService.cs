using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Shared.Web.Services
{
    public class TrainingProviderAgreementService : ITrainingProviderAgreementService
    {
        private readonly IProviderVacancyClient _client;
        private readonly IGetProviderStatusClient _providerStatusClient;

        public TrainingProviderAgreementService(IProviderVacancyClient client, IGetProviderStatusClient providerStatusClient)
        {
            _client = client;
            _providerStatusClient = providerStatusClient;
        }

        public async Task<bool> HasAgreementAsync(long ukprn)
        {
            var editVacancyInfo = await _client.GetProviderEditVacancyInfoAsync(ukprn);

            if (editVacancyInfo == null)
                return false;

            if (editVacancyInfo.HasProviderAgreement)
                return true;

            //Agreement may have been signed since the projection was created. Check PAS.
            var providerAccountResponse = await _providerStatusClient.GetProviderStatus(ukprn);

            if (providerAccountResponse.CanAccessService)
            {
                await _client.SetupProviderAsync(ukprn);
            }

            return providerAccountResponse.CanAccessService;
        }
    }
}
