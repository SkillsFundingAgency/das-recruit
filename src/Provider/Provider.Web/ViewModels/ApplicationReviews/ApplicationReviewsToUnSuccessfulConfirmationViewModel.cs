using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ApplicationReviewsToUnsuccessfulConfirmationViewModel : ApplicationReviewsToUnsuccessfulModel
    {
        public new IList<VacancyApplication> ApplicationsToUnsuccessful { get; set; }
    }
}
