using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode.Responses;
using RestSharp;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode
{
    public class PostcodeAnywhereGeocodeService : IGeocodeService
    {
        private readonly string _postcodeAnywhereUrl;
        private readonly string _postcodeAnywhereKey;

        public PostcodeAnywhereGeocodeService(string postcodeAnywhereUrl, string postcodeAnywhereKey)
        {
            _postcodeAnywhereUrl = postcodeAnywhereUrl;
            _postcodeAnywhereKey = postcodeAnywhereKey;
        }

        public async Task<Geocode> Geocode(string postcode)
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
                    return new Geocode
                    {
                        Latitude = result.Latitude.Value,
                        Longitude = result.Longitude.Value,
                        GeoCodeMethod = GeoCodeMethod.Loqate
                    };
                }
            }

            return null;
        }
    }
}
