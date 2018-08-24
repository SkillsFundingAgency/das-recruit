using System.Collections.Generic;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalVacanciesForReview { get; set; }
        public int TotalVacanciesBrokenSla { get; set; }
        public int TotalVacanciesResubmitted { get; set; }

        public List<ReviewDashboardItem> AllReviews { get; set; }

        public string SearchTerm { get; set; }

        public string LastSearchTerm { get; set; }

        public List<VacancyReviewSearchModel> SearchResults { get; set; } = new List<VacancyReviewSearchModel>();
        public bool IsPostBack { get; set; }
    }
}