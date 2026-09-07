using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Vacancy.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Microsoft.AspNetCore.WebUtilities;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

public interface IOuterApiVacancyClient
{
    Task CreateAsync(Vacancy vacancy);
    Task UpdateAsync(Vacancy vacancy);
    Task<long> GetNextVacancyIdAsync();
    Task<Vacancy> GetVacancyAsync(long vacancyReference);
    Task<Vacancy> GetVacancyAsync(Guid vacancyId);
    
    // deprecated methods here for migration to sql
    Task<IList<Vacancy>> FindClosedVacanciesAsync(IList<long> vacancyReferences);
    Task<IEnumerable<ProviderVacancySummary>> GetVacanciesAssociatedToProviderAsync(long ukprn);
    Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesForLegalEntityAsync(long ukprn, string accountLegalEntityPublicHashedId);
    Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesInReviewAsync(long ukprn, string accountLegalEntityPublicHashedId);
    Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesForEmployerWithoutAccountLegalEntityPublicHashedIdAsync(long ukprn, string employerAccountId);
    Task<IEnumerable<T>> GetDraftVacanciesCreatedBeforeAsync<T>(DateTime staleDate);
    Task<IEnumerable<T>> GetReferredVacanciesSubmittedBeforeAsync<T>(DateTime staleDate);
    Task<int> GetVacancyCountForUserAsync(string userId);
    Task<IEnumerable<VacancyIdentifier>> GetVacanciesToCloseAsync(DateTime pointInTime);
}

public sealed class GetRequest(string url) : IGetApiRequest
{
    public string GetUrl => url;
}

public sealed class PostRequest(string url, object data) : IPostApiRequest
{
    public string PostUrl => url;
    public object Data { get; set; } = data;
}

