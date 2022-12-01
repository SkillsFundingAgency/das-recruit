using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalVacanciesForReview { get; set; }
        public int TotalVacanciesBrokenSla { get; set; }
        public int TotalVacanciesResubmitted { get; set; }

        public int TotalVacanciesSubmittedTwelveToTwentyFourHours { get; set; }
        public string DashboardMessage { get; set; }

        public string SearchTerm { get; set; }

        public string LastSearchTerm { get; set; }

        public VacancyReviewSearchResultViewModel SearchResult { get; set; }
        public bool HasSearchResult => SearchResult != null;
        public bool DisplayLastSearchTerm => !string.IsNullOrEmpty(LastSearchTerm);
        public bool DisplayNoSearchResultsMessage => DisplayLastSearchTerm && HasSearchResult == false;
        public bool DisplaySearchResults => DisplayLastSearchTerm && HasSearchResult;
        public bool HasDashboardMessage => string.IsNullOrWhiteSpace(DashboardMessage) == false;
        public bool IsUserAdmin { get; set; }
        public List<VacancyReviewSearchResultViewModel> InProgressVacancies { get; set; } = new List<VacancyReviewSearchResultViewModel>();
        public bool DisplayNoInProgressVacanciesMessage => IsUserAdmin && !InProgressVacancies.Any();
        public bool DisplayInProgressResults => IsUserAdmin && InProgressVacancies.Any();
    }
}