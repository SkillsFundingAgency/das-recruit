using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalVacanciesForReview { get; set; }
        public int TotalVacanciesBrokenSla { get; set; }
        public int TotalVacanciesResubmitted { get; set; }
        public List<ReviewDashboardItem> AllReviews { get; set; }
        public string DashboardMessage { get; set; }

        public string SearchTerm { get; set; }

        public string LastSearchTerm { get; set; }

        public List<VacancyReviewSearchModel> SearchResults { get; set; } = new List<VacancyReviewSearchModel>();
        public bool DisplayLastSearchTerm => !string.IsNullOrEmpty(LastSearchTerm);
        public bool DisplayNoResultsMessage => DisplayLastSearchTerm && !SearchResults.Any();
        public bool DisplaySearchResults => DisplayLastSearchTerm && SearchResults.Any();
        public bool HasDashboardMessage => string.IsNullOrWhiteSpace(DashboardMessage) == false;
    }
}