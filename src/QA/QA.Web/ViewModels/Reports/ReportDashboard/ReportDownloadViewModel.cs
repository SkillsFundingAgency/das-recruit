using System.IO;

namespace Esfa.Recruit.Qa.Web.ViewModels.Reports.ReportDashboard
{
    public class ReportDownloadViewModel
    {
        public Stream Content { get; set; }
        public string ReportName { get; set; }
    }
}
