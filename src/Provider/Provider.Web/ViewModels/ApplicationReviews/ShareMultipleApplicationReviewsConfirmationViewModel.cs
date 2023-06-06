using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ShareMultipleApplicationReviewsConfirmationViewModel : VacancyRouteModel
    {
        public List<VacancyApplication> ApplicationReviewsToShare { get; set; }
    }
}
