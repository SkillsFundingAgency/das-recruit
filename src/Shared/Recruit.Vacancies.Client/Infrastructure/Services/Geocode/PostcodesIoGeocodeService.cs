using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode.Responses;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode
{
    public class PostcodesIoGeocodeService : IGeocodeService
    {
        private readonly string _postcodesIoUrl;

        public PostcodesIoGeocodeService(string postcodesIoUrl)
        {
            _postcodesIoUrl = postcodesIoUrl;
        }
        public async Task<Geocode> Geocode(string postcode)
        {
            
            var client = new RestClient(_postcodesIoUrl);

            var request = new RestRequest("postcodes?q={postcode}", Method.GET);
            request.AddUrlSegment("postcode", postcode);

            var response = await client.ExecuteTaskAsync<PostcodesIoResponse>(request);

            if (response.IsSuccessful)
            {
                var result = response.Data.Result?.FirstOrDefault();

                if (result?.Latitude != null && result?.Longitude != null)
                {
                    return new Geocode
                    {
                        Latitude = result.Latitude.Value,
                        Longitude = result.Longitude.Value,
                        GeoCodeMethod = GeoCodeMethod.PostcodesIo
                    };
                }
            }
            
            return null;
        }
    }


}
