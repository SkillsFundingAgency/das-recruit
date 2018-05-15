using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode
{
    public class ExistingVacancyGeocodeService : IGeocodeService
    {
        private readonly ILogger<GeocodeService> _logger;
        private readonly IVacancyRepository _repository;

        public ExistingVacancyGeocodeService(ILogger<GeocodeService> logger, IVacancyRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<Geocode> Geocode(string postcode)
        {
            var vacancy = await _repository.GetSingleVacancyForPostcode(postcode);

            if (vacancy?.EmployerLocation?.Latitude == null || vacancy?.EmployerLocation?.Longitude == null)
            {
                return null;
            }

            var geocode = new Geocode
            {
                Latitude = vacancy.EmployerLocation.Latitude.Value,
                Longitude = vacancy.EmployerLocation.Longitude.Value
            };

            _logger.LogInformation("Resolved geocode:{geocode} for postcode:{postcode} using existing vacancy:{vacancyId}", geocode, postcode, vacancy.Id);

            return geocode;
        }
    }
}
