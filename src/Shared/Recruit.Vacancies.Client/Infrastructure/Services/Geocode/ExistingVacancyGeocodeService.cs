using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode
{
    public class ExistingVacancyGeocodeService : IGeocodeService
    {
        private readonly ILogger<GeocodeService> _logger;
        private readonly IVacancyQuery _vacancyQuery;

        public ExistingVacancyGeocodeService(ILogger<GeocodeService> logger, IVacancyQuery vacancyQuery)
        {
            _logger = logger;
            _vacancyQuery = vacancyQuery;
        }

        public async Task<Geocode> Geocode(string postcode)
        {
            var vacancy = await _vacancyQuery.GetSingleVacancyForPostcodeAsync(postcode);

            if (vacancy?.EmployerLocation?.Latitude == null || vacancy?.EmployerLocation?.Longitude == null)
            {
                return null;
            }

            var geocode = new Geocode
            {
                Latitude = vacancy.EmployerLocation.Latitude.Value,
                Longitude = vacancy.EmployerLocation.Longitude.Value,
                GeoCodeMethod = GeoCodeMethod.ExistingVacancy
            };

            _logger.LogInformation("Resolved geocode:{geocode} for postcode:{postcode} using existing vacancy:{vacancyId}", geocode, postcode, vacancy.Id);

            return geocode;
        }
    }
}
