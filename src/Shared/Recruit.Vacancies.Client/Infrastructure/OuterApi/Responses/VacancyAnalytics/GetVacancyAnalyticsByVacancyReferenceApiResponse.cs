using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.VacancyAnalytics;

public record GetVacancyAnalyticsByVacancyReferenceApiResponse
{
    public required long VacancyReference { get; init; }
    public DateTime UpdatedDate { get; init; }
    public List<Domain.VacancyAnalytics.VacancyAnalytics> Analytics { get; set; } = [];
}