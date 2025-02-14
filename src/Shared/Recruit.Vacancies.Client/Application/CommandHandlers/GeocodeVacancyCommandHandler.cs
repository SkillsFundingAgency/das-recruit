using System;
using System.Collections.Generic;
using System.Linq;
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
    public class GeocodeVacancyCommandHandler(
        IVacancyRepository repository,
        IOuterApiGeocodeService geocodeServiceFactory,
        ILogger<GeocodeVacancyCommandHandler> logger)
        : IRequestHandler<GeocodeVacancyCommand, Unit>
    {
        public async Task<Unit> Handle(GeocodeVacancyCommand message, CancellationToken cancellationToken)
        {
            logger.LogInformation("Geocoding: vacancy {vacancyId}", message.VacancyId);
            var vacancy = await repository.GetVacancyAsync(message.VacancyId);

            if (vacancy is null)
            {
                logger.LogWarning("Geocoding: vacancy {vacancyId} does not exist", message.VacancyId);
                return Unit.Value;
            }

            if (vacancy.EmployerLocationOption is AvailableWhere.AcrossEngland)
            {
                logger.LogInformation("Geocoding: vacancy {vacancyId} is recruiting nationally, no need for geocoding", message.VacancyId);
                return Unit.Value;
            }
            
            // Handle existing data
            if (vacancy.EmployerLocation is not null)
            {
                await UpdateDeprecatedEmployerLocation(vacancy);
                return Unit.Value;
            }
            
            // Handle post multiple locations data
            if (vacancy.EmployerLocations is { Count: > 0 })
            {
                await UpdateEmployerLocations(vacancy);
            }
            else
            {
                logger.LogWarning("Geocode: vacancy {vacancyId} does not have any locations to geocode", vacancy.Id);
            }

            return Unit.Value;
        }

        private async Task UpdateEmployerLocations(Vacancy vacancy)
        {
            // log empty postcodes
            var noPostcodes = vacancy.EmployerLocations.Where(x => string.IsNullOrWhiteSpace(x.Postcode)).ToList();
            if (noPostcodes is { Count: > 0 })
            {
                logger.LogWarning("Geocode: vacancy {vacancyId} - {count} locations do not have postcodes to lookup", vacancy.Id, noPostcodes.Count);
            }
            
            var locationsNeedingGeocoding = vacancy.EmployerLocations
                .Where(x => !x.HasGeocode)
                .Except(noPostcodes)
                .ToList();
            
            if (locationsNeedingGeocoding is { Count: 0 })
            {
                logger.LogInformation("Geocode: vacancy {vacancyId} - no locations need geocoding", vacancy.Id);
                return;
            }

            // setup a dictionary with the postcode mapped to the lookup task
            var lookups = locationsNeedingGeocoding
                .Select(x => vacancy.GeocodeUsingOutcode ? x.PostcodeAsOutcode() : x.Postcode)
                .Distinct()
                .ToDictionary(x => x, x => TryGeocode(vacancy.Id, x));
            
            logger.LogInformation("Geocode: vacancy {vacancyId} - attempting to lookup geocode data for the following postcodes {postcodes}", vacancy.Id, string.Join(", ", lookups.Keys));
            
            // wait for all the lookups to complete
            await Task.WhenAll(lookups.Select(x => x.Value));
            
            // did any tasks fail to return a geocode?
            var failedLookups = lookups.Where(x => x.Value.Result is null).Select(x => x.Key).ToList();
            if (failedLookups is { Count: > 0 })
            {
                logger.LogWarning("Geocode: vacancy {vacancyId} - failed to lookup geocode data for the following postcodes {postcodes}", vacancy.Id, string.Join(", ", failedLookups));
            }
            
            // process the successful lookups
            var postcodeLookups = lookups.Where(x => x.Value.Result is not null).ToDictionary(x => x.Key, x => x.Value.Result);
            locationsNeedingGeocoding.ForEach(location =>
            {
                string postcode = vacancy.GeocodeUsingOutcode ? location.PostcodeAsOutcode() : location.Postcode;
                if (!postcodeLookups.TryGetValue(postcode, out var geocode))
                {
                    return;
                }
                
                location.Latitude = geocode.Latitude;
                location.Longitude = geocode.Longitude;
            });
            
            vacancy.GeoCodeMethod = GeoCodeMethod.OuterApi;
            await repository.UpdateAsync(vacancy);
        }

        private async Task<Geocode> TryGeocode(Guid vacancyId, string postcode)
        {
            try
            {
                return await geocodeServiceFactory.Geocode(postcode);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Geocode: vacancy {vacancyId} - error thrown whilst geocoding postcode {postcode}", vacancyId, postcode);
            }

            return null;
        }

        private async Task UpdateDeprecatedEmployerLocation(Vacancy vacancy)
        {
            if (vacancy is null || vacancy.EmployerLocation?.HasGeocode is true)
            {
                return;
            }

            if (string.IsNullOrEmpty(vacancy.EmployerLocation?.Postcode))
            {
                logger.LogWarning("Geocode: vacancy {vacancyId} - cannot geocode as vacancy has no postcode", vacancy.Id);
                return;
            }

            var geocode = await GetGeocode(vacancy, vacancy.GeocodeUsingOutcode);
            if (geocode != null)
            {
                await SetVacancyGeocode(vacancy, geocode);
            }
            else
            {
                logger.LogWarning("Unable to get geocode information for postcode: {Postcode}", vacancy.EmployerLocation.Postcode);
            }
        }

        private Task<Geocode> GetGeocode(Vacancy vacancy, bool usingOutCode)
        {
            if (usingOutCode)
            {
                logger.LogInformation("Attempting to geocode outcode:'{outcode}' for anonymous vacancyId:'{vacancyId}'", vacancy.EmployerLocation.PostcodeAsOutcode(), vacancy.Id);
                return geocodeServiceFactory.Geocode(vacancy.EmployerLocation.PostcodeAsOutcode());
            }
            
            logger.LogInformation("Attempting to geocode postcode:'{postcode}' for vacancyId:'{vacancyId}'", vacancy.EmployerLocation.Postcode, vacancy.Id);
            return geocodeServiceFactory.Geocode(vacancy.EmployerLocation.Postcode);
            
        }

        private async Task SetVacancyGeocode(Vacancy vacancy, Geocode geocode)
        {
            if (vacancy.EmployerLocation == null)
            {
                logger.LogInformation("Vacancy:{vacancyId} does not have employer location information. Cannot update vacancy", vacancy.Id);
                return;
            }

            if (vacancy.EmployerLocation?.Latitude != null && vacancy.EmployerLocation?.Longitude != null)
            {
                if (Math.Abs(vacancy.EmployerLocation.Latitude.Value - geocode.Latitude) < 0.0001 &&
                    Math.Abs(vacancy.EmployerLocation.Longitude.Value - geocode.Longitude) < 0.0001)
                {
                    logger.LogInformation("Vacancy geocode:{geocode} has not changed for vacancy:{vacancyId}. Not updating vacancy", geocode, vacancy.Id);
                    return;
                }
            }

            vacancy.EmployerLocation.Latitude = geocode.Latitude;
            vacancy.EmployerLocation.Longitude = geocode.Longitude;
            vacancy.GeoCodeMethod = geocode.GeoCodeMethod;

            await repository.UpdateAsync(vacancy);

            logger.LogInformation("Successfully geocoded vacancy:{vacancyId} with geocode Latitude:{latitude} Logtitude:{longitude}", vacancy.Id, vacancy.EmployerLocation.Latitude, vacancy.EmployerLocation.Longitude);
        }
    }
}
