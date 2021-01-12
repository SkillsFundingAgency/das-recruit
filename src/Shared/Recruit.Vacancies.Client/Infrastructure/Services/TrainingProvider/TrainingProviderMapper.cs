using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider
{
    public static class TrainingProviderMapper
    {
        public static Domain.Entities.TrainingProvider MapFromApiProvider(ReferenceData.TrainingProviders.TrainingProvider provider)
        {
            return new Domain.Entities.TrainingProvider
            {
                Ukprn = provider.Ukprn,
                Name = provider.Name,
                Address = GetAddress(provider.Address)
            };
        }
        
        private static Address GetAddress(TrainingProviderAddress address)
        {
            return new Address
            {
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                AddressLine3 = address.AddressLine3,
                AddressLine4 = address.AddressLine4,
                Postcode = address.Postcode
            };
        }
    }
}
