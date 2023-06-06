using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ShareMultipleApplicationReviewsViewModel : ApplicationReviewsToShareRouteModel
    {
        public long VacancyReference { get; set; }
        public bool SelectedAllApplications { get; set; } // tbc if needed
        public List<VacancyApplication> VacancyApplications { get; set; }
    }
}
