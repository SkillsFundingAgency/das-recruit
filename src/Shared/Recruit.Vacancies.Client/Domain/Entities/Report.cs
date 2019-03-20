using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class Report
    {
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
    }
}
