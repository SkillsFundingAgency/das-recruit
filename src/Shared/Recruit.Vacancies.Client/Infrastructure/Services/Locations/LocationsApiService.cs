using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;

public interface ILocationsService
{
    Task<bool?> IsPostcodeInEnglandAsync(string postcode);
}

public class LocationsService(ILocationsClient locationsClient) : ILocationsService
{
    public async Task<bool?> IsPostcodeInEnglandAsync(string postcode)
    {
        var result = await locationsClient.GetPostcodeInfo(postcode);
        return result switch
        {
            null => null,
            { Country: "England" } => true,
            _ => false
        };
    }
}