using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ShareMultipleApplicationReviewsConfirmationViewModel : VacancyRouteModel
    {
        public IList<VacancyApplication> ApplicationReviewsToShare { get; set; }
        public bool? ShareApplicationsConfirmed { get; set; }
        public string ShareApplicationHeaderTitle => ApplicationReviewsToShare.Count() == 1 ? "Share an application" : "Share multiple applications";
        public bool SharingMultipleApplications => ApplicationReviewsToShare.Count() > 1 ? true : false;
    }
}
