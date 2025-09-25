using System;
using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReviews;

namespace Esfa.Recruit.Provider.Web.RouteModel
{
    public class ApplicationReviewsToUnsuccessfulRouteModel : VacancyRouteModel
    {
        public bool IsMultipleApplications { get; set; }
    }
}