using System;
using System.Collections.Generic;
using System.Text;

namespace Esfa.Recruit.Vacancies.Client.Domain.Exceptions
{
    [Serializable]
    public class InvalidRouteForVacancyException : Exception
    {
        public InvalidRouteForVacancyException(string message, string routeNameToRedirectTo, object routeValues) : base(message)
        {
            RouteNameToRedirectTo = routeNameToRedirectTo;
            RouteValues = routeValues;
        }

        public string RouteNameToRedirectTo { get; }

        public object RouteValues { get; set; }
    }
}
