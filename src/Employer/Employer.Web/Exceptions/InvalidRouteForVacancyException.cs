using System;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.Exceptions
{
    [Serializable]
    public class InvalidRouteForVacancyException : Exception
    {
        public InvalidRouteForVacancyException(string message, string routeNameToRedirectTo, VacancyRouteModel routeValues) : base(message)
        {
            RouteNameToRedirectTo = routeNameToRedirectTo;
            RouteValues = routeValues;
        }

        public string RouteNameToRedirectTo { get; }

        public VacancyRouteModel RouteValues { get; set; }
    }
}
