using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ApplicationReviewsToUnSuccessfulConfirmationViewModel : ApplicationReviewsStatusChangeModel
    {
        public new IList<VacancyApplication> ApplicationsToUnSuccessful { get; set; }
    }
}
