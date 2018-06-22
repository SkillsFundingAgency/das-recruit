using Microsoft.AspNetCore.Mvc;
using System;

namespace Esfa.Recruit.Employer.Web.RouteModel
{
    public class VacancyRouteModel : RouteModel
    {
        [FromRoute]
        public Guid VacancyId { get; set; }
    }
}