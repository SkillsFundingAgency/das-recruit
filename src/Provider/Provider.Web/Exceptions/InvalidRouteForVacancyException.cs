using System;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Provider.Web.Exceptions;

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