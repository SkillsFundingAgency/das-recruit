using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

public interface ILocationsClient
{
    Task<GetPostcodeDataResponse> GetPostcodeData(string postcode);
    Task<GetBulkPostcodeDataResponse> GetBulkPostcodeData(List<string> postcodes);
}

public class LocationsClient(IOuterApiClient outerApiClient, ILogger<LocationsService> logger): ILocationsClient
{
    public async Task<GetPostcodeDataResponse> GetPostcodeData(string postcode)
    {
        try
        {
            return await outerApiClient.Get<GetPostcodeDataResponse>(new GetPostcodeDataRequest(postcode));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred looking up postcode: {postcode}", postcode);
            return null;
        }
    }

    public async Task<GetBulkPostcodeDataResponse> GetBulkPostcodeData(List<string> postcodes)
    {
        ArgumentNullException.ThrowIfNull(postcodes);
        try
        {
            return await outerApiClient.Post<GetBulkPostcodeDataResponse>(new GetBulkPostcodeDataRequest(postcodes));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred looking up postcodes: {postcodes}", string.Join(",", postcodes));
            return null;
        }
    }
}