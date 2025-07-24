using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ShareMultipleApplicationReviewsViewModel : ApplicationReviewsToShareRouteModel
    {
        public long VacancyReference { get; set; }
        public List<VacancyApplication> VacancyApplications { get; set; }
        public bool CanShowCandidateAppliedLocations => VacancyApplications?.Any(app => app.CanShowCandidateAppliedLocations) ?? false;
    }
}
