using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public class Report(
    Guid id,
    ReportOwner owner,
    ReportStatus status,
    string reportName,
    ReportType reportType,
    Dictionary<string, object> parameters,
    VacancyUser requestedBy,
    DateTime requestedOn)
{
    public Guid Id { get; set; } = id;
    public ReportOwner Owner { get; set; } = owner;
    public ReportStatus Status { get; set; } = status;
    public ReportType ReportType { get; set; } = reportType;
    public string ReportName { get; set; } = reportName;
    public Dictionary<string, object> Parameters { get; set; } = parameters;
    public VacancyUser RequestedBy { get; set; } = requestedBy;
    public DateTime RequestedOn { get; set; } = requestedOn;
    public DateTime? GenerationStartedOn { get; set; }
    public DateTime? GeneratedOn { get; set; }
    public int DownloadCount { get; set; } = 0;
    public string Data { get; set; }
    public IEnumerable<KeyValuePair<string, string>> Headers { get; set; }
    public string Query { get; set; }
}