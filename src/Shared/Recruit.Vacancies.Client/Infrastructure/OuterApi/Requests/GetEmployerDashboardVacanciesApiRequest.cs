using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public record GetEmployerDashboardVacanciesApiRequest(long AccountId,int PageNumber, List<ApplicationReviewStatus> ApplicationReviewStatuses) : IGetApiRequest
{
    public string GetUrl => $"employers/dashboard/{AccountId}/vacancies?pageNumber={PageNumber}&status={string.Join("&status=",ApplicationReviewStatuses)}";
}