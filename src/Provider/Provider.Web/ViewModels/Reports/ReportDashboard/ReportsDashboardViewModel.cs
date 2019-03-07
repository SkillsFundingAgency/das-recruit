using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Humanizer;
using Microsoft.EntityFrameworkCore.Internal;

namespace Esfa.Recruit.Provider.Web.ViewModels.Reports.ReportDashboard
{
    public class ReportsDashboardViewModel
    {
        public int ProcessingCount { get; set; }
        public IEnumerable<ReportRowViewModel> Reports { get; set; }

        public bool HasReports => Reports.Any();

        public string ProcessingCaption => $"{"report".ToQuantity(ProcessingCount)}";
    }

    public class ReportRowViewModel
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; } 
        public string CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long DownloadCount { get; set; }
        public bool IsProcessing { get; set; }
        public ReportStatus Status { get; set; }

        public bool CanDownload => Status == ReportStatus.Generated;
        public bool IsNew => Status == ReportStatus.Generated && DownloadCount == 0;
    }
}
