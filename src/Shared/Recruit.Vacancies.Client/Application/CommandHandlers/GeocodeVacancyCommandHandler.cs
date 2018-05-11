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

        public GeoVacancyCommandHandler(IVacancyRepository repository, 
            IGeocodeService geocodeService,
            ILogger<GeoVacancyCommandHandler> logger)
        {
            _repository = repository;
            _geocodeService = geocodeService;
            _logger = logger;
        }

        public async Task Handle(GeocodeVacancyCommand message, CancellationToken cancellationToken)
        {
            var vacancy = await _repository.GetVacancyAsync(message.VacancyId);

            if (string.IsNullOrEmpty(vacancy?.EmployerLocation.Postcode))
            {
                _logger.LogWarning("Geocode Vacancy: {vacancyId} cannot geocode as no postcode");
                return;
            }

            var geocode = _geocodeService.Geocode(vacancy.EmployerLocation.Postcode);

            if (geocode?.Latitude == null || geocode?.Longitude == null)
            {
                _logger.LogWarning("Geocode Vacancy: {vacancyId} cannot geocode postcode:{postcode}", vacancy.Id, vacancy.EmployerLocation.Postcode);
                return;
            }

            SetVacancyGeocode(vacancy.Id, geocode.Latitude.Value, geocode.Longitude.Value);
        }

        private async void SetVacancyGeocode(Guid vacancyId, double latitude, double longitude)
        {
            var vacancy = await _repository.GetVacancyAsync(vacancyId);
            vacancy.EmployerLocation.Latitude = latitude;
            vacancy.EmployerLocation.Longitude = longitude;
            await _repository.UpdateAsync(vacancy);

            _logger.LogInformation("Geocode Vacancy: {vacancyId} with geocode Latitude: {latitude} Logtitude: {longitude}", vacancy.Id, vacancy.EmployerLocation.Latitude, vacancy.EmployerLocation.Longitude);
        }
    }
}
