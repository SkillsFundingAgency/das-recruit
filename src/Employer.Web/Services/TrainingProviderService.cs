using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Models;
using SFA.DAS.Providers.Api.Client;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class TrainingProviderService : ITrainingProviderService
    {
        private readonly IProviderApiClient _providerClient;

        public TrainingProviderService(IProviderApiClient providerClient)
        {
            _providerClient = providerClient;
        }

        public async Task<ProviderDetail> GetProviderDetailAsync(long ukprn)
        {
            var provider = await _providerClient.GetAsync(ukprn);
            return ProviderDetailMapper.MapFromProvider(provider);
        }

        public async Task<bool> ExistsAsync(long ukprn)
        {
            return await _providerClient.ExistsAsync(ukprn);
        }
    }
}