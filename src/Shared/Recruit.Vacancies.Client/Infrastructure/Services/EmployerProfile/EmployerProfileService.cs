using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.EmployerProfiles;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.EmployerProfiles;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.EmployerProfiles;
using Microsoft.Extensions.Logging;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerProfile;

public class EmployerProfileService(ILogger<EmployerProfileService> logger,
    IOuterApiClient outerApiClient,
    IEncodingService encodingService) : IEmployerProfileService
{
    private readonly Dictionary<string, object> _apiLoggingContext = new()
    {
        {
            "apiCall", "EmployerProfile"
        }
    };

    public async Task CreateAsync(Domain.Entities.EmployerProfile profile)
    {
        logger.LogTrace("Posting Employer Provider details to Outer Api");

        var retryPolicy = PollyRetryPolicy.GetPolicy();

        var legalEntityId = encodingService.Decode(profile.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId);
        var accountId = encodingService.Decode(profile.EmployerAccountId, EncodingType.AccountId);
        
        await retryPolicy.Execute(_ => outerApiClient.Post(
                new PostEmployerProfileRequest(legalEntityId, new PostEmployerProfileRequest.PostEmployerProfileRequestData
                {
                    AccountId = accountId,
                    AboutOrganisation = profile.AboutOrganisation,
                    TradingName = profile.TradingName
                })),
            _apiLoggingContext);
    }

    public async Task<IList<Domain.Entities.EmployerProfile>> GetEmployerProfilesForEmployerAsync(
        string hashedAccountId)
    {
        logger.LogTrace("Getting employer profile Details from Outer Api");

        var retryPolicy = PollyRetryPolicy.GetPolicy();

        var accountId = encodingService.Decode(hashedAccountId, EncodingType.AccountId);
        var result = await retryPolicy.Execute(_ =>
                outerApiClient.Get<GetEmployerProfilesByAccountIdResponse>(new GetEmployerProfilesByAccountIdRequest(accountId)),
            _apiLoggingContext);

        return result.EmployerProfiles.Select(MapEmployerProfile).ToList();
    }

    public async Task<Domain.Entities.EmployerProfile> GetAsync(string employerAccountId,
        string accountLegalEntityPublicHashedId)
    {
        logger.LogTrace("Getting employer profile details from Outer API");

        var retryPolicy = PollyRetryPolicy.GetPolicy();

        var legalEntityId = encodingService.Decode(accountLegalEntityPublicHashedId,
            EncodingType.PublicAccountLegalEntityId);

        var result = await retryPolicy.Execute(_ => outerApiClient.Get<GetEmployerProfilesByLegalEntityIdResponse>(
                new GetEmployerProfileByLegalEntityIdRequest(legalEntityId)),
            _apiLoggingContext);

        if (result?.EmployerProfile != null)
            return MapEmployerProfile(result.EmployerProfile);

        logger.LogInformation("No employer profile found for LegalEntityId {LegalEntityId}, creating new profile", legalEntityId);

        var newProfile = new Domain.Entities.EmployerProfile
        {
            AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
            EmployerAccountId = employerAccountId
        };

        await CreateAsync(newProfile);
        return newProfile;
    }

    public async Task UpdateAsync(Domain.Entities.EmployerProfile profile)
    {
        logger.LogTrace("Patching Employer Provider details to Outer Api");

        var retryPolicy = PollyRetryPolicy.GetPolicy();

        await retryPolicy.Execute(_ => outerApiClient.Post(
                new PatchEmployerProfileRequest(ToEmployerProfile(profile))),
            _apiLoggingContext);
    }

    private Domain.Entities.EmployerProfile MapEmployerProfile(Domain.EmployerProfiles.EmployerProfile source) =>
        new()
        {
            EmployerAccountId = encodingService.Encode(source.AccountId, EncodingType.AccountId),
            AccountLegalEntityPublicHashedId = encodingService.Encode(source.AccountLegalEntityId, EncodingType.PublicAccountLegalEntityId),
            AboutOrganisation = source.AboutOrganisation,
            TradingName = source.TradingName,
            OtherLocations = source.Addresses != null ? MapOtherLocations(source.Addresses) : []
        };

    private static List<Address> MapOtherLocations(List<EmployerProfileAddress> sourceAddresses) =>
        sourceAddresses.ConvertAll(source => new Address
        {
            Id = source.Id,
            AddressLine1 = source.AddressLine1,
            AddressLine2 = source.AddressLine2,
            AddressLine3 = source.AddressLine3,
            AddressLine4 = source.AddressLine4,
            Postcode = source.Postcode,
            Latitude = source.Latitude,
            Longitude = source.Longitude
        });

    private Domain.EmployerProfiles.EmployerProfile ToEmployerProfile(Domain.Entities.EmployerProfile source)
    {
        var accountId = encodingService.Decode(source.EmployerAccountId, EncodingType.AccountId);
        var accountLegalEntityId = encodingService.Decode(source.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId);
        var addresses = source.OtherLocations != null ? MapOtherLocations(source.OtherLocations) : [];

        return new Domain.EmployerProfiles.EmployerProfile(accountLegalEntityId, accountId, source.AboutOrganisation,
            source.TradingName, addresses);
    }

    private static List<EmployerProfileAddress> MapOtherLocations(IList<Address> sourceAddresses)
    {
        if (sourceAddresses == null || sourceAddresses.Count == 0)
            return [];

        return sourceAddresses.Select(source => new EmployerProfileAddress(
            source.Id,
            source.AddressLine1,
            source.AddressLine2,
            source.AddressLine3,
            source.AddressLine4,
            source.Postcode,
            source.Latitude,
            source.Longitude
        )).ToList();
    }
}
