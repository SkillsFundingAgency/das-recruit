using System;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class VacancyRouteParameters
    {
        public VacancyRouteParameters(string routeName, Guid? vacancyId = null, string fragment = null)
        {
            RouteName = routeName;
            Fragment = fragment;

            if (vacancyId.HasValue)
            {
                RouteValues = new VacancyRouteParametersRouteValues(vacancyId.Value);
            }
        }

        public string RouteName { get; }
        public VacancyRouteParametersRouteValues RouteValues { get; }
        public string Fragment { get; }
    }

    public class VacancyRouteParametersRouteValues
    {
        public VacancyRouteParametersRouteValues(Guid vacancyId)
        {
            VacancyId = vacancyId;
        }

        public Guid VacancyId { get; }
    }


}
