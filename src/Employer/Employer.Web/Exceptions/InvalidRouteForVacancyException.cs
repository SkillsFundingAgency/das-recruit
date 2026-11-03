using System;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Employer.Web.Exceptions;

[Serializable]
public class InvalidRouteForVacancyException(
    string message,
    string routeNameToRedirectTo,
    VacancyRouteModel routeValues)
    : RecruitException(message)
{
    public string RouteNameToRedirectTo { get; } = routeNameToRedirectTo;

    public VacancyRouteModel RouteValues { get; set; } = routeValues;
}