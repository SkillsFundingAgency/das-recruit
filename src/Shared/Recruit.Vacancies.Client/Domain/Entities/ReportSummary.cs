using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class ReportSummary
    {
        public Guid Id { get; set; }
        public ReportOwner Owner { get; set; }
        public ReportStatus Status { get; set; }
        public ReportType ReportType { get; set; }
        public List<ReportParameter> Parameters { get; set; }
        public VacancyUser RequestedBy { get; set; }
        public DateTime RequestedOn { get; set; }
        public DateTime? GeneratedOn { get; set; }
        public long DownloadCount { get; set; }

        public bool IsProcessing => Status == ReportStatus.New || Status == ReportStatus.InProgress;
    }
}
