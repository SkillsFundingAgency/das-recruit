using System;
using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Models.ApplicationReviews
{
    public class ApplicationReviewsToUnSuccessfulRequest : VacancyRouteModel
    {
        [BindProperty(Name = "ApplicationsToShare")]
        public List<Guid> ApplicationsToUnSuccessful { get; set; }
    }
}