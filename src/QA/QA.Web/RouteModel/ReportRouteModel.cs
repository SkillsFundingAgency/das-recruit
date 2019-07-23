using System;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.RouteModel
{
    public class ReportRouteModel
    {
        [FromRoute]
        public Guid ReportId { get; set; }
    }
}
