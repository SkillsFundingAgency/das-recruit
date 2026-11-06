using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders;

public class TrainingProvider
{
    public string Name { get; set; }
    public long Ukprn { get; set; }
    public TrainingProviderAddress Address { get; set; }

    public static implicit operator TrainingProvider(GetProviderResponseItem source)
    {
        return new TrainingProvider
        {
            Name = source.Name,
            Ukprn = source.Ukprn,
            Address = source.Address
        };
    }
}
public class TrainingProviderAddress : IAddress
{
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string AddressLine3 { get; set; }
    public string AddressLine4 { get; set; }
    public string Postcode { get; set; }

    public static implicit operator TrainingProviderAddress(ProviderAddress source)
    {
        string address3 = source.Address3;
        if (!string.IsNullOrEmpty(source.Address4))
        {
            if (!string.IsNullOrEmpty(address3))
            {
                address3 += $", {source.Address4}";
            }
            else
            {
                address3 = source.Address4;
            }
        }

        return new TrainingProviderAddress
        {
            AddressLine1 = source.Address1,
            AddressLine2 = source.Address2,
            AddressLine3 = address3,
            AddressLine4 = source.Town,
            Postcode = source.Postcode
        };
    }
}