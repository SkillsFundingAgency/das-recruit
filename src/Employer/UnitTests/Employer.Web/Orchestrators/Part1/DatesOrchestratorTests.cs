using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Dates;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part1
{
    public class DatesOrchestratorTests
    {
        private DatesOrchestratorTestsFixture _fixture;

        public DatesOrchestratorTests()
        {
            _fixture = new DatesOrchestratorTestsFixture();
        }

        [Theory]
        [InlineData("31/12/2021", "01/01/2001", new string[] { FieldIdentifiers.ClosingDate }, new string[] { FieldIdentifiers.PossibleStartDate })]
        [InlineData("01/01/2001", "31/12/2021", new string[] { FieldIdentifiers.PossibleStartDate }, new string[] { FieldIdentifiers.ClosingDate })]
        [InlineData("01/01/2001", "01/01/2001", new string[] {  }, new string[] { FieldIdentifiers.ClosingDate, FieldIdentifiers.PossibleStartDate })]
        [InlineData("31/12/2021", "31/12/2021", new string[] { FieldIdentifiers.ClosingDate, FieldIdentifiers.PossibleStartDate }, new string[] { })]
        public async Task WhenUpdated_ShouldFlagFieldIndicators(string closingDate, string startDate, string[] setFieldIdentifers, string [] unsetFieldIdentifiers)
        {
            _fixture
                .WithClosingDate("01/01/2001")
                .WithStartDate("01/01/2001")
                .Setup();

            var closingDateTime = DateTime.ParseExact(closingDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var startDateTime = DateTime.ParseExact(startDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var datesEditModel = new DatesEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                ClosingDay = closingDateTime.Day.ToString(),
                ClosingMonth = closingDateTime.Month.ToString(),
                ClosingYear = closingDateTime.Year.ToString(),
                StartDay = startDateTime.Day.ToString(),
                StartMonth = startDateTime.Month.ToString(),
                StartYear = startDateTime.Year.ToString()
            };

            await _fixture.PostDatesEditModelAsync(datesEditModel);

            _fixture.VerifyEmployerReviewFieldIndicators(setFieldIdentifers, unsetFieldIdentifiers);
        }

        public class DatesOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.ClosingDate | VacancyRuleSet.StartDate | VacancyRuleSet.StartDateEndDate | VacancyRuleSet.TrainingExpiryDate;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public DatesOrchestrator Sut {get; private set;}

            public DatesOrchestratorTestsFixture()
            {
                MockClient = new Mock<IEmployerVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public DatesOrchestratorTestsFixture WithClosingDate(string closingDate)
            {
                Vacancy.ClosingDate = closingDate.AsDateTimeUk()?.ToUniversalTime();
                return this;
            }

            public DatesOrchestratorTestsFixture WithStartDate(string startDate)
            {
                Vacancy.StartDate = startDate.AsDateTimeUk()?.ToUniversalTime();
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

                var utility = new Utility(MockRecruitVacancyClient.Object);

                Sut = new DatesOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<DatesOrchestrator>>(), 
                    Mock.Of<ITimeProvider>(),Mock.Of<IReviewSummaryService>(), Mock.Of<IApprenticeshipProgrammeProvider>(), utility);
            }

            public async Task PostDatesEditModelAsync(DatesEditModel model)
            {
                await Sut.PostDatesEditModelAsync(model, User);
            }

            public void VerifyEmployerReviewFieldIndicators(string[] setFieldIdentifiers, string[] unsetFieldIdentifiers)
            {
                foreach (var fieldIdentifier in setFieldIdentifiers)
                {
                    VerifyEmployerReviewFieldIndicators(fieldIdentifier, true);
                }

                foreach (var fieldIdentifier in unsetFieldIdentifiers)
                {
                    VerifyEmployerReviewFieldIndicators(fieldIdentifier, false);
                }
            }

            public void VerifyEmployerReviewFieldIndicators(string fieldIdentifier, bool value)
            {
                Vacancy.EmployerReviewFieldIndicators
                    .Where(p => p.FieldIdentifier == fieldIdentifier).Single()
                    .Should().NotBeNull().And
                    .Match<EmployerReviewFieldIndicator>((x) => x.IsChangeRequested == value);
            }

            public Mock<IEmployerVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
        }
    }
}
