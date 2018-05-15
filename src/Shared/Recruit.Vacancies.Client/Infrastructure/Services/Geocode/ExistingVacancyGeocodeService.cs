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
            try
            {
                var vacancy = await _repository.GetSingleVacancyForPostcode(postcode);

                if (vacancy?.EmployerLocation?.Latitude == null || vacancy?.EmployerLocation?.Longitude == null)
                {
                    _logger.LogInformation("No existing vacancy found for postcode:{postcode}", postcode);
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
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Resolving geocode from an existing vacancy caused an error for postcode:{postcode}", postcode);
                return null;
            }
        }
    }
}
