using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode.Responses;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode
{
    public class PostcodeAnywhereGeocodeService : IGeocodeService
    {
        private readonly ILogger<GeocodeService> _logger;
        private readonly string _postcodeAnywhereUrl;
        private readonly string _postcodeAnywhereKey;

        public PostcodeAnywhereGeocodeService(ILogger<GeocodeService> logger, string postcodeAnywhereUrl, string postcodeAnywhereKey)
        {
            _logger = logger;
            _postcodeAnywhereUrl = postcodeAnywhereUrl;
            _postcodeAnywhereKey = postcodeAnywhereKey;
        }

        public async Task<Geocode> Geocode(string postcode)
        {
            try
            {
                var client = new RestClient(_postcodeAnywhereUrl);

                var request = new RestRequest("Geocoding/UK/Geocode/v2.10/json3.ws?Key={key}&Location={postcode}", Method.GET);
                request.AddUrlSegment("key", _postcodeAnywhereKey);
                request.AddUrlSegment("postcode", postcode);

                var response = await client.ExecuteTaskAsync<PostcodeAnywhereResponse>(request);

                if (response.IsSuccessful)
                {
                    var result = response.Data.Items?.FirstOrDefault();

                    if (result?.Latitude != null && result?.Longitude != null)
                    {
                        var geocode = new Geocode
                        {
                            Latitude = result.Latitude.Value,
                            Longitude = result.Longitude.Value,
                        };

                        _logger.LogInformation("Resolved geocode:{geocode} for postcode:{postcode} using PostcodeAnywhere", geocode, postcode);
                        
                        return geocode;
                    }
                }

                _logger.LogInformation("Cannot resolve geocode for postcode:{postcode} using PostcodeAnywhere", postcode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Calling PostcodeAnywhere service caused an error for postcode:{postcode}", postcode);
                return null;
            }
        }
    }
}
