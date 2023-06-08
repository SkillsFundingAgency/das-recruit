using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ShareMultipleApplicationReviewsViewModel : ApplicationReviewsToShareRouteModel
    {
        public long VacancyReference { get; set; }
        public List<VacancyApplication> VacancyApplications { get; set; }
    }
}
