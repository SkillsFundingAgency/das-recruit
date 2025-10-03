using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
public record GetVacanciesByAccountIdApiRequest(long AccountId,
    int Page = 1,
    int PageSize = 25,
    string SortColumn = "",
    string SortOrder = "Desc",
    FilteringOptions FilterBy = FilteringOptions.All,
    string SearchTerm = "") : IGetApiRequest
{
    public string GetUrl => $"employerAccounts/{AccountId}/vacancies?page={Page}&pageSize={PageSize}&sortColumn={SortColumn}&sortOrder={SortOrder}&filterBy={FilterBy}&searchTerm={SearchTerm}";
}
