using System;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class GeocodeVacancyCommandHandler: IRequestHandler<GeocodeVacancyCommand,Unit>
    {
        private readonly IVacancyRepository _repository;
        private readonly IOuterApiGeocodeService _geocodeService;
        private readonly ILogger<GeocodeVacancyCommandHandler> _logger;

        public GeocodeVacancyCommandHandler(
            IVacancyRepository repository,
            IOuterApiGeocodeService geocodeServiceFactory,
            ILogger<GeocodeVacancyCommandHandler> logger)
        {
            _repository = repository;
            _geocodeService = geocodeServiceFactory;
            _logger = logger;
        }

        public async Task<Unit> Handle(GeocodeVacancyCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Geocoding vacancy {vacancyId}.", message.VacancyId);

            var vacancy = await _repository.GetVacancyAsync(message.VacancyId);

            if (string.IsNullOrEmpty(vacancy?.EmployerLocation?.Postcode))
            {
                _logger.LogWarning("Geocode vacancyId:{vacancyId} cannot geocode as vacancy has no postcode", vacancy.Id);
                return Unit.Value;
            }

            var geocode = await GetGeocode(vacancy, vacancy.GeocodeUsingOutcode);
          
            await SetVacancyGeocode(vacancy.Id, geocode);
            
            return Unit.Value;
        }

        private Task<Geocode> GetGeocode(Vacancy vacancy, bool usingOutCode)
        {
            if (usingOutCode)
            {
                _logger.LogInformation("Attempting to geocode outcode:'{outcode}' for anonymous vacancyId:'{vacancyId}'", vacancy.EmployerLocation.PostcodeAsOutcode(), vacancy.Id);
            }
            else
            {
                _logger.LogInformation("Attempting to geocode postcode:'{postcode}' for vacancyId:'{vacancyId}'", vacancy.EmployerLocation.Postcode, vacancy.Id);
            }

            return _geocodeService.Geocode(vacancy.EmployerLocation.Postcode);
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
            vacancy.GeoCodeMethod = geocode.GeoCodeMethod;

            await _repository.UpdateAsync(vacancy);

            _logger.LogInformation("Successfully geocoded vacancy:{vacancyId} with geocode Latitude:{latitude} Logtitude:{longitude}", vacancy.Id, vacancy.EmployerLocation.Latitude, vacancy.EmployerLocation.Longitude);
        }
    }
}
