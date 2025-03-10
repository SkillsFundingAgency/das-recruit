using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;

namespace Esfa.Recruit.Provider.Web.Services;

public interface IVacancyLocationService
{
    public Task<List<Address>> GetVacancyLocations(Vacancy vacancy, long ukprn);
    public Task<UpdateVacancyLocationsResult> UpdateDraftVacancyLocations(Vacancy vacancy, VacancyUser user, AvailableWhere availableWhere, List<Address> locations = null, string locationInformation = null);
    public Task SaveEmployerAddress(VacancyUser user, Vacancy vacancy, long ukprn, Address address);
}

public record UpdateVacancyLocationsResult(EntityValidationResult ValidationResult);

public class VacancyLocationService(
    IRecruitVacancyClient recruitVacancyClient,
    IProviderVacancyClient providerVacancyClient,
    IReviewFieldIndicatorService reviewFieldIndicatorService,
    ILocationsService locationsService): IVacancyLocationService
{
    public async Task<List<Address>> GetVacancyLocations(Vacancy vacancy, long ukprn)
    {
        ArgumentNullException.ThrowIfNull(vacancy);
        var employerProfile = await recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);
        var providerData = await providerVacancyClient.GetProviderEditVacancyInfoAsync(ukprn);
        var employerInfo = providerData.Employers.Single(e => e.EmployerAccountId == vacancy.EmployerAccountId);
        var legalEntity = employerInfo.LegalEntities.FirstOrDefault(l => l.AccountLegalEntityPublicHashedId == employerProfile.AccountLegalEntityPublicHashedId);
        
        var locations = new List<Address>();
        var legalAddress = legalEntity?.Address.ConvertToDomainAddress();
        if (legalAddress is not null && !legalAddress.IsEmpty())
        {
            locations.Add(legalAddress);
        }
        locations.AddRange(employerProfile.OtherLocations.Where(x => !x.IsEmpty()));
        return locations;
    }
    
    public async Task<UpdateVacancyLocationsResult> UpdateDraftVacancyLocations(Vacancy vacancy, VacancyUser user, AvailableWhere availableWhere, List<Address> locations = null, string locationInformation = null)
    {
        ArgumentNullException.ThrowIfNull(vacancy);
        ArgumentNullException.ThrowIfNull(user);
        
        reviewFieldIndicatorService.SetVacancyWithProviderReviewFieldIndicators(vacancy.EmployerLocations, FieldIdResolver.ToFieldId(v => v.EmployerLocations), vacancy, locations);
        reviewFieldIndicatorService.SetVacancyWithProviderReviewFieldIndicators(vacancy.EmployerLocationInformation, FieldIdResolver.ToFieldId(v => v.EmployerLocationInformation), vacancy, locationInformation);
        
        await UpdateAddressCountriesAsync(locations, user, vacancy);
        
        vacancy.EmployerLocation = null; // null it for records created before this feature that are edited 
        vacancy.EmployerLocationOption = availableWhere;
        vacancy.EmployerLocations = locations;
        vacancy.EmployerLocationInformation = locationInformation;
        
        var validationResult = recruitVacancyClient.Validate(vacancy, VacancyRuleSet.EmployerAddress);
        if (validationResult.HasErrors)
        {
            return new UpdateVacancyLocationsResult(validationResult);
        }

        await recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
        return new UpdateVacancyLocationsResult(null);
    }
    
    private async Task UpdateAddressCountriesAsync(List<Address> locations, VacancyUser user, Vacancy vacancy)
    {
        if (locations is null || locations.Count == 0)
        {
            return;
        }

        var nonEnglishAddresses = locations.Where(x => x.Country is not ("England" or null)).ToArray();
        if (nonEnglishAddresses.Length is not 0)
        {
            // fail fast since we know this will fail validation anyway
            return;
        }

        var addressesToQuery = locations
            .Where(x => x.Country is null)
            .Select(x => x.Postcode)
            .Distinct()
            .ToList();
        var results = await locationsService.GetBulkPostcodeDataAsync(addressesToQuery);

        bool isDirty = false;
        locations.ForEach(x =>
        {
            if (x.Country is null && results.TryGetValue(x.Postcode, out var postcodeData) && postcodeData is not null)
            {
                x.Country = postcodeData.Country;
                isDirty = true;
            }
        });

        if (isDirty)
        {
            await PatchExistingEmployerAddresses(user, vacancy, locations);
        }
    }

    private async Task PatchExistingEmployerAddresses(VacancyUser user, Vacancy vacancy, List<Address> addresses)
    {
        var employerProfile = await recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);
        var existingLocations = employerProfile.OtherLocations;

        foreach (var address in addresses)
        {
            string newAddressString = address.ToSingleLineFullAddress();
            var existingLocation = existingLocations.FirstOrDefault(x => x.ToSingleLineFullAddress().Equals(newAddressString, StringComparison.InvariantCultureIgnoreCase));
            if (existingLocation is null)
            {
                // will hit this when:
                //   - it's the legal entity address
                //   - unknown postcode 
                continue;
            }

            existingLocation.Country = address.Country;
        }

        await recruitVacancyClient.UpdateEmployerProfileAsync(employerProfile, user);
    }

    public async Task SaveEmployerAddress(VacancyUser user, Vacancy vacancy, long ukprn, Address address)
    {
        var existingLocations = await GetVacancyLocations(vacancy, ukprn);
        string newAddressString = address.ToSingleLineFullAddress();
        if (existingLocations.Any(x => x.ToSingleLineFullAddress().Equals(newAddressString, StringComparison.InvariantCultureIgnoreCase)))
        {
            // Don't add existing addresses
            return;
        }
        
        var employerProfile = await recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);
        employerProfile.OtherLocations.Add(address);
        await recruitVacancyClient.UpdateEmployerProfileAsync(employerProfile, user);
    }
}