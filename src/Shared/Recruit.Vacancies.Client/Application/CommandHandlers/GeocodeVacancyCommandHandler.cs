using System;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class GeoVacancyCommandHandler: IRequestHandler<GeocodeVacancyCommand>
    {
        private readonly IVacancyRepository _repository;
        private readonly IGeocodeService _geocodeService;
        private readonly ILogger<GeoVacancyCommandHandler> _logger;

        public GeoVacancyCommandHandler(
            IVacancyRepository repository, 
            IGeocodeServiceFactory geocodeServiceFactory,
            ILogger<GeoVacancyCommandHandler> logger)
        {
            _repository = repository;
            _geocodeService = geocodeServiceFactory.GetGeocodeService();
            _logger = logger;
        }

        public async Task Handle(GeocodeVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Geocoding vacancy {vacancyId}.", message.VacancyId);

            var vacancy = await _repository.GetVacancyAsync(message.VacancyId);

            if (string.IsNullOrEmpty(vacancy?.EmployerLocation?.Postcode))
            {
                _logger.LogWarning("Geocode vacancyId:{vacancyId} cannot geocode as vacancy has no postcode", vacancy.Id);
                return;
            }

            _logger.LogInformation("Attempting to geocode postcode:{postcode} for vacancyId:{vacancyId}", vacancy.EmployerLocation.Postcode, vacancy.Id);
            var geocode = await _geocodeService.Geocode(vacancy.EmployerLocation.Postcode);

            if (geocode == null)
            {
                _logger.LogError("Geocode vacancyId:{vacancyId} failed to geocode postcode:{postcode}", vacancy.Id, vacancy.EmployerLocation.Postcode);
                return;
            }

            await SetVacancyGeocode(vacancy.Id, geocode);
        }

        private async Task SetVacancyGeocode(Guid vacancyId, Geocode geocode)
        {
            var vacancy = await _repository.GetVacancyAsync(vacancyId);

            if (vacancy.EmployerLocation == null)
            {
                _logger.LogInformation("Vacancy:{vacancyId} does not have employer location information. Cannot update vacancy", vacancy.Id);
                return;
            }

            if (vacancy.EmployerLocation?.Latitude != null && vacancy.EmployerLocation?.Longitude != null)
            {
                if (Math.Abs(vacancy.EmployerLocation.Latitude.Value - geocode.Latitude) < 0.0001 &&
                    Math.Abs(vacancy.EmployerLocation.Longitude.Value - geocode.Longitude) < 0.0001)
                {
                    _logger.LogInformation("Vacancy geocode:{geocode} has not changed for vacancy:{vacancyId}. Not updating vacancy", geocode, vacancy.Id);
                    return;
                }
            }

            vacancy.EmployerLocation.Latitude = geocode.Latitude;
            vacancy.EmployerLocation.Longitude = geocode.Longitude;
            await _repository.UpdateAsync(vacancy);

            _logger.LogInformation("Successfully geocoded vacancy:{vacancyId} with geocode Latitude:{latitude} Logtitude:{longitude}", vacancy.Id, vacancy.EmployerLocation.Latitude, vacancy.EmployerLocation.Longitude);
        }
    }
}
