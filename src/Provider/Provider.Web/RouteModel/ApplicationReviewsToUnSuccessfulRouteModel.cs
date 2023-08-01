using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.RouteModel
{
    public class ApplicationReviewsToUnsuccessfulRouteModel : VacancyRouteModel
    {
        public List<Guid> ApplicationsToUnsuccessful { get; set; }
    }
}