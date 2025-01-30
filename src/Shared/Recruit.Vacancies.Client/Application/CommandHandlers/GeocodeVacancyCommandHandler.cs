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
            logger.LogInformation("Geocoding vacancy {vacancyId}.", message.VacancyId);
            var vacancy = await repository.GetVacancyAsync(message.VacancyId);
            
            // Handle existing data
            if (vacancy.EmployerLocation is not null)
            {
                await UpdateDeprecatedEmployerLocation(vacancy);
                return Unit.Value;
            }
            
            if (vacancy.EmployerLocations is { Count: > 0 })
            {
                await UpdateEmployerLocations(vacancy);
            }
            else
            {
                logger.LogWarning("Geocode vacancyId:{vacancyId} does not have any locations to geocode", vacancy.Id);
            }

            return Unit.Value;
        }

        private async Task UpdateEmployerLocations(Vacancy vacancy)
        {
            var locations = vacancy.EmployerLocations;

            // log empty postcodes
            var noPostcodes = locations.Where(x => string.IsNullOrEmpty(x.Postcode)).ToList();
            if (noPostcodes.Count is not 0)
            {
                logger.LogWarning("Geocode vacancyId:{vacancyId} - {count} locations do not have postcodes to lookup", vacancy.Id, locations.Count);
            }

            // get a distinct list of postcode to lookup
            var postcodes = locations
                .Except(noPostcodes)
                .Select(x => vacancy.GeocodeUsingOutcode ? x.PostcodeAsOutcode() : x.Postcode)
                .Distinct()
                .ToList();
            
            logger.LogInformation("Geocode vacancyId:{vacancyId} - attempting to lookup geocode data for the following postcodes: {Postcodes}", vacancy.Id, string.Join(", ", postcodes));
            
            // setup a dictionary with the postcode mapped to the lookup task
            var lookups = postcodes
                .Select(x => new KeyValuePair<string, Task<Geocode>>(x, TryGeocode(vacancy.Id, x)))
                .ToList();
            
            await Task.WhenAll(lookups.Select(x => x.Value));
            
            // did any tasks fail to return a geocode?
            if (lookups.Any(x => x.Value.Result is null))
            {
                var failedLookups = lookups.Where(x => x.Value.Result is null).Select(x => x.Key);
                logger.LogWarning("Geocode vacancyId:{vacancyId} - failed to lookup geocode data for the following postcodes: {Postcodes}", vacancy.Id, string.Join(", ", failedLookups));
            }
            
            // process the successful lookups
            var postcodeLookups = lookups.Where(x => x.Value.Result is not null).ToDictionary(x => x.Key, x => x.Value.Result);
            locations.ForEach(location =>
            {
                if (string.IsNullOrEmpty(location.Postcode))
                {
                    return;
                }

                string postcode = vacancy.GeocodeUsingOutcode ? location.PostcodeAsOutcode() : location.Postcode;
                if (!postcodeLookups.TryGetValue(postcode, out var geocode))
                {
                    return;
                }

                if (location.Latitude is not null && location.Longitude is not null)
                {
                    if (Math.Abs(location.Latitude.Value - geocode.Latitude) < 0.0001 &&
                        Math.Abs(location.Longitude.Value - geocode.Longitude) < 0.0001)
                    {
                        logger.LogInformation("Geocode vacancyId:{vacancyId} - location geocode has not changed for postcode {Postcode} - {GeoCode}", vacancy.Id, postcode, geocode);
                        return;
                    }
                }
                
                location.Latitude = geocode.Latitude;
                location.Longitude = geocode.Longitude;
            });
            
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
                logger.LogError(ex, "Geocode vacancyId:{vacancyId} - error thrown whilst geocoding postcode: {postcode}", vacancyId, postcode);
            }

            return null;
        }

        private async Task UpdateDeprecatedEmployerLocation(Vacancy vacancy)
        {
            if (vacancy is null)
            {
                return;
            }

            if (string.IsNullOrEmpty(vacancy.EmployerLocation?.Postcode))
            {
                logger.LogWarning("Geocode vacancyId:{vacancyId} cannot geocode as vacancy has no postcode", vacancy.Id);
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
