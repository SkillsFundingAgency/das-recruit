using System;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.UtilityTests
{
    public class CheckRouteIsValidForVacancyTests
    {
        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData("any other route", true)]
        public void ShouldRedirectToEmployerName(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e")
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.EmployerName_Get);
        }

        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData(RouteNames.Training_Get, false)]
        [InlineData(RouteNames.Training_Post, false)]
        [InlineData(RouteNames.Training_Confirm_Get, false)]
        [InlineData(RouteNames.Training_Confirm_Post, false)]
        [InlineData(RouteNames.Training_First_Time_Get, false)]
        [InlineData(RouteNames.Training_First_Time_Post, false)]
        [InlineData(RouteNames.Training_Help_Get, false)]
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
        [InlineData(RouteNames.Training_First_Time_Get, false)]
        [InlineData(RouteNames.Training_First_Time_Post, false)]
        [InlineData(RouteNames.Training_Help_Get, false)]
        [InlineData(RouteNames.TrainingProvider_Select_Get, false)]
        [InlineData(RouteNames.TrainingProvider_Select_Post, false)]
        [InlineData(RouteNames.TrainingProvider_Confirm_Get, false)]
        [InlineData(RouteNames.TrainingProvider_Confirm_Post, false)]
        [InlineData(RouteNames.NumberOfPositions_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Post, false)]
        [InlineData("any other route", true)]
        public void And_No_NumberOfPositions_ShouldRedirectToEmployer(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                ProgrammeId = "has a value",
                TrainingProvider = null,
                NumberOfPositions = null
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.Employer_Get);
        }

        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData(RouteNames.Training_Get, false)]
        [InlineData(RouteNames.Training_Post, false)]
        [InlineData(RouteNames.Training_Confirm_Get, false)]
        [InlineData(RouteNames.Training_Confirm_Post, false)]
        [InlineData(RouteNames.Training_First_Time_Get, false)]
        [InlineData(RouteNames.Training_First_Time_Post, false)]
        [InlineData(RouteNames.Training_Help_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Post, false)]
        [InlineData(RouteNames.Employer_Get, false)]
        [InlineData(RouteNames.Employer_Post, false)]
        [InlineData(RouteNames.SetCompetitivePayRate_Get, false)]
        [InlineData(RouteNames.SetCompetitivePayRate_Post, false)]
        [InlineData("any other route", true)]
        public void And_Has_NumberOfPositions_ShouldRedirectToEmployer(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                ProgrammeId = "has a value",
                NumberOfPositions = 3
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.Employer_Get);
        }

        [Theory]
        [InlineData(RouteNames.ShortDescription_Get, false, true)]
        [InlineData(RouteNames.ShortDescription_Post, false, true)]
        [InlineData(RouteNames.VacancyDescription_Index_Get, false, true)]
        [InlineData(RouteNames.VacancyDescription_Index_Post, false, true)]
        [InlineData(RouteNames.Dates_Post, false, true)]
        [InlineData(RouteNames.Dates_Get, false, true)]
        [InlineData(RouteNames.Duration_Post, false, true)]
        [InlineData(RouteNames.Duration_Get, false, true)]
        [InlineData(RouteNames.Wage_Post, false, true)]
        [InlineData(RouteNames.Wage_Get, false, true)]
        [InlineData(RouteNames.Skills_Post, false, true)]
        [InlineData(RouteNames.Skills_Get, false, true)]
        [InlineData(RouteNames.Qualification_Add_Post, false, true)]
        [InlineData(RouteNames.Qualification_Delete_Post, false, true)]
        [InlineData(RouteNames.Qualification_Edit_Post, false, true)]
        [InlineData(RouteNames.Qualification_Edit_Get, false, true)]
        [InlineData(RouteNames.Qualification_Add_Get, false, true)]
        [InlineData(RouteNames.Qualifications_Get, false, true)]
        [InlineData(RouteNames.Considerations_Post, false, true)]
        [InlineData(RouteNames.Considerations_Get, false, true)]
        [InlineData(RouteNames.EmployerName_Post, false, true)]
        [InlineData(RouteNames.EmployerName_Get, false, true)]
        [InlineData(RouteNames.AboutEmployer_Post, false, true)]
        [InlineData(RouteNames.AboutEmployer_Get, false, true)]
        [InlineData(RouteNames.EmployerContactDetails_Post, false, true)]
        [InlineData(RouteNames.EmployerContactDetails_Get, false, true)]
        [InlineData(RouteNames.ApplicationProcess_Post, false, true)]
        [InlineData(RouteNames.ApplicationProcess_Get, false, true)]
        [InlineData(RouteNames.AdditionalQuestions_Get, false, true)]
        [InlineData(RouteNames.AdditionalQuestions_Post, false, true)]
        [InlineData(RouteNames.MultipleLocations_Get, false, true)]
        [InlineData(RouteNames.MultipleLocations_Post, false, true)]
        [InlineData("any other route", true)]
        public void ShouldRedirectEmployer(string route, bool shouldRedirect, bool enableTaskList = false)
        {
            var vacancy = new Vacancy {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                ProgrammeId = "has a value",
                TrainingProvider = new TrainingProvider(),
                NumberOfPositions = null,
            };

            CheckRouteIsValidForVacancyTest(vacancy, route, shouldRedirect, RouteNames.Employer_Get, enableTaskList);
        }

        [Theory]
        [InlineData(RouteNames.Title_Get, false)]
        [InlineData(RouteNames.Title_Post, false)]
        [InlineData(RouteNames.Training_Get, false)]
        [InlineData(RouteNames.Training_Post, false)]
        [InlineData(RouteNames.Training_Confirm_Get, false)]
        [InlineData(RouteNames.Training_Confirm_Post, false)]
        [InlineData(RouteNames.Training_First_Time_Get, false)]
        [InlineData(RouteNames.Training_First_Time_Post, false)]
        [InlineData(RouteNames.Training_Help_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Post, false)]
        [InlineData(RouteNames.Employer_Get, false)]
        [InlineData(RouteNames.Employer_Post, false)]
        [InlineData(RouteNames.EmployerName_Get, false)]
        [InlineData(RouteNames.EmployerName_Post, false)]
        [InlineData(RouteNames.Location_Get, false)]
        [InlineData(RouteNames.Location_Post, false)]
        [InlineData(RouteNames.MultipleLocations_Get, false)]
        [InlineData(RouteNames.MultipleLocations_Post, false)]
        [InlineData(RouteNames.LegalEntityAgreement_SoftStop_Get, false)]
        [InlineData("any other route", true)]
        public void ShouldRedirectToDates(string route, bool shouldRedirect)
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                ProgrammeId = "has a value",
                NumberOfPositions = 3,
                LegalEntityName = "legal name",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                EmployerLocation = new Address { Postcode = "has a value" }
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
        [InlineData(RouteNames.Training_First_Time_Get, false)]
        [InlineData(RouteNames.Training_First_Time_Post, false)]
        [InlineData(RouteNames.Training_Help_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Post, false)]
        [InlineData(RouteNames.Employer_Get, false)]
        [InlineData(RouteNames.Employer_Post, false)]
        [InlineData(RouteNames.EmployerName_Get, false)]
        [InlineData(RouteNames.EmployerName_Post, false)]
        [InlineData(RouteNames.Location_Get, false)]
        [InlineData(RouteNames.Location_Post, false)]
        [InlineData(RouteNames.MultipleLocations_Get, false)]
        [InlineData(RouteNames.MultipleLocations_Post, false)]
        [InlineData(RouteNames.LegalEntityAgreement_SoftStop_Get, false)]
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
                NumberOfPositions = 3,
                LegalEntityName = "legal name",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                EmployerLocation = new Address { Postcode = "has a value" },
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
        [InlineData(RouteNames.Training_First_Time_Get, false)]
        [InlineData(RouteNames.Training_First_Time_Post, false)]
        [InlineData(RouteNames.Training_Help_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Get, false)]
        [InlineData(RouteNames.NumberOfPositions_Post, false)]
        [InlineData(RouteNames.Employer_Get, false)]
        [InlineData(RouteNames.Employer_Post, false)]
        [InlineData(RouteNames.EmployerName_Get, false)]
        [InlineData(RouteNames.EmployerName_Post, false)]
        [InlineData(RouteNames.Location_Get, false)]
        [InlineData(RouteNames.Location_Post, false)]
        [InlineData(RouteNames.MultipleLocations_Get, false)]
        [InlineData(RouteNames.MultipleLocations_Post, false)]
        [InlineData(RouteNames.LegalEntityAgreement_SoftStop_Get, false)]
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
                NumberOfPositions = 3,
                LegalEntityName = "legal name",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                EmployerLocation = new Address { Postcode = "has a value" },
                StartDate = DateTime.Now,
                Wage = new Wage { Duration = 1 }
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
                NumberOfPositions = 3,
                LegalEntityName = "legal name",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                EmployerLocation = new Address { Postcode = "has a value" },
                StartDate = DateTime.Now,
                Wage = new Wage
                {
                    Duration = 1,
                    WageType = WageType.FixedWage
                }
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
                ProgrammeId = "has a value",
                NumberOfPositions = 3,
                EmployerNameOption = EmployerNameOption.RegisteredName,
                EmployerLocation = new Address { Postcode = "has a value" },
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
                ProgrammeId = "has a value",
                NumberOfPositions = 3,
                LegalEntityName = "legal name",
                EmployerLocation = new Address { Postcode = "has a value" },
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
                ProgrammeId = "has a value",
                NumberOfPositions = 3,
                LegalEntityName = "legal name",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                Wage = new Wage { WageType = WageType.FixedWage}
            };

            CheckRouteIsValidForVacancyTest(vacancy, RouteNames.Location_Get, false, null);
        }

        [Fact]
        public void ShouldShowTaskList()
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e")
            };

            CheckRouteIsValidForVacancyTest(vacancy, RouteNames.EmployerTaskListGet, false, null, true);
        }
        
        [Fact]
        public void ShouldShowCheckYourAnswers()
        {
            var vacancy = new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e")
            };

            CheckRouteIsValidForVacancyTest(vacancy, RouteNames.EmployerCheckYourAnswersGet, false, null, true);
        }
        
        private void CheckRouteIsValidForVacancyTest(Vacancy vacancy, string route, bool shouldRedirect, string expectedRedirectRoute, bool enableTaskList = false)
        {
            var utility = new Utility(Mock.Of<IRecruitVacancyClient>());
            if (!shouldRedirect)
            {
                utility.CheckRouteIsValidForVacancy(vacancy, route);
                return;
            }
            
            var ex = Assert.Throws<InvalidRouteForVacancyException>(() => utility.CheckRouteIsValidForVacancy(vacancy, route));

            ex.RouteNameToRedirectTo.Should().Be(expectedRedirectRoute);
            ex.RouteValues.EmployerAccountId.Should().Be(vacancy.EmployerAccountId);
            ex.RouteValues.VacancyId.Should().Be(vacancy.Id);
        }

    }
}
