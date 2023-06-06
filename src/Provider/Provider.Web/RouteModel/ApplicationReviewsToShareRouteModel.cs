using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.RouteModel
{
    public class ApplicationReviewsToShareRouteModel : VacancyRouteModel
    {
        public List<Guid> ApplicationsToShare { get; set; }
    }
}