using System;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public class ReviewDashboardItem
    {
        public Guid ReviewId { get; set; }
        public long VacancyReference { get; set; }
        public string Title { get; set; }  
        public string Status { get; set; }
    }
}