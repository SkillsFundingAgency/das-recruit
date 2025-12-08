using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

public interface IReferenceDataClient
{
    Task<List<string>> GetCandidateSkillsAsync();
    Task<List<string>> GetCandidateQualificationsAsync();
}

internal class GetCandidateSkillsRequest : IGetApiRequest
{
    public string GetUrl => "referencedata/candidate-skills";
}

internal class GetCandidateQualificationsRequest : IGetApiRequest
{
    public string GetUrl => "referencedata/candidate-qualifications";
}

public class ReferenceDataClient(IOuterApiClient apimClient, ICache cache) : IReferenceDataClient
{
    public async Task<List<string>> GetCandidateSkillsAsync()
    {
        return await cache.CacheAsideAsync(CacheKeys.Skills,
            DateTime.UtcNow.AddDays(1),
            async () => await apimClient.Get<List<string>>(new GetCandidateSkillsRequest()));
    }

    public async Task<List<string>> GetCandidateQualificationsAsync()
    {
        return await cache.CacheAsideAsync(CacheKeys.Qualifications,
            DateTime.UtcNow.AddDays(1),
            async () => await apimClient.Get<List<string>>(new GetCandidateQualificationsRequest()));
    }
}