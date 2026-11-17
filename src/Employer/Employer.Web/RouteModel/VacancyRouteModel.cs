using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.RouteModel;

public class VacancyRouteModel
{
    [FromRoute]
    public string EmployerAccountId
    {
        get;
        set => field = value.ToUpper();
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