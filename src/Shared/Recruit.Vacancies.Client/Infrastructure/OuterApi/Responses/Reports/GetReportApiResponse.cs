using Esfa.Recruit.Vacancies.Client.Domain.Reports;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Reports;
public record GetReportApiResponse
{
    public Report Report { get; init; }
}