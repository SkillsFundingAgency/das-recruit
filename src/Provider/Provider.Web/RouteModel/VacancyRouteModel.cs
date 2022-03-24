using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.RouteModel
{
    public class VacancyRouteModel
    {
        [FromRoute]
        public long Ukprn { get; set; }

        [FromRoute]
        public Guid? VacancyId { get; set; }

        public Dictionary<string, string> RouteDictionary
        {
            get
            {
                var routeDictionary = new Dictionary<string, string>
                {
                    {"Ukprn", Ukprn.ToString()}
                };  
                if(VacancyId != null)
                {
                    routeDictionary.Add("VacancyId", VacancyId.ToString());
                }
                return routeDictionary;
            }
        }
    }
}