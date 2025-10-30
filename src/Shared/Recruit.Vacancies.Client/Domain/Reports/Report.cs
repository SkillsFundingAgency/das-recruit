using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Esfa.Recruit.Vacancies.Client.Domain.Reports;
public class Report
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; } = null!;
    public ReportType Type { get; set; }
    public ReportOwnerType OwnerType { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public int DownloadCount { get; set; } = 0;
    public string DynamicCriteria { get; set; } = null!;
    private bool TryGetCriteria(out ReportCriteria? criteria)
    {
        try
        {
            criteria = JsonSerializer.Deserialize<ReportCriteria>(DynamicCriteria);
            return criteria is not null;
        }
        catch
        {
            criteria = null;
            return false;
        }
    }

    [JsonIgnore]
    public ReportCriteria? Criteria
    {
        get
        {
            _ = TryGetCriteria(out var criteria);
            return criteria;
        }
    }

    public static ReportSummary ToReportSummary(Report report) =>
        new()
        {
            Id = report.Id,
            Owner = new ReportOwner
            {
                OwnerType = report.OwnerType,
                Ukprn = report.Criteria?.Ukprn
            },
            Status = ReportStatus.Generated,
            ReportType = report.Type,
            ReportName = report.Name,
            RequestedBy = new VacancyUser
            {
                Name = report.CreatedBy,
                Ukprn = report.Criteria?.Ukprn
            },
            RequestedOn = report.CreatedDate,
            GeneratedOn = report.CreatedDate,
            DownloadCount = report.DownloadCount,
        };

    public Entities.Report ToEntity(Report report)
    {
        var criteria = report.Criteria ?? new ReportCriteria();
        var isProvider = report.OwnerType == ReportOwnerType.Provider;

        var reportParams = new Dictionary<string, object>
        {
            { ReportParameterName.FromDate, criteria.FromDate },
            { ReportParameterName.ToDate, criteria.ToDate }
        };

        if (isProvider)
        {
            reportParams[ReportParameterName.Ukprn] = criteria.Ukprn;
        }

        return new Entities.Report(
            report.Id,
            new ReportOwner
            {
                OwnerType = report.OwnerType,
                Ukprn = criteria.Ukprn
            },
            ReportStatus.Generated,
            Name,
            Type,
            reportParams,
            new VacancyUser
            {
                Name = report.CreatedBy,
                Ukprn = criteria.Ukprn,
                UserId = report.UserId,
                DfEUserId = report.UserId
            },
            CreatedDate
        );
    }
}
