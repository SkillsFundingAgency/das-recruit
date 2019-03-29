using System;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.RouteModel
{
    public class ReportRouteModel
    {
        [FromRoute]
        public long Ukprn { get; set; }

        [FromRoute]
        public Guid ReportId { get; set; }
    }
}