using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.Models.ApplicationReviews
{
    public class ShareApplicationReviewsPostRequest : VacancyRouteModel
    {
        public List<VacancyApplication> ApplicationReviewsToShare { get; set; }
        public bool ShareApplicationsConfirmed { get; set; }
    }
}