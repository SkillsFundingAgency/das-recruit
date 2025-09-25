using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;

public class VacancyAnalyticsSummaryV2 : QueryProjectionBase
{
    public VacancyAnalyticsSummaryV2() : base(QueryViewType.VacancyAnalyticsSummaryV2.TypeName)
    {
    }
    public string VacancyReference { get; set; }
    public List<VacancyAnalytics> VacancyAnalytics { get; set; }
}

public class VacancyAnalytics
{
    public DateTime AnalyticsDate { get; set; }
    public int ViewsCount { get; set; }
    public int SearchResultsCount { get; set; }
    public int ApplicationStartedCount { get; set; }
    public int ApplicationSubmittedCount { get; set; }
}