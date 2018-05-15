using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode.Responses;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode
{
    public class PostcodesIoOutcodeGeocodeService : IGeocodeService
    {
        private const int PostcodeMinLength = 5;
        private const int IncodeLength = 3;

        private readonly ILogger<GeocodeService> _logger;
        private readonly string _postcodesIoUrl;

        public PostcodesIoOutcodeGeocodeService(ILogger<GeocodeService> logger, string postcodesIoUrl)
        {
            _logger = logger;
            _postcodesIoUrl = postcodesIoUrl;
        }
        public async Task<Geocode> Geocode(string postcode)
        {
            string outcode = null;

            try
            {
                outcode = GetOutcode(postcode);

                if (!string.IsNullOrEmpty(outcode))
                {

                    var client = new RestClient(_postcodesIoUrl);

                    var request = new RestRequest("outcodes/{outcode}", Method.GET);
                    request.AddUrlSegment("outcode", outcode);

                    var response = await client.ExecuteTaskAsync<PostcodesIoResponse>(request);

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

                            _logger.LogInformation(
                                "Resolved geocode {geocode} for outcode:{outcode} using postcodes.io", geocode,
                                outcode);

                            return geocode;
                        }
                    }
                }

                _logger.LogInformation("Cannot resolve geocode for outcode:{outcode} using postcodes.io", outcode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Calling postcodes.io service caused an error for outcode:{outcode}", outcode);
                return null;
            }
        }

        private string GetOutcode(string postcode)
        {
            //Rule: Anything before the last 3 incode characters (minus the space if there is one) is the outcode
            postcode = postcode.Replace(" ", "");

            if (postcode.Length < PostcodeMinLength)
            {
                return null;
            }

            return postcode.Substring(0, postcode.Length - IncodeLength);
        }
    }


}
