using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Services;

public interface IVacancyLocationService
{
    public Task<List<Address>> GetVacancyLocations(Vacancy vacancy);
    public Task<UpdateVacancyLocationsResult> UpdateDraftVacancyLocations(Vacancy vacancy, VacancyUser user, AvailableWhere availableWhere, List<Address> locations = null, string locationInformation = null);
}

public record UpdateVacancyLocationsResult(EntityValidationResult ValidationResult);

public class VacancyLocationService(IRecruitVacancyClient recruitVacancyClient, IEmployerVacancyClient employerVacancyClient): IVacancyLocationService
{
    public async Task<List<Address>> GetVacancyLocations(Vacancy vacancy)
    {
        ArgumentNullException.ThrowIfNull(vacancy);
        var employerProfile = await recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);
        var employerData = await employerVacancyClient.GetEditVacancyInfoAsync(employerProfile.EmployerAccountId);
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
    
    public async Task<UpdateVacancyLocationsResult> UpdateDraftVacancyLocations(Vacancy vacancy, VacancyUser user, AvailableWhere availableWhere, List<Address> locations = null, string locationInformation = null)
    {
        ArgumentNullException.ThrowIfNull(vacancy);
        ArgumentNullException.ThrowIfNull(user);
        
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
}