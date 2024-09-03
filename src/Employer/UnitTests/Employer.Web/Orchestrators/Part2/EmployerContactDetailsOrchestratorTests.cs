using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part2
{
    public class EmployerContactDetailsOrchestratorTests
    {
        private EmployerContactDetailsOrchestratorTestsFixture _fixture;

        public EmployerContactDetailsOrchestratorTests()
        {
            _fixture = new EmployerContactDetailsOrchestratorTestsFixture();
        }

        [Theory]
        [InlineData("has a new value", "has a value", "has a value")]
        [InlineData("has a value", "has a new value", "has a value")]
        [InlineData("has a value", "has a value", "has a new value")]
        [InlineData("has a new value", "has a new value", "has a new value")]
        public async Task WhenEmployerContactNameIsUpdated__ShouldCallUpdateDraftVacancy(string employerContactName, string employerContactEmail, string employerContactPhone)
        {
            _fixture
                .WithEmployerContactName("has a value")
                .WithEmployerContactEmail("has a value")
                .WithEmployerContactPhone("has a value")
                .Setup();

            var employerContactDetailsEditModel = new EmployerContactDetailsEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                EmployerContactName = employerContactName,
                EmployerContactEmail = employerContactEmail,
                EmployerContactPhone = employerContactPhone
            };

            await _fixture.PostEmployerContactDetailsEditModelAsync(employerContactDetailsEditModel);

            _fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
        }

        [Theory]
        [InlineData("has a new value", "has a value", "has a value")]
        [InlineData("has a value", "has a new value", "has a value")]
        [InlineData("has a value", "has a value", "has a new value")]
        [InlineData("has a new value", "has a new value", "has a new value")]
        public async Task WhenEmployerContactNameIsUpdated_ShouldFlagEmployerContactFieldIndicator(string employerContactName, string employerContactEmail, string employerContactPhone)
        {
            _fixture
                .WithEmployerContactName("has a value")
                .WithEmployerContactEmail("has a value")
                .WithEmployerContactPhone("has a value")
                .Setup();

            var employerContactDetailsEditModel = new EmployerContactDetailsEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                EmployerContactName = employerContactName,
                EmployerContactEmail = employerContactEmail,
                EmployerContactPhone = employerContactPhone
            };

            await _fixture.PostEmployerContactDetailsEditModelAsync(employerContactDetailsEditModel);

            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.EmployerContact, true);
        }

        public class EmployerContactDetailsOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerContactDetails;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public EmployerContactDetailsOrchestrator Sut {get; private set;}

            public EmployerContactDetailsOrchestratorTestsFixture()
            {
                MockClient = new Mock<IEmployerVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public EmployerContactDetailsOrchestratorTestsFixture WithEmployerContactName(string employerContactName)
            {
                if (Vacancy.EmployerContact == null)
                    Vacancy.EmployerContact = new ContactDetail();

                Vacancy.EmployerContact.Name = employerContactName;
                return this;
            }

            public EmployerContactDetailsOrchestratorTestsFixture WithEmployerContactEmail(string employerContactEmail)
            {
                if (Vacancy.EmployerContact == null)
                    Vacancy.EmployerContact = new ContactDetail();

                Vacancy.EmployerContact.Email = employerContactEmail;
                return this;
            }

            public EmployerContactDetailsOrchestratorTestsFixture WithEmployerContactPhone(string employerContactPhone)
            {
                if (Vacancy.EmployerContact == null)
                    Vacancy.EmployerContact = new ContactDetail();

                Vacancy.EmployerContact.Phone = employerContactPhone;
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));
                var utility = new Utility(MockRecruitVacancyClient.Object);
                
                Sut = new EmployerContactDetailsOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<EmployerContactDetailsOrchestrator>>(), Mock.Of<IReviewSummaryService>(), utility);
            }

            public async Task PostEmployerContactDetailsEditModelAsync(EmployerContactDetailsEditModel model)
            {
                await Sut.PostEmployerContactDetailsEditModelAsync(model, User);
            }

            public void VerifyEmployerReviewFieldIndicators(string fieldIdentifier, bool value)
            {
                Vacancy.EmployerReviewFieldIndicators
                    .Where(p => p.FieldIdentifier == fieldIdentifier).FirstOrDefault()
                    .Should().NotBeNull().And
                    .Match<EmployerReviewFieldIndicator>((x) => x.IsChangeRequested == value);
            }

            public void VerifyUpdateDraftVacancyAsyncIsCalled()
            {
                MockRecruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(Vacancy, User), Times.Once);
            }

            public Mock<IEmployerVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
        }
    }
}