public class OuterApiVacancyClient(
    IEncodingService encodingService,
    IOuterApiClient apimRecruitClient): IOuterApiVacancyClient
{
    private const string DeprecatedControllerName = "deprecated/vacancies";

    private static string EncodeDateTime(DateTime dateTime)
    {
        return $"{dateTime:yyyy-MM-ddTHH:mm:ssZ}";
    }
    
    public async Task CreateAsync(Vacancy vacancy)
    {
        // TODO: we'll want the returned data here at some point
        await apimRecruitClient.Post(new PostRequest($"vacancies/{vacancy.Id}", PostVacancyDto.From(vacancy, encodingService)));
    }

    public async Task UpdateAsync(Vacancy vacancy)
    {
        // TODO: we'll want the returned data here at some point
        await apimRecruitClient.Post(new PostRequest($"vacancies/{vacancy.Id}", PostVacancyDto.From(vacancy, encodingService)));
    }

    public async Task<long> GetNextVacancyIdAsync()
    {
        var response = await apimRecruitClient.Get<GetNextVacancyReferenceResponse>(new GetRequest("vacancies/vacancyreference"));
        return response.NextVacancyReference;
    }

    public async Task<Vacancy> GetVacancyAsync(long vacancyReference)
    {
        var response = await apimRecruitClient.Get<DataResponse<GetVacancyDto>>(new GetRequest($"vacancies/by/ref/{vacancyReference}"));
        return GetVacancyDto.ToDomain(response.Data, encodingService);
    }

    public async Task<Vacancy> GetVacancyAsync(Guid vacancyId)
    {
        var response = await apimRecruitClient.Get<DataResponse<GetVacancyDto>>(new GetRequest($"vacancies/{vacancyId}"));
        return GetVacancyDto.ToDomain(response.Data, encodingService);
    }
    
    public async Task<IList<Vacancy>> FindClosedVacanciesAsync(IList<long> vacancyReferences)
    {
        var response = await apimRecruitClient.Post<DataResponse<List<GetVacancyDto>>>(new PostRequest($"{DeprecatedControllerName}/findClosedVacancies", vacancyReferences));
        return response.Data?.Select(x => GetVacancyDto.ToDomain(x, encodingService)).ToList() ?? [];
    }
    
    public async Task<IEnumerable<ProviderVacancySummary>> GetVacanciesAssociatedToProviderAsync(long ukprn)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "ukprn", $"{ukprn}" },
        };
        var url = QueryHelpers.AddQueryString($"{DeprecatedControllerName}/getProviderVacancies", queryParams);
        var response = await apimRecruitClient.Get<DataResponse<List<ProviderVacancySummaryDto>>>(new GetRequest(url));
        return response.Data?.Select(ProviderVacancySummaryDto.ToDomain) ?? [];
    }

    public async Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesForLegalEntityAsync(long ukprn, string accountLegalEntityPublicHashedId)
    {
        var legalEntityId = encodingService.Decode(accountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId);
        var queryParams = new Dictionary<string, string>
        {
            { "ukprn", $"{ukprn}" },
            { "legalEntityId", $"{legalEntityId}" }
        };
            
        var url = QueryHelpers.AddQueryString($"{DeprecatedControllerName}/getProviderOwnedVacanciesForLegalEntity", queryParams);
        var response = await apimRecruitClient.Get<DataResponse<List<GetVacancyDto>>>(new GetRequest(url));
        return response.Data?.Select(x => GetVacancyDto.ToDomain(x, encodingService)).ToList() ?? [];
    }
    
    public async Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesInReviewAsync(long ukprn, string accountLegalEntityPublicHashedId)
    {
        var legalEntityId = encodingService.Decode(accountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId);
        var queryParams = new Dictionary<string, string>
        {
            { "ukprn", $"{ukprn}" },
            { "legalEntityId", $"{legalEntityId}" }
        };
            
        var url = QueryHelpers.AddQueryString($"{DeprecatedControllerName}/getProviderOwnedVacanciesInReviewForLegalEntity", queryParams);
        var response = await apimRecruitClient.Get<DataResponse<List<GetVacancyDto>>>(new GetRequest(url));
        return response.Data?.Select(x => GetVacancyDto.ToDomain(x, encodingService)).ToList() ?? [];
    }
    
    public async Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesForEmployerWithoutAccountLegalEntityPublicHashedIdAsync(long ukprn, string employerAccountId)
    {
        var accountId = encodingService.Decode(employerAccountId, EncodingType.AccountId);
        var queryParams = new Dictionary<string, string>
        {
            { "ukprn", $"{ukprn}" },
            { "accountId", $"{accountId}" }
        };
            
        var url = QueryHelpers.AddQueryString($"{DeprecatedControllerName}/getProviderOwnedVacanciesForEmployerWithoutAccountLegalEntityId", queryParams);
        var response = await apimRecruitClient.Get<DataResponse<List<GetVacancyDto>>>(new GetRequest(url));
        return response.Data?.Select(x => GetVacancyDto.ToDomain(x, encodingService)).ToList() ?? [];
    }
    
    public async Task<IEnumerable<T>> GetDraftVacanciesCreatedBeforeAsync<T>(DateTime staleDate)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "createdDate", EncodeDateTime(staleDate) }
        };
            
        var url = QueryHelpers.AddQueryString($"{DeprecatedControllerName}/getDraftVacanciesCreatedBefore", queryParams);
        var response = await apimRecruitClient.Get<DataResponse<IEnumerable<T>>>(new GetRequest(url));
        return response.Data;
    }
    
    public async Task<IEnumerable<T>> GetReferredVacanciesSubmittedBeforeAsync<T>(DateTime staleDate)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "submittedDate", EncodeDateTime(staleDate) }
        };
            
        var url = QueryHelpers.AddQueryString($"{DeprecatedControllerName}/getReferredVacanciesSubmittedBefore", queryParams);
        var response = await apimRecruitClient.Get<DataResponse<IEnumerable<T>>>(new GetRequest(url));
        return response.Data;
    }

    public async Task<int> GetVacancyCountForUserAsync(string userId)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "userId", userId }
        };
            
        var url = QueryHelpers.AddQueryString($"{DeprecatedControllerName}/getVacancyCountForUser", queryParams);
        var response = await apimRecruitClient.Get<DataResponse<int>>(new GetRequest(url));
        return response.Data;
    }

    public async Task<IEnumerable<VacancyIdentifier>> GetVacanciesToCloseAsync(DateTime pointInTime)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "pointInTime", EncodeDateTime(pointInTime) }
        };
            
        var url = QueryHelpers.AddQueryString($"{DeprecatedControllerName}/getVacanciesToClose", queryParams);
        var response = await apimRecruitClient.Get<DataResponse<IEnumerable<VacancyIdentifier>>>(new GetRequest(url));
        return response.Data;
    }
}