using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;

internal class VacancyDashboardAggQueryResponseDto
{
    public VacancyDashboardSummary Id { get; set; }
    public int StatusCount { get; set;  }
        
}
internal class VacancyDashboardSummary
{
    public VacancyStatus Status { get; set; }
    public bool ClosingSoon { get; set; }
}