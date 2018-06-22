using System;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview
{
    public class TrainingCoursesViewModel
    {
        public string Provider { get; set; }
        public string Title { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string FromDateAsText => FromDate.ToString("MMMM yyyy");
        public string ToDateAsText => ToDate.ToString("MMMM yyyy");
    }
}