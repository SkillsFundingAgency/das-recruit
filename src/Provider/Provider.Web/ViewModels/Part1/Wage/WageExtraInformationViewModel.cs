using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Wage
{
    public class WageExtraInformationViewModel : WageViewModel
    {
        public string RouteName
        {
            get
            {
                var routeMappings = new Dictionary<WageType, string>
                {
                    { Recruit.Vacancies.Client.Domain.Entities.WageType.FixedWage, RouteNames.CustomWage_Get },
                    { Recruit.Vacancies.Client.Domain.Entities.WageType.CompetitiveSalary, RouteNames.SetCompetitivePayRate_Get },
                    { Recruit.Vacancies.Client.Domain.Entities.WageType.NationalMinimumWage, RouteNames.Wage_Get },
                    { Recruit.Vacancies.Client.Domain.Entities.WageType.NationalMinimumWageForApprentices, RouteNames.Wage_Get },
                };

                return routeMappings.TryGetValue(WageType ?? Recruit.Vacancies.Client.Domain.Entities.WageType.NationalMinimumWage, out var routeName) ? routeName : RouteNames.ProviderCheckYourAnswersGet;
            }
        }

    }
}