using System;
using System.Text.Json;

namespace Esfa.Recruit.Vacancies.Client.Domain.Reports;
public record ReportCriteria
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int? Ukprn { get; set; }

    public string ToJson() => JsonSerializer.Serialize(this);
}