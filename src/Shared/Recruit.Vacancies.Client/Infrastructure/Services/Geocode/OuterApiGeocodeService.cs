using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode.Responses;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode
{
    public class OuterApiGeocodeService : IOuterApiGeocodeService
    {
        private IOuterApiClient _outerApiClient;

        public OuterApiGeocodeService(IOuterApiClient outerApiClient)
        {
            _outerApiClient = outerApiClient;
        }

        public async Task<Geocode> Geocode(string postcode)
        {
            var result = await _outerApiClient.Get<OuterApiGeoCodeResponse>(new GetGetoCodeRequest(postcode));

            if (result != null && result.Latitude != null && result.Longitude != null)
            {
                return new Geocode
                {
                    GeoCodeMethod = Domain.Entities.GeoCodeMethod.OuterApi,
                    Latitude = result.Latitude.Value,
                    Longitude = result.Longitude.Value
                };
            }

            return null;
        }
    }

    public class GetGetoCodeRequest : IGetApiRequest
    {
        private readonly string _query;

        public GetGetoCodeRequest(string query)
        {
            _query = query;
        }

        string IGetApiRequest.GetUrl
        {
            get
            {
                return $"locations/geocode/{_query}";
            }
        }
    }
}
