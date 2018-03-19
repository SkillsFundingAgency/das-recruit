using Microsoft.AspNetCore.Mvc;
using System;

namespace Esfa.Recruit.Employer.Web.RouteModel
{
    public class VacancyRouteModel
    {
        private string _employerAccountId;

        [FromRoute]
        public string EmployerAccountId
        {
            get { return _employerAccountId; }
            set { _employerAccountId = value.ToUpper(); }
        }

        [FromRoute]
        public Guid VacancyId { get; set; }       
    }
}