using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.EmployerProfiles;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.EmployerProfiles;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Microsoft.Extensions.Logging;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerProfile;

public class EmployerProfileService(ILogger<EmployerProfileService> logger,
    IOuterApiClient outerApiClient,
    EncodingService encodingService) : IEmployerProfileService
{
    public async Task CreateAsync(Domain.Entities.EmployerProfile profile)
    {
        logger.LogTrace("Getting employer profile Details from Outer Api");

        var retryPolicy = PollyRetryPolicy.GetPolicy();

        var result = await retryPolicy.Execute(_ =>
                outerApiClient.Get<GetProviderResponseItem>(new GetProviderRequest(ukprn)),
            new Dictionary<string, object>
            {
                {
                    "apiCall", "EmployerProfile"
                }
            });

        return result;
    }

    public async Task<IList<Domain.Entities.EmployerProfile>> GetEmployerProfilesForEmployerAsync(
        string hashedAccountId)
    {
        logger.LogTrace("Getting employer profile Details from Outer Api");

        var retryPolicy = PollyRetryPolicy.GetPolicy();

        var accountId = encodingService.Decode(hashedAccountId, EncodingType.AccountId);
        var result = await retryPolicy.Execute(_ =>
                outerApiClient.Get<List<Domain.EmployerProfiles.EmployerProfile>>(new GetEmployerProfilesByAccountIdRequest(accountId)),
            new Dictionary<string, object>
            {
                {
                    "apiCall", "EmployerProfile"
                }
            });

        return result.Select(MapEmployerProfile).ToList();
    }

    public async Task<Domain.Entities.EmployerProfile> GetAsync(string hashedAccountId,
        string accountLegalEntityPublicHashedId)
    {
        logger.LogTrace("Getting employer profile Details from Outer Api");

        var retryPolicy = PollyRetryPolicy.GetPolicy();

        var accountId = encodingService.Decode(hashedAccountId, EncodingType.AccountId);
        var result = await retryPolicy.Execute(_ =>
                outerApiClient.Get<Domain.EmployerProfiles.EmployerProfile>(new GetEmployerProfilesByAccountIdRequest(accountId)),
            new Dictionary<string, object>
            {
                {
                    "apiCall", "EmployerProfile"
                }
            });

        return result.Select(MapEmployerProfile).ToList();
    }

    public async Task UpdateAsync(Domain.Entities.EmployerProfile profile) => throw new NotImplementedException();

    private static Domain.Entities.EmployerProfile MapEmployerProfile(Domain.EmployerProfiles.EmployerProfile source) =>
        new()
        {
            EmployerAccountId = source.AccountId.ToString(),
            AccountLegalEntityPublicHashedId = source.AccountLegalEntityId.ToString(),
            AboutOrganisation = source.AboutOrganisation,
            TradingName = source.TradingName,
            OtherLocations = source.Addresses != null ? MapOtherLocations(source.Addresses) : new List<Address>()
        };

    private static IList<Address> MapOtherLocations(List<EmployerProfileAddress> sourceAddresses) =>
        sourceAddresses.ConvertAll(source => new Address
        {
            AddressLine1 = source.AddressLine1,
            AddressLine2 = source.AddressLine2,
            AddressLine3 = source.AddressLine3,
            AddressLine4 = source.AddressLine4,
            Postcode = source.Postcode,
            Latitude = source.Latitude,
            Longitude = source.Longitude
        });
}
