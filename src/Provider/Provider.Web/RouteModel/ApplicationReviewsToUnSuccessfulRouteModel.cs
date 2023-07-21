using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.RouteModel
{
    public class ApplicationReviewsToUnSuccessfulRouteModel : VacancyRouteModel
    {
        public List<Guid> ApplicationsToUnSuccessful { get; set; }
    }
}