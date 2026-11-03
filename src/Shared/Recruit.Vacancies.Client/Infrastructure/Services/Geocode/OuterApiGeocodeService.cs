using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode.Request;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode.Responses;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode;

public class OuterApiGeocodeService(IOuterApiClient outerApiClient, ILogger<OuterApiGeocodeService> logger)
    : IOuterApiGeocodeService
{
    public async Task<Geocode> Geocode(string postcode)
    {
        try
        {
            logger.LogInformation($"Getting geo code for postcode {postcode}");
            var result = await outerApiClient.Get<GetGeoPointResponse>(new GetGeoCodeRequest(postcode));

            if (result?.GeoPoint?.Latitude != null && result?.GeoPoint?.Longitude != null)
            {
                return new Geocode
                {
                    GeoCodeMethod = Domain.Entities.GeoCodeMethod.OuterApi,
                    Latitude = result.GeoPoint.Latitude,
                    Longitude = result.GeoPoint.Longitude
                };
            }

            return null;
        }
        catch (Exception e)
        {
            string message = $"Get geocode failed for postcode: {postcode}";
            logger.LogDebug(message);
            throw new Exception(message, e);
        }
    }
}