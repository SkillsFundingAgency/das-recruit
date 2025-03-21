using Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.UnitTests.Employer.Web.Orchestrators.ManageNotificationsOrchestratorTests
{
    public class GetManageNotificationsViewModelAsyncTests
    {
        private readonly Mock<IRecruitVacancyClient> _recruitVacancyClientMock = new Mock<IRecruitVacancyClient>();
        private readonly Mock<IConfiguration> _iConfigurationMock = new Mock<IConfiguration>();
        public const string EmployerAccountId = "EmployerAccountId";


        [Test]
        public async Task WhenUserPreferencesAreNotSet()
        {
            var employerAccountId = "ABC123";
            _recruitVacancyClientMock.Setup(c => c.GetUserNotificationPreferencesAsync(It.IsAny<string>(),It.IsAny<string>())).ReturnsAsync(new UserNotificationPreferences());
            var sut = GetSut();
            var result = await sut.GetManageNotificationsViewModelAsync(new VacancyUser(), employerAccountId);

            result.EmployerAccountId.Should().Be(employerAccountId);
            result.IsApplicationSubmittedSelected.Should().BeFalse();
            result.IsVacancyClosingSoonSelected.Should().BeFalse();
            result.IsVacancyRejectedSelected.Should().BeFalse();
            result.IsVacancySentForEmployerReviewSelected.Should().BeFalse();
            result.NotificationFrequency.Should().BeNull();
            result.NotificationScope.Should().BeNull();
            result.EnvironmentIsProd.Should().BeTrue();
        }
        
        [Test]
        public async Task WhenUserPreferencesAreNotSet_For_Non_Prod()
        {
            var employerAccountId = "ABC123";
            _recruitVacancyClientMock.Setup(c => c.GetUserNotificationPreferencesAsync(It.IsAny<string>(),It.IsAny<string>())).ReturnsAsync(new UserNotificationPreferences());
            var sut = GetSut(false);
            var result = await sut.GetManageNotificationsViewModelAsync(new VacancyUser(), employerAccountId);

            result.EnvironmentIsProd.Should().BeFalse();
        }


        [TestCase(NotificationTypes.None, false, false, false, false)]
        [TestCase(NotificationTypes.ApplicationSubmitted, true, false, false, false)]
        [TestCase(NotificationTypes.VacancyClosingSoon, false, true, false, false)]
        [TestCase(NotificationTypes.VacancyRejected, false, false, true, false)]
        [TestCase(NotificationTypes.VacancySentForReview, false, false, false, true)]
        [TestCase(NotificationTypes.VacancyRejected | NotificationTypes.VacancyClosingSoon | NotificationTypes.ApplicationSubmitted | NotificationTypes.VacancySentForReview, true, true, true, true)]
        [TestCase(NotificationTypes.VacancyClosingSoon | NotificationTypes.ApplicationSubmitted, true, true, false, false)]
        [TestCase(NotificationTypes.VacancyRejected | NotificationTypes.VacancyClosingSoon, false, true, true, false)]
        [TestCase(NotificationTypes.VacancySentForReview | NotificationTypes.ApplicationSubmitted, true, false, false, true)]
        [TestCase(NotificationTypes.VacancyRejected | NotificationTypes.VacancySentForReview, false, false, true, true)]
        public async Task WhenUserPreferencesAreSet(NotificationTypes notificationTypes, bool expectedIsApplicationSubmittedSelected, 
            bool expectedIsVacancyClosingSoonSelected, bool expectedIsVacancyRejectedSelected, bool expectIsVacancySentForReviewSelected)
        {
            _recruitVacancyClientMock
                .Setup(c => c.GetUserNotificationPreferencesAsync(It.IsAny<string>(),It.IsAny<string>()))
                .ReturnsAsync(new UserNotificationPreferences { NotificationTypes = notificationTypes });
            var sut = GetSut();
            var result = await sut.GetManageNotificationsViewModelAsync(new VacancyUser(), "ABC123");
            result.IsApplicationSubmittedSelected.Should().Be(expectedIsApplicationSubmittedSelected);
            result.IsVacancyClosingSoonSelected.Should().Be(expectedIsVacancyClosingSoonSelected);
            result.IsVacancyRejectedSelected.Should().Be(expectedIsVacancyRejectedSelected);
            result.IsVacancySentForEmployerReviewSelected.Should().Be(expectIsVacancySentForReviewSelected);
            result.EnvironmentIsProd.Should().BeTrue();
        }

        private ManageNotificationsOrchestrator GetSut(bool isProd = true)
        {
            var _loggerMock = new Mock<ILogger<ManageNotificationsOrchestrator>>();
            _iConfigurationMock.Setup(x=>x["Environment"]).Returns(isProd?"Prod":"test");
            return new ManageNotificationsOrchestrator(_loggerMock.Object, new RecruitConfiguration(EmployerAccountId), _iConfigurationMock.Object, _recruitVacancyClientMock.Object);
        }
    }
}