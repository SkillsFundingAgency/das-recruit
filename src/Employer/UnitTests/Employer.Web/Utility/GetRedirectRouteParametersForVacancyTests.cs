using System;
using System.Collections.Generic;
using System.Text;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Utility
{
    public class GetRedirectRouteParametersForVacancyTests
    {
        [Theory]
        [InlineData(RouteNames.Title_Post, RouteNames.ShortDescription_Get)]
        [InlineData(RouteNames.ShortDescription_Post, RouteNames.Employer_Get)]
        [InlineData(RouteNames.Employer_Post, RouteNames.Training_Get)]
        [InlineData(RouteNames.Training_Post, RouteNames.Wage_Get)]
        [InlineData(RouteNames.Wage_Post, RouteNames.SearchResultPreview_Get)]
        public void GetRedirectRouteParametersForVacancy_ShouldGetNextPageWhenInWizardMode(string currentRouteName, string expectedRedirectRouteName)
        {
            //If the vacancy has not completed part 1 then we still need to go through the steps 1-6 (aka wizard mode)
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                EmployerLocation = new Address { Postcode = "has a value" },
                ShortDescription = "has a value",
                ProgrammeId = "has a value",
                Wage = new Wage { WageType = WageType.FixedWage },
                HasCompletedPart1 = false
            };
            
            var redirectRouteParameters = Esfa.Recruit.Employer.Web.Utility.GetRedirectRouteParametersForVacancy(
                vacancy, "the preview anchor which should be ignored if not redirecting to the preview page", currentRouteName);

            redirectRouteParameters.RouteName.Should().Be(expectedRedirectRouteName);
            redirectRouteParameters.RouteValues.VacancyId.Should().Be(vacancy.Id);
            redirectRouteParameters.Fragment.Should().Be(null);
        }
    }
}
