using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.RouteModel
{
    public class VacancyRouteModel
    {
        private string _employerAccountId;

        [FromRoute]
        public string EmployerAccountId
        {
            get => _employerAccountId;
            set => _employerAccountId = value.ToUpper();
        }

        [FromRoute]
        public Guid VacancyId { get; set; }
        
        public Dictionary<string, string> RouteDictionary
        {
            get
            {
                var routeDictionary = new Dictionary<string, string>
                {
                    {"EmployerAccountId", EmployerAccountId}
                };
                if(VacancyId != Guid.Empty)
                {
                    routeDictionary.Add("VacancyId", VacancyId.ToString());
                }
                return routeDictionary;
            }
        }
    }
}