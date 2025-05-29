using System;
using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.Models.ApplicationReviews
{
    public class ShareApplicationReviewsRequest : VacancyRouteModel
    {
        public List<Guid> ApplicationsToShare { get; set; }
    }
}
