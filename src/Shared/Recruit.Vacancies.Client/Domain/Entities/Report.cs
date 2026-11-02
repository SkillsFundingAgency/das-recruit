using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public class Report
{
    public Report(Guid id, ReportOwner owner, ReportStatus status, string reportName,
        ReportType reportType, Dictionary<string, object> parameters, VacancyUser requestedBy,
        DateTime requestedOn)
    {
        Id = id;
        Owner = owner;
        Status = status;
        ReportName = reportName;
        ReportType = reportType;
        Parameters = parameters;
        RequestedBy = requestedBy;
        RequestedOn = requestedOn;
        DownloadCount = 0;
    }

    public Guid Id { get; set; }
    public ReportOwner Owner { get; set; }
    public ReportStatus Status { get; set; }
    public ReportType ReportType { get; set; }
    public string ReportName { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public VacancyUser RequestedBy { get; set; }
    public DateTime RequestedOn { get; set; }
    public DateTime? GenerationStartedOn { get; set; }
    public DateTime? GeneratedOn { get; set; }
    public int DownloadCount { get; set; }
    public string Data { get; set; }
    public IEnumerable<KeyValuePair<string, string>> Headers { get; set; }
    public string Query { get; set; }
}