using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.Configuration.Routing;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage
{
    public class WageViewModel : WageEditModel
    {
        public VacancyRouteParameters CancelButtonRouteParameters { get; set; }
        public bool ShowStep => CancelButtonRouteParameters.RouteName != RouteNames.Vacancy_Preview_Get;
        public string SubmitButtonText => CancelButtonRouteParameters.RouteName == RouteNames.Vacancy_Preview_Get ? "Save and Preview" : "Save and Continue";

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(Duration),
            nameof(WorkingWeekDescription),
            nameof(WeeklyHours),
            nameof(WageType),
            nameof(FixedWageYearlyAmount),
            nameof(WageAdditionalInformation)
        };
    }
}
