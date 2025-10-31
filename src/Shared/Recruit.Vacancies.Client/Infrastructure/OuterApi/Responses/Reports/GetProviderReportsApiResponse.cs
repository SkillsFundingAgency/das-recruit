using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Reports;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Reports;
public record GetProviderReportsApiResponse
{
    public List<Report> Reports { get; init; }
}
