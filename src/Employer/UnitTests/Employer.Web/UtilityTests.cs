using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web
{
    public class UtilityTests
    {
        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData("any other route", true)]
        public void CheckRouteIsValidForVacancy_ShouldRedirectToTitle(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e")
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.Title_Get);
        }

        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData(RouteNames.Employer_Get, false)]
        [InlineData(RouteNames.Employer_Post, false)]
        [InlineData("any other route", true)]
        public void CheckRouteIsValidForVacancy_ShouldRedirectToEmployer(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value"
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.Employer_Get);
        }

        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData(RouteNames.Employer_Get, false)]
        [InlineData(RouteNames.Employer_Post, false)]
        [InlineData(RouteNames.ShortDescription_Get, false)]
        [InlineData(RouteNames.ShortDescription_Post, false)]
        [InlineData("any other route", true)]
        public void CheckRouteIsValidForVacancy_ShouldRedirectToShortDescription(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                EmployerLocation = new Address { Postcode = "has a value"}
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.ShortDescription_Get);
        }

        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData(RouteNames.Employer_Get, false)]
        [InlineData(RouteNames.Employer_Post, false)]
        [InlineData(RouteNames.ShortDescription_Get, false)]
        [InlineData(RouteNames.ShortDescription_Post, false)]
        [InlineData(RouteNames.Training_Get, false)]
        [InlineData(RouteNames.Training_Post, false)]
        [InlineData("any other route", true)]
        public void CheckRouteIsValidForVacancy_ShouldRedirectToTraining(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                EmployerLocation = new Address { Postcode = "has a value" },
                ShortDescription = "has a value"
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.Training_Get);
        }

        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData(RouteNames.Employer_Get, false)]
        [InlineData(RouteNames.Employer_Post, false)]
        [InlineData(RouteNames.ShortDescription_Get, false)]
        [InlineData(RouteNames.ShortDescription_Post, false)]
        [InlineData(RouteNames.Training_Get, false)]
        [InlineData(RouteNames.Training_Post, false)]
        [InlineData(RouteNames.Wage_Get, false)]
        [InlineData(RouteNames.Wage_Post, false)]
        [InlineData("any other route", true)]
        public void CheckRouteIsValidForVacancy_ShouldRedirectToWage(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                EmployerLocation = new Address { Postcode = "has a value" },
                ShortDescription = "has a value",
                ProgrammeId = "has a value"
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.Wage_Get);
        }

        [Theory]
        [InlineData("any other route", false)]
        public void CheckRouteIsValidForVacancy_ShouldNotRedirect(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                EmployerLocation = new Address { Postcode = "has a value" },
                ShortDescription = "has a value",
                ProgrammeId = "has a value",
                Wage = new Wage { WageType = WageType.FixedWage}
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, null);
        }

        private void CheckRouteIsValidForVacancyTest(Vacancy vacancy, string route, bool shouldRedirect, string expectedRedirectRoute)
        {
            if (!shouldRedirect)
            {
                Utility.CheckRouteIsValidForVacancy(vacancy, route);
                return;
            }
            
            var ex = Assert.Throws<InvalidRouteForVacancyException>(() => Utility.CheckRouteIsValidForVacancy(vacancy, route));

            ex.RouteNameToRedirectTo.Should().Be(expectedRedirectRoute);
            ex.RouteValues.EmployerAccountId.Should().Be(vacancy.EmployerAccountId);
            ex.RouteValues.VacancyId.Should().Be(vacancy.Id);
        }

    }
}
