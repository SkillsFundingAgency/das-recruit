using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalVacanciesForReview { get; set; }
        public int TotalVacanciesBrokenSla { get; set; }
        public int TotalVacanciesResubmitted { get; set; }
        public string DashboardMessage { get; set; }
        public string SearchTerm { get; set; }
        public string LastSearchTerm { get; set; }
        public List<Task<VacancyReviewSearchResultViewModel>> SearchResults { get; set; } = new List<Task<VacancyReviewSearchResultViewModel>>();
        public bool DisplayLastSearchTerm => !string.IsNullOrEmpty(LastSearchTerm);
        public bool DisplayNoSearchResultsMessage => DisplayLastSearchTerm && !SearchResults.Any();
        public bool DisplaySearchResults => DisplayLastSearchTerm && SearchResults.Any();
        public bool HasDashboardMessage => string.IsNullOrWhiteSpace(DashboardMessage) == false;
        public bool DisplayInProgressVacancies { get; set; }
        public List<Task<VacancyReviewSearchResultViewModel>> InProgressVacancies { get; set; } =new List<Task<VacancyReviewSearchResultViewModel>>();
        public bool DisplayNoInProgressVacanciesMessage => DisplayInProgressVacancies && !InProgressVacancies.Any();
        public bool DisplayInProgressResults => DisplayInProgressVacancies && InProgressVacancies.Any();
    }
}