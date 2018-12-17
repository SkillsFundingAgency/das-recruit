using System;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Provider.Web.Exceptions
{
    [Serializable]
    public class InvalidRouteForVacancyException : RecruitException
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
