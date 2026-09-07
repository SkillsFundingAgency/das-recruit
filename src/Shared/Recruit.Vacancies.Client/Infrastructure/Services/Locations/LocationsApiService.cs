using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;

public interface ILocationsService
{
    Task<bool?> IsPostcodeInEnglandAsync(string postcode);
    Task<Dictionary<string, PostcodeData>> GetBulkPostcodeDataAsync(List<string> postcodes);
}

public class LocationsService(ILocationsClient locationsClient) : ILocationsService
{
    public async Task<Dictionary<string, PostcodeData>> GetBulkPostcodeDataAsync(List<string> postcodes)
    {
        var response = await locationsClient.GetBulkPostcodeData(postcodes);
        return response?.Distinct().ToDictionary(x => x.Query, x => x.Result) ?? new Dictionary<string, PostcodeData>();
    }

    public async Task<bool?> IsPostcodeInEnglandAsync(string postcode)
    {
        var result = await GetCountryForPostcodeAsync(postcode);
        if (result != null)
        {
            return result;
        }

        var outerPostcode = GetOuterPostcode(postcode);
        if (!string.IsNullOrEmpty(outerPostcode) && !outerPostcode.Equals(postcode, StringComparison.OrdinalIgnoreCase))
        {
            return await GetCountryForPostcodeAsync(outerPostcode);
        }

        return null;
    }

    // Returns true if the postcode is in England, false if it is not, and null if the postcode is invalid or not found.
    private async Task<bool?> GetCountryForPostcodeAsync(string postcode)
    {
        var result = await locationsClient.GetPostcodeData(postcode);
        return result switch
        {
            null => null,
            { Result: null } => null,
            { Result.Country: Constants.EnglandCountryCode } => true,
            _ => false
        };
    }

    // The outer postcode is the part of the postcode before the inward code (the last 3 characters). For example, for postcode "SW1A 1AA", the outer postcode is "SW1A".
    private static string GetOuterPostcode(string postcode)
    {
        if (string.IsNullOrWhiteSpace(postcode))
        {
            return string.Empty;
        }

        var normalised = postcode.Replace(" ", "").Trim();

        // UK inward code is always 3 characters (digit + letter + letter),
        // so the outer code is everything before that.
        return normalised.Length > 3
            ? normalised[..^3]
            : normalised;
    }
}