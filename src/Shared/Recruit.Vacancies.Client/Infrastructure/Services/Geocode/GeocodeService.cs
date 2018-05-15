using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode
{
    public class GeocodeService : IGeocodeService
    {
        private readonly ILogger<GeocodeService> _logger;
        private readonly IEnumerable<IGeocodeService> _geocodeServices;

        public GeocodeService(ILogger<GeocodeService> logger, IEnumerable<IGeocodeService> geocodeServices)
        {
            _logger = logger;
            _geocodeServices = geocodeServices;
        }

        public async Task<Geocode> Geocode(string postcode)
        {
            foreach (var service in _geocodeServices)
            {
                _logger.LogInformation("Using {geocodeService} to resolve postcode:{postcode}", service.GetType().Name, postcode);
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
