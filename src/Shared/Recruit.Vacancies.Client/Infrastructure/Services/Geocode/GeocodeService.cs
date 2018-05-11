using System;
using System.Collections.Generic;
using System.Text;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode
{
    public class GeocodeService : IGeocodeService
    {
        private readonly IEnumerable<IGeocodeService> _geocodeServices;

        public GeocodeService(IEnumerable<IGeocodeService> geocodeServices)
        {
            _geocodeServices = geocodeServices;
        }

        public Geocode Geocode(string postcode)
        {
            foreach (var service in _geocodeServices)
            {
                var geocode = service.Geocode(postcode);

                if (geocode != null)
                {
                    return geocode;
                }
            }

            return null;
        }
    }
}
