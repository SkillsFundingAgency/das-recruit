using System;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.RouteModel
{
    public class ApplicationReviewRouteModel : VacancyRouteModel
    {
        [FromRoute]
        public Guid ApplicationReviewId { get; set; }
    }
}
