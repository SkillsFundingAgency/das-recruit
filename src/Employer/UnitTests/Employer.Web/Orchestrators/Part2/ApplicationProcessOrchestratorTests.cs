using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part2
{
    public class ApplicationProcessOrchestratorTests
    {
        private readonly Mock<IEmployerVacancyClient> _mockClient;
        private readonly Mock<IRecruitVacancyClient> _mockVacancyClient;

        public ApplicationProcessOrchestratorTests()
        {
            _mockClient = new Mock<IEmployerVacancyClient>();
            _mockVacancyClient = new Mock<IRecruitVacancyClient>();
        }

        [Fact]
        public void WhenApplicationMethodIsThroughFaaVacancy_ShouldOverwriteApplicationUrlAsNull()
        {
            var user = VacancyOrchestratorTestData.GetVacancyUser();
            var vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();

            _mockVacancyClient.Setup(x => x.GetVacancyAsync(vacancy.Id))
                        .ReturnsAsync(vacancy);
            _mockVacancyClient.Setup(x => x.Validate(vacancy, VacancyRuleSet.ApplicationMethod))
                        .Returns(new EntityValidationResult());
            _mockVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), user));

            var sut = new ApplicationProcessOrchestrator(_mockClient.Object, _mockVacancyClient.Object, Options.Create(new ExternalLinksConfiguration()), Mock.Of<ILogger<ApplicationProcessOrchestrator>>(), Mock.Of<IReviewSummaryService>());

            var applicationProcessEditModel = new ApplicationProcessEditModel
            {
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyId = vacancy.Id,
                ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship,
                ApplicationUrl = "www.google.com"
            };

            var result = sut.PostApplicationProcessEditModelAsync(applicationProcessEditModel, user);

            vacancy.ApplicationMethod.HasValue.Should().BeTrue();
            vacancy.ApplicationMethod.Value.Should().Be(ApplicationMethod.ThroughFindAnApprenticeship);
            vacancy.ApplicationUrl.Should().BeNull();
            _mockVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(vacancy, user), Times.Once);
        }

        [Fact]
        public void WhenApplicationMethodIsThroughFaaVacancy_ShouldOverwriteApplicationInstructionsAsNull()
        {
            var user = VacancyOrchestratorTestData.GetVacancyUser();
            var vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();

            _mockVacancyClient.Setup(x => x.GetVacancyAsync(vacancy.Id))
                        .ReturnsAsync(vacancy);
            _mockVacancyClient.Setup(x => x.Validate(vacancy, VacancyRuleSet.ApplicationMethod))
                        .Returns(new EntityValidationResult());
            _mockVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), user));

            var sut = new ApplicationProcessOrchestrator(_mockClient.Object, _mockVacancyClient.Object, Options.Create(new ExternalLinksConfiguration()), Mock.Of<ILogger<ApplicationProcessOrchestrator>>(), Mock.Of<IReviewSummaryService>());

            var applicationProcessEditModel = new ApplicationProcessEditModel
            {
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyId = vacancy.Id,
                ApplicationMethod = ApplicationMethod.ThroughFindAnApprenticeship,
                ApplicationInstructions = "just do it"
            };

            var result = sut.PostApplicationProcessEditModelAsync(applicationProcessEditModel, user);

            vacancy.ApplicationMethod.HasValue.Should().BeTrue();
            vacancy.ApplicationMethod.Value.Should().Be(ApplicationMethod.ThroughFindAnApprenticeship);
            vacancy.ApplicationInstructions.Should().BeNull();
            _mockVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(vacancy, user), Times.Once);
        }
    }
}
