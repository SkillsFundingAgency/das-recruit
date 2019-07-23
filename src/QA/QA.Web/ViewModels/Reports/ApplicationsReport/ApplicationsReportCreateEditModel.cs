namespace Esfa.Recruit.Qa.Web.ViewModels.Reports.ApplicationsReport
{
    public class ApplicationsReportCreateEditModel
    {

        public DateRangeType? DateRange { get; set; }

        public string FromDay { get; set; }
        public string FromMonth { get; set; }
        public string FromYear { get; set; }

        public string ToDay { get; set; }
        public string ToMonth { get; set; }
        public string ToYear { get; set; }

        public string FromDate
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FromDay) ||
                    string.IsNullOrWhiteSpace(FromMonth) ||
                    string.IsNullOrWhiteSpace(FromYear))
                {
                    return null;
                }

                return $"{FromDay}/{FromMonth}/{FromYear}";
            }
        }

        public string ToDate
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ToDay) ||
                    string.IsNullOrWhiteSpace(ToMonth) ||
                    string.IsNullOrWhiteSpace(ToYear))
                {
                    return null;
                }

                return $"{ToDay}/{ToMonth}/{ToYear}";
            }
        }
    }

}
