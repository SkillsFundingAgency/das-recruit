using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.RouteModel
{
    public class ApplicationReviewRouteModel : RouteModel
    {
        [FromRoute]
        public Guid ApplicationReviewId { get; set; }
    }
}
