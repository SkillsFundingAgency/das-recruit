using System;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.UtilityTests
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
        [InlineData(RouteNames.Training_Get, false)]
        [InlineData(RouteNames.Training_Post, false)]
        [InlineData(RouteNames.Training_Confirm_Get, false)]
        [InlineData(RouteNames.Training_Confirm_Post, false)]
        [InlineData("any other route", true)]
        public void ShouldRedirectToTraining(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value"
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.Training_Get);
        }

        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData(RouteNames.Training_Get, false)]
        [InlineData(RouteNames.Training_Post, false)]
        [InlineData(RouteNames.Training_Confirm_Get, false)]
        [InlineData(RouteNames.Training_Confirm_Post, false)]
        [InlineData(RouteNames.NumberOfPositions_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Post, false)]
        [InlineData("any other route", true)]
        public void ShouldRedirectToNumberOfPositions(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                ProgrammeId = "has a value",
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.NumberOfPositions_Get);
        }

        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData(RouteNames.Training_Get, false)]
        [InlineData(RouteNames.Training_Post, false)]
        [InlineData(RouteNames.Training_Confirm_Get, false)]
        [InlineData(RouteNames.Training_Confirm_Post, false)]
        [InlineData(RouteNames.NumberOfPositions_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Post, false)]
        [InlineData(RouteNames.Location_Get, false)]
        [InlineData(RouteNames.Location_Post, false)]
        [InlineData("any other route", true)]
        public void ShouldRedirectToLocation(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy 
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                ProgrammeId = "has a value",
                NumberOfPositions = 1
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.LegalEntity_Get);
        }

        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData(RouteNames.Training_Get, false)]
        [InlineData(RouteNames.Training_Post, false)]
        [InlineData(RouteNames.Training_Confirm_Get, false)]
        [InlineData(RouteNames.Training_Confirm_Post, false)]
        [InlineData(RouteNames.NumberOfPositions_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Post, false)]
        [InlineData(RouteNames.LegalEntity_Get, false)]
        [InlineData(RouteNames.LegalEntity_Post, false)]
        [InlineData(RouteNames.EmployerName_Get, false)]
        [InlineData(RouteNames.EmployerName_Post, false)]
        [InlineData(RouteNames.Location_Get, false)]
        [InlineData(RouteNames.Location_Post, false)]
        [InlineData("any other route", true)]
        public void ShouldRedirectToDates(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                ProgrammeId = "has a value",
                NumberOfPositions = 1,
                EmployerLocation = new Address { Postcode = "has a value" },
                LegalEntityName = "legal name",
                EmployerNameOption = EmployerNameOption.RegisteredName
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.Dates_Get);
        }

        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData(RouteNames.Training_Get, false)]
        [InlineData(RouteNames.Training_Post, false)]
        [InlineData(RouteNames.Training_Confirm_Get, false)]
        [InlineData(RouteNames.Training_Confirm_Post, false)]
        [InlineData(RouteNames.NumberOfPositions_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Post, false)]
        [InlineData(RouteNames.LegalEntity_Get, false)]
        [InlineData(RouteNames.LegalEntity_Post, false)]
        [InlineData(RouteNames.EmployerName_Get, false)]
        [InlineData(RouteNames.EmployerName_Post, false)]
        [InlineData(RouteNames.Location_Get, false)]
        [InlineData(RouteNames.Location_Post, false)]
        [InlineData(RouteNames.Duration_Get, false)]
        [InlineData(RouteNames.Duration_Post, false)]
        [InlineData("any other route", true)]
        public void ShouldRedirectToDuration(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                ProgrammeId = "has a value",
                NumberOfPositions = 1,
                EmployerLocation = new Address { Postcode = "has a value" },
                LegalEntityName = "legal name",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                StartDate = DateTime.Now
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.Duration_Get);
        }

        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData(RouteNames.Training_Get, false)]
        [InlineData(RouteNames.Training_Post, false)]
        [InlineData(RouteNames.Training_Confirm_Get, false)]
        [InlineData(RouteNames.Training_Confirm_Post, false)]
        [InlineData(RouteNames.NumberOfPositions_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Post, false)]
        [InlineData(RouteNames.LegalEntity_Get, false)]
        [InlineData(RouteNames.LegalEntity_Post, false)]
        [InlineData(RouteNames.EmployerName_Get, false)]
        [InlineData(RouteNames.EmployerName_Post, false)]
        [InlineData(RouteNames.Location_Get, false)]
        [InlineData(RouteNames.Location_Post, false)]
        [InlineData(RouteNames.Duration_Get, false)]
        [InlineData(RouteNames.Duration_Post, false)]
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
                ProgrammeId = "has a value",
                NumberOfPositions = 1,
                EmployerLocation = new Address { Postcode = "has a value" },
                LegalEntityName = "legal name",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                StartDate = DateTime.Now,
                Wage = new Wage { Duration = 1}
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
                ProgrammeId = "has a value",
                NumberOfPositions = 1,
                EmployerLocation = new Address { Postcode = "has a value" },
                LegalEntityName = "legal name",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                Wage = new Wage {Duration = 1, WageType = WageType.FixedWage },
                StartDate = DateTime.Now
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, null);
        }

        [Fact]
        public void ShouldRedirectToLegalEntityGet()
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                ProgrammeId = "has a value",
                NumberOfPositions = 1,
                EmployerNameOption = EmployerNameOption.RegisteredName,
                EmployerLocation = new Address { Postcode = "has a value" },
                Wage = new Wage { WageType = WageType.FixedWage}
            };

            CheckRouteIsValidForVacancyTest(vacancy, RouteNames.LegalEntity_Get, false, null);
        }

        private void CheckRouteIsValidForVacancyTest(Vacancy vacancy, string route,
            bool shouldRedirect, string expectedRedirectRoute)
        {
            var utility = new Utility(Mock.Of<IRecruitVacancyClient>());
            
            var vrm = new VacancyRouteModel { Ukprn = 12345678, VacancyId = Guid.NewGuid() };
            if (!shouldRedirect)
            {
                utility.CheckRouteIsValidForVacancy(vacancy, route, vrm);
                return;
            }

            var ex = Assert.Throws<InvalidRouteForVacancyException>(()
                => utility.CheckRouteIsValidForVacancy(vacancy, route, vrm));

            ex.RouteNameToRedirectTo.Should().Be(expectedRedirectRoute);
            ex.RouteValues.Ukprn.Should().Be(vrm.Ukprn);
            ex.RouteValues.VacancyId.Should().Be(vrm.VacancyId);
        }
    }
}