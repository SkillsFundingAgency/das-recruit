using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Humanizer;

namespace Esfa.Recruit.Provider.Web.ViewModels.Reports.ReportDashboard
{
    public class ReportsDashboardViewModel
    {
        public int ProcessingCount { get; set; }
        public IEnumerable<ReportRowViewModel> Reports { get; set; }

        public bool HasReports => Reports.Any();

        public string ProcessingCaption => $"{"report".ToQuantity(ProcessingCount)}";

        public bool IsProcessingReports => ProcessingCount > 0;
        public long Ukprn { get; set; }
    }

    public class ReportRowViewModel
    {
        public Guid ReportId { get; set; }
        public string ReportName { get; set; }
        public string CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public int DownloadCount { get; set; }
        public bool IsProcessing { get; set; }
        public ReportStatus Status { get; set; }

        public bool CanDownload => Status == ReportStatus.Generated;
        public bool IsNew => DownloadCount == 0;
    }
}
