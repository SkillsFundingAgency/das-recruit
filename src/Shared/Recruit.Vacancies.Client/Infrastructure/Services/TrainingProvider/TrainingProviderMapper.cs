using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using SFA.DAS.Apprenticeships.Api.Types.Providers;
using System.Linq;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider
{
    public static class TrainingProviderMapper
    {
        public static Domain.Entities.TrainingProvider MapFromApiProvider(Provider provider)
        {
            return new Domain.Entities.TrainingProvider
            {
                Ukprn = provider.Ukprn,
                Name = provider.ProviderName,
                Address = GetAddress(provider.Addresses.FirstOrDefault(addr => addr.ContactType.Equals("PRIMARY")) ?? provider.Addresses.First())
            };
        }
        
        private static Address GetAddress(ContactAddress address)
        {
            return new Address
            {
                AddressLine1 = address.Primary,
                AddressLine2 = address.Secondary,
                AddressLine3 = address.Street,
                AddressLine4 = address.Town,
                Postcode = address.PostCode
            };
        }
    }
}
