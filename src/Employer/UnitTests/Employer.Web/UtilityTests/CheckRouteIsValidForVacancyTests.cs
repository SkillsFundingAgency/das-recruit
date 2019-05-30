using System;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.UtilityTests
{
    public class CheckRouteIsValidForVacancyTests
    {
        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData("any other route", true)]
        public void ShouldRedirectToTitle(string route, bool shouldRedirect)
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
        [InlineData(RouteNames.NumberOfPositions_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Post, false)]
        [InlineData("any other route", true)]
        public void ShouldRedirectToNumberOfPositions(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value"
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.NumberOfPositions_Get);
        }

        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData(RouteNames.NumberOfPositions_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Post, false)]
        [InlineData(RouteNames.ShortDescription_Get, false)]
        [InlineData(RouteNames.ShortDescription_Post, false)]
        [InlineData("any other route", true)]
        public void ShouldRedirectToShortDescription(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                NumberOfPositions = 3
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.ShortDescription_Get);
        }

        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData(RouteNames.NumberOfPositions_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Post, false)]
        [InlineData(RouteNames.ShortDescription_Get, false)]
        [InlineData(RouteNames.ShortDescription_Post, false)]
        [InlineData(RouteNames.Employer_Get, false)]
        [InlineData(RouteNames.Employer_Post, false)]
        [InlineData("any other route", true)]
        public void ShouldRedirectToEmployer(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                NumberOfPositions = 3,
                ShortDescription = "has a value"
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.Employer_Get);
        }

        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData(RouteNames.NumberOfPositions_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Post, false)]
        [InlineData(RouteNames.ShortDescription_Get, false)]
        [InlineData(RouteNames.ShortDescription_Post, false)]
        [InlineData(RouteNames.Employer_Get, false)]
        [InlineData(RouteNames.Employer_Post, false)]
        [InlineData(RouteNames.EmployerName_Get, false)]
        [InlineData(RouteNames.EmployerName_Post, false)]
        [InlineData(RouteNames.Location_Get, false)]
        [InlineData(RouteNames.Location_Post, false)]
        [InlineData(RouteNames.LegalEntityAgreement_SoftStop_Get, false)]
        [InlineData(RouteNames.Training_Get, false)]
        [InlineData(RouteNames.Training_Post, false)]
        [InlineData("any other route", true)]
        public void ShouldRedirectToTraining(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                NumberOfPositions = 3,
                LegalEntityName = "Legal name",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                EmployerLocation = new Address { Postcode = "has a value" },
                ShortDescription = "has a value"
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.Training_Get);
        }

        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData(RouteNames.NumberOfPositions_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Post, false)]
        [InlineData(RouteNames.ShortDescription_Get, false)]
        [InlineData(RouteNames.ShortDescription_Post, false)]
        [InlineData(RouteNames.Employer_Get, false)]
        [InlineData(RouteNames.Employer_Post, false)]
        [InlineData(RouteNames.EmployerName_Get, false)]
        [InlineData(RouteNames.EmployerName_Post, false)]
        [InlineData(RouteNames.Location_Get, false)]
        [InlineData(RouteNames.Location_Post, false)]
        [InlineData(RouteNames.LegalEntityAgreement_SoftStop_Get, false)]
        [InlineData(RouteNames.Training_Get, false)]
        [InlineData(RouteNames.Training_Post, false)]
        [InlineData(RouteNames.Wage_Get, false)]
        [InlineData(RouteNames.Wage_Post, false)]
        [InlineData("any other route", true)]
        public void ShouldRedirectToWage(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                NumberOfPositions = 3,
                LegalEntityName = "legal name",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                EmployerLocation = new Address { Postcode = "has a value" },
                ShortDescription = "has a value",
                ProgrammeId = "has a value"
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.Wage_Get);
        }

        [Theory]
        [InlineData("any other route", false)]
        public void ShouldNotRedirect(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                NumberOfPositions = 3,
                LegalEntityName = "legal name",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                EmployerLocation = new Address { Postcode = "has a value" },
                ShortDescription = "has a value",
                ProgrammeId = "has a value",
                Wage = new Wage { WageType = WageType.FixedWage}
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, null);
        }

        [Fact]
        public void ShouldRedirectToEmployerGet()
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                NumberOfPositions = 3,
                EmployerNameOption = EmployerNameOption.RegisteredName,
                EmployerLocation = new Address { Postcode = "has a value" },
                ShortDescription = "has a value",
                ProgrammeId = "has a value",
                Wage = new Wage { WageType = WageType.FixedWage}
            };

            CheckRouteIsValidForVacancyTest(vacancy, RouteNames.Employer_Get, false, null);
        }

        [Fact]
        public void ShouldRedirectToEmployerNameGet()
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                NumberOfPositions = 3,
                LegalEntityName = "legal name",
                EmployerLocation = new Address { Postcode = "has a value" },
                ShortDescription = "has a value",
                ProgrammeId = "has a value",
                Wage = new Wage { WageType = WageType.FixedWage}
            };

            CheckRouteIsValidForVacancyTest(vacancy, RouteNames.EmployerName_Get, false, null);
        }

        [Fact]
        public void ShouldRedirectToLocationGet()
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                NumberOfPositions = 3,
                LegalEntityName = "legal name",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                ShortDescription = "has a value",
                ProgrammeId = "has a value",
                Wage = new Wage { WageType = WageType.FixedWage}
            };

            CheckRouteIsValidForVacancyTest(vacancy, RouteNames.Location_Get, false, null);
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
