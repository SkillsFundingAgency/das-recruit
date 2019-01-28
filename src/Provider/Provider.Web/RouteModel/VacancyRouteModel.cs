using System;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.RouteModel
{
    public class VacancyRouteModel
    {
        [FromRoute]
        public long Ukprn { get; set; }

        [FromRoute]
        public Guid? VacancyId { get; set; }
    }
}