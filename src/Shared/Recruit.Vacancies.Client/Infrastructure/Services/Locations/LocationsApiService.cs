using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public async Task<bool?> IsPostcodeInEnglandAsync(string postcode)
    {
        var result = await locationsClient.GetPostcodeData(postcode);
        return result switch
        {
            null => null,
            { Result: null } => null,
            { Result.Country: "England" } => true,
            _ => false
        };
    }

    public async Task<Dictionary<string, PostcodeData>> GetBulkPostcodeDataAsync(List<string> postcodes)
    {
        var response = await locationsClient.GetBulkPostcodeData(postcodes);
        return response?.ToDictionary(x => x.Query, x => x.Result) ?? new Dictionary<string, PostcodeData>();
    }
}