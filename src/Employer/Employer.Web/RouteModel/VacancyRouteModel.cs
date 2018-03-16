using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.RouteModel
{
    public class VacancyRouteModel
    {
        [FromRoute]
        [Required]
        public Guid VacancyId { get; set; }
        
        [FromRoute]
        [Required]
        public string EmployerAccountId { get; set; }
    }
}
