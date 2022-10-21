using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.RouteModel
{
    public class ApplicationReviewRouteModel : VacancyRouteModel
    {
        [FromRoute]
        public Guid ApplicationReviewId { get; set; }

        public Dictionary<string, string> ApplicationRouteDictionary
        {
            get
            {
                var dictionary = base.RouteDictionary;
                if(ApplicationReviewId != Guid.Empty)
                {
                    dictionary.Add("ApplicationReviewId", ApplicationReviewId.ToString());
                }
                return dictionary;
            }
        }
    }
}
