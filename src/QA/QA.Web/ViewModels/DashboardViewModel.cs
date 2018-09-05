using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;

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
        public bool DisplayLastSearchTerm => !string.IsNullOrEmpty(LastSearchTerm);
        public bool DisplayNoSearchResultsMessage => DisplayLastSearchTerm && !SearchResults.Any();
        public bool DisplaySearchResults => DisplayLastSearchTerm && SearchResults.Any();
        public bool DisplayInProgressVacancies { get; set; }
        public List<VacancyReviewSearchModel> InProgressVacancies { get; set; } = new List<VacancyReviewSearchModel>();
        public bool DisplayNoInProgressVacanciesMessage => DisplayInProgressVacancies && !InProgressVacancies.Any();
        public bool DisplayInProgressResults => DisplayInProgressVacancies && InProgressVacancies.Any();
    }
}