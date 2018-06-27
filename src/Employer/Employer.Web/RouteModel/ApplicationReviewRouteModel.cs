using System;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.RouteModel
{
    public class ApplicationReviewRouteModel : VacancyRouteModel
    {
        [FromRoute]
        public Guid ApplicationReviewId { get; set; }
    }
}
