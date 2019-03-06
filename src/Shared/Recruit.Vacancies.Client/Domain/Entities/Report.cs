using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class Report
    {
        public Guid Id { get; set; }
        public ReportStatus Status { get; set; }
        public ReportType ReportType { get; set; }
        public List<ReportParameter> Parameters { get; set; }
        public VacancyUser RequestedBy { get; set; }
        public DateTime RequestedOn { get; set; }
        public DateTime? GeneratedOn { get; set; }
        public long DownloadCount { get; set; }
    }
}
