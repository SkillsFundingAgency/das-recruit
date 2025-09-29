using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
public record GetVacanciesByUkprnApiRequest(int Ukprn,
    int Page = 1,
    int PageSize = 25,
    string SortColumn = "",
    string SortOrder = "Desc",
    FilteringOptions FilterBy = FilteringOptions.All,
    string SearchTerm = "") : IGetApiRequest
{
    public string GetUrl => $"providers/{Ukprn}/vacancies?page={Page}&pageSize={PageSize}&sortColumn={SortColumn}&sortOrder={SortOrder}&filterBy={FilterBy}&searchTerm={SearchTerm}";
}