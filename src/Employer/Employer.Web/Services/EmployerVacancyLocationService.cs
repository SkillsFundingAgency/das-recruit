using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;

namespace Esfa.Recruit.Employer.Web.Services;

public interface IEmployerVacancyLocationService : IVacancyLocationService
{
    public Task<List<Address>> GetVacancyLocations(Vacancy vacancy);    
    public Task SaveEmployerAddress(Vacancy vacancy, Address address);
}

public class EmployerVacancyLocationService(
    IRecruitVacancyClient recruitVacancyClient,
    IEmployerVacancyClient employerVacancyClient,
    IReviewFieldIndicatorService reviewFieldIndicatorService,
    ILocationsService locationsService) : VacancyLocationServiceBase(locationsService), IEmployerVacancyLocationService
{

    private readonly IRecruitVacancyClient _recruitVacancyClient = recruitVacancyClient;
    private readonly IEmployerVacancyClient _employerVacancyClient = employerVacancyClient;
    private readonly IReviewFieldIndicatorService _reviewFieldIndicatorService = reviewFieldIndicatorService;
    private readonly ILocationsService _locationsService = locationsService;

    public async Task<List<Address>> GetVacancyLocations(Vacancy vacancy)
    {
        ArgumentNullException.ThrowIfNull(vacancy);
        var employerProfile = await _recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);
        var employerData = await _employerVacancyClient.GetEditVacancyInfoAsync(employerProfile.EmployerAccountId);
        var legalEntity = employerData.LegalEntities.FirstOrDefault(l => l.AccountLegalEntityPublicHashedId == employerProfile.AccountLegalEntityPublicHashedId);

        var locations = new List<Address>();
        var legalAddress = legalEntity?.Address.ConvertToDomainAddress();
        if (legalAddress is not null && !legalAddress.IsEmpty())
        {
            locations.Add(legalAddress);
        }
        locations.AddRange(employerProfile.OtherLocations.Where(x => !x.IsEmpty()));
        return locations;
    }
    
    public async Task<UpdateVacancyLocationsResult> UpdateDraftVacancyLocations(Vacancy vacancy,
        VacancyUser user,
        AvailableWhere availableWhere,
        List<Address> locations = null,
        string locationInformation = null)
    {
        ArgumentNullException.ThrowIfNull(vacancy);
        ArgumentNullException.ThrowIfNull(user);
        
        _reviewFieldIndicatorService.SetVacancyWithEmployerReviewFieldIndicators(vacancy.EmployerLocations, FieldIdResolver.ToFieldId(v => v.EmployerLocations), vacancy, locations);
        _reviewFieldIndicatorService.SetVacancyWithEmployerReviewFieldIndicators(vacancy.EmployerLocationInformation, FieldIdResolver.ToFieldId(v => v.EmployerLocationInformation), vacancy, locationInformation);

        if (locations is not null)
            locations = [.. (await Task.WhenAll(locations.Select(EnrichAddressWithGeoData)))];

        await UpdateAddressCountriesAsync(locations, vacancy);
        
        vacancy.EmployerLocation = null; // null it for records created before this feature that are edited
        vacancy.EmployerLocationOption = availableWhere;
        vacancy.EmployerLocations = locations;
        vacancy.EmployerLocationInformation = locationInformation;
        
        var validationResult = _recruitVacancyClient.Validate(vacancy, VacancyRuleSet.EmployerAddress);
        if (validationResult.HasErrors)
        {
            return new UpdateVacancyLocationsResult(validationResult);
        }

        await _recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
        return new UpdateVacancyLocationsResult(null);
    }

    private async Task UpdateAddressCountriesAsync(List<Address> locations, Vacancy vacancy)
    {
        if (locations is null || locations.Count == 0)
            return;

        if (locations.Any(x => x.Country is not (Constants.EnglandCountryCode or null)))
            return;

        var postcodesToQuery = locations
            .Where(x => x.Country is null)
            .Select(x => x.Postcode)
            .Distinct()
            .ToList();

        if (postcodesToQuery.Count == 0)
            return;

        var results = await _locationsService.GetBulkPostcodeDataAsync(postcodesToQuery);

        var isDirty = false;
        foreach (var location in locations.Where(x => x.Country is null))
        {
            if (results.TryGetValue(location.Postcode, out var postcodeData) && postcodeData is not null)
            {
                location.Country = postcodeData.Country;
                isDirty = true;
            }
        }

        if (isDirty)
            await PatchExistingEmployerAddresses(vacancy, locations);
    }

    private async Task PatchExistingEmployerAddresses(Vacancy vacancy, List<Address> addresses)
    {
        var employerProfile = await _recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);
        var existingLocations = employerProfile.OtherLocations;
        
        foreach (var address in addresses)
        {
            var newAddressString = address.ToSingleLineFullAddress();
            var existingLocation = existingLocations.FirstOrDefault(x => x.ToSingleLineFullAddress().Equals(newAddressString, StringComparison.InvariantCultureIgnoreCase));
            existingLocation?.Country = address.Country;
        }
        
        await _recruitVacancyClient.UpdateEmployerProfileAsync(employerProfile);
    }

    public async Task SaveEmployerAddress(Vacancy vacancy, Address address)
    {
        var existingLocations = await GetVacancyLocations(vacancy);
        var newAddressString = address.ToSingleLineFullAddress();
        if (existingLocations.Any(x => x.ToSingleLineFullAddress().Equals(newAddressString, StringComparison.InvariantCultureIgnoreCase)))
        {
            // Don't add existing addresses
            return;
        }

        address = await base.EnrichAddressWithGeoData(address);

        var employerProfile = await _recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);
        employerProfile.OtherLocations.Add(address);
        await _recruitVacancyClient.UpdateEmployerProfileAsync(employerProfile);
    }    
}