using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode
{
    public class PostcodesIoGeocodeService : IGeocodeService
    {
        private readonly ILogger<GeocodeService> _logger;
        private readonly string _postcodesIoUrl;

        public PostcodesIoGeocodeService(ILogger<GeocodeService> logger, string postcodesIoUrl)
        {
            _logger = logger;
            _postcodesIoUrl = postcodesIoUrl;
        }
        public async Task<Geocode> Geocode(string postcode)
        {
            try
            {
                //http://api.postcodes.io/postcodes?q=SW1A2AA

                var client = new RestClient(_postcodesIoUrl);

                var request = new RestRequest("postcodes?q={q}", Method.GET);
                request.AddUrlSegment("q", postcode);

                var response = await client.ExecuteTaskAsync<PostcodesResponse>(request);

                if (response.IsSuccessful)
                {
                    var result = response.Data.Result?.FirstOrDefault();

                    if (result?.Latitude != null && result?.Longitude != null)
                    {
                        var geocode = new Geocode
                        {
                            Latitude = result.Latitude.Value,
                            Longitude = result.Longitude.Value,
                        };

                        _logger.LogInformation("Resolved geocode {geocode} for postcode {postcode} using postcodes.io", geocode, postcode);

                        return geocode;
                    }
                }

                _logger.LogInformation("Cannot resolve geocode for postcode {postcode} using postcodes.io", postcode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Calling postcodes.io service caused an error. {postcode}", postcode);
                return null;
            }
        }

        public class PostcodesResponse
        {
            public List<PostcodesResponseResult> Result { get; set; }   
        }

        public class PostcodesResponseResult
        {
            public double? Longitude { get; set; }
            public double? Latitude { get; set; }
        }
    }


}
