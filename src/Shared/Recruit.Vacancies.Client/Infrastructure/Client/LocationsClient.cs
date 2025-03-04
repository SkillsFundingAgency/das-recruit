using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

public interface ILocationsClient
{
    Task<GetPostcodeInfoResponse> GetPostcodeInfo(string postcode);
}

public class LocationsClient(IOuterApiClient outerApiClient, ILogger<LocationsService> logger): ILocationsClient
{
    public async Task<GetPostcodeInfoResponse> GetPostcodeInfo(string postcode)
    {
        try
        {
            return await outerApiClient.Get<GetPostcodeInfoResponse>(new GetPostcodeInfoRequest(postcode));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Postcode lookup failed");
            return null;
        }
    }
}