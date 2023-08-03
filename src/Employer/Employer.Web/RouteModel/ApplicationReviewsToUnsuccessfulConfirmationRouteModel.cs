using System.Collections.Generic;
using System;

namespace Esfa.Recruit.Employer.Web.RouteModel
{

    public class ApplicationReviewsToUnsuccessfulConfirmationRouteModel : VacancyRouteModel
    {
        public List<Guid> ApplicationsToUnsuccessful { get; set; }
    }
}
