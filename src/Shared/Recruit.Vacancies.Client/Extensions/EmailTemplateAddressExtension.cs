using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;

namespace Esfa.Recruit.Vacancies.Client.Extensions;

public static class EmailTemplateAddressExtension
{
    public static string GetVacancyLocation(this Vacancy vacancy)
    {
        if (vacancy is null)
        {
            return null;
        }

        switch (vacancy.EmployerLocationOption)
        {
            case AvailableWhere.OneLocation:
                return vacancy.EmployerLocations?.FirstOrDefault().GetOneLocation(vacancy.IsAnonymous);
            case AvailableWhere.MultipleLocations:
                var cityGroups = vacancy.EmployerLocations?.GroupBy(x => x.GetCity()).ToList() ?? [];
                switch (cityGroups)
                {
                    case null:
                    case { Count: 0 }:
                        return null;
                    case { Count: 1 }:
                        var group = cityGroups.First();
                        return $"{group.Key} ({group.Count()} available locations)";
                    case { Count: > 1 }:
                        return string.Join(", ", cityGroups.Select(x => x.Key).Order());
                }
            case AvailableWhere.AcrossEngland:
                return "Recruiting nationally";
            default:
                return vacancy.EmployerLocation.GetOneLocation(vacancy.IsAnonymous);
        }
    }
        
    private static string GetOneLocation(this Address address, bool isAnonymous)
    {
        if (address is null)
        {
            return null;
        }
            
        string city = address.GetCity();
        string postcode = isAnonymous ? address.PostcodeAsOutcode() : address.Postcode;
        return string.IsNullOrWhiteSpace(city)
            ? postcode
            : $"{city} ({postcode})";
    }
        
    private static string GetCity(this Address address)
    {
        return new[]
        {
            address.AddressLine4,
            address.AddressLine3,
            address.AddressLine2,
            // city should never be on line 1
        }.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
    }
}