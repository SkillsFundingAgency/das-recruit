using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode
{
    public class GeocodeService : IGeocodeService
    {
        private readonly IEnumerable<IGeocodeService> _geocodeServices;

        public GeocodeService(IEnumerable<IGeocodeService> geocodeServices)
        {
            _geocodeServices = geocodeServices;
        }

        public async Task<Geocode> Geocode(string postcode)
        {
            foreach (var service in _geocodeServices)
            {
                var geocode = await service.Geocode(postcode);

                if (geocode != null)
                {
                    return geocode;
                }
            }

            return null;
        }
    }
}
