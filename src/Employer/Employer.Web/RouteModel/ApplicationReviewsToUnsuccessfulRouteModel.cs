using System.Collections.Generic;
using System;

namespace Esfa.Recruit.Employer.Web.RouteModel
{
    public class ApplicationReviewsToUnsuccessfulRouteModel : VacancyRouteModel
    {
        public List<Guid> ApplicationsToUnsuccessful { get; set; }
    }
}
