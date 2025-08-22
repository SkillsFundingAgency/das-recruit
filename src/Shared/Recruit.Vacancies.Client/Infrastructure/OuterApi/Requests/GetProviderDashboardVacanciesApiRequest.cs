using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public record GetProviderDashboardVacanciesApiRequest(long Ukprn, int PageNumber, int PageSize, List<ApplicationReviewStatus> ApplicationReviewStatuses) : IGetApiRequest
{
    public string GetUrl => $"providers/dashboard/{Ukprn}/vacancies?pageNumber={PageNumber}&pageSize={PageSize}&status={string.Join("&status=",ApplicationReviewStatuses)}";
}