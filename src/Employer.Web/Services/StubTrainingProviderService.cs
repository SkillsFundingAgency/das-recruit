using Esfa.Recruit.Employer.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class StubTrainingProviderService : ITrainingProviderService
    {
        private readonly IDictionary<long, ProviderDetail> _providerDetails = new Dictionary<long, ProviderDetail>
        {
            { 10004607, new ProviderDetail { Ukprn = 10004607, ProviderName = "College", ProviderAddress = "Address1" } },
            { 10004609, new ProviderDetail { Ukprn = 10004609, ProviderName = "University", ProviderAddress = "Address2" } }
        };
        
        public Task<ProviderDetail> GetProviderDetailAsync(long ukprn)
        {
            return Task.FromResult(_providerDetails[ukprn]);
        }

        public Task<bool> ExistsAsync(long ukprn)
        {
            return Task.FromResult(_providerDetails.ContainsKey(ukprn));
        }
    }
}