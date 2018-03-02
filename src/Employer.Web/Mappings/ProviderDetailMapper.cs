using Esfa.Recruit.Employer.Web.Models;
using SFA.DAS.Apprenticeships.Api.Types.Providers;
using System.Linq;

namespace Esfa.Recruit.Employer.Web.Mappings
{
    public class ProviderDetailMapper
    {
        public static ProviderDetail MapFromProvider(Provider provider)
        {
            var detail = new ProviderDetail
            {
                Ukprn = provider.Ukprn,
                ProviderName = provider.ProviderName,
                ProviderAddress = GetAddress(provider.Addresses.FirstOrDefault(addr => addr.ContactType.Equals("PRIMARY")) ?? provider.Addresses.First())
            };

            return detail;
        }

        private static string GetAddress(ContactAddress address)
        {
            var arr = new string[] 
            {
                address.Primary,
                address.Secondary,
                address.Street,
                address.Town,
                address.PostCode
            };
            
            return string.Join(", ", arr.Where(x => !string.IsNullOrEmpty(x)));
        }
    }
}
