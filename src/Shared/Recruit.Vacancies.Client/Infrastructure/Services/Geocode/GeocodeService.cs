using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
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
                var geocodeService = service.GetType().Name;

                try
                {
                    _logger.LogInformation("Calling {geocodeService} to resolve postcode:{postcode}",
                        geocodeService, postcode);

                    var geocode = await service.Geocode(postcode);

                    if (geocode != null)
                    {
                        _logger.LogInformation("{geocodeService} successfully geocoded postcode:{postcode} geocode:{geocode}",
                            geocodeService, postcode, geocode);

                        return geocode;
                    }

                    _logger.LogInformation("{geocodeService} failed to geocode postcode:{postcode}",
                        geocodeService, postcode);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "{geocodeService} threw exception for postcode:{postcode}",
                        geocodeService, postcode);
                }
            }

            return new Geocode { GeoCodeMethod = GeoCodeMethod.FailedToGeoCode };
        }
    }
}
