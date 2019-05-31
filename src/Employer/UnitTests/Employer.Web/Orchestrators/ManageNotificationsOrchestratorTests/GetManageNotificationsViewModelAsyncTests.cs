using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Employer.Web.Orchestrators.ManageNotificationsOrchestratorTests
{
    public class GetManageNotificationsViewModelAsyncTests
    {
        private readonly Mock<IRecruitVacancyClient> _recruitVacancyClientMock = new Mock<IRecruitVacancyClient>();
        

        [Fact]
        public async Task WhenUserPreferencesAreNotSet()
        {
            _recruitVacancyClientMock.Setup(c => c.GetUserNotificationPreferencesAsync(It.IsAny<string>())).ReturnsAsync((UserNotificationPreferences)null);
            var sut = GetSut();
            var result = await sut.GetManageNotificationsViewModelAsync(new VacancyUser());
            result.IsApplicationSubmittedSelected.Should().BeFalse();
            result.IsVacancyClosingSoonSelected.Should().BeFalse();
            result.IsVacancyRejectedSelected.Should().BeFalse();
            result.NotificationFrequency.Should().BeNull();
            result.NotificationScope.Should().BeNull();
        }

        [Theory]
        [InlineData(NotificationTypes.None, false, false, false)]
        [InlineData(NotificationTypes.ApplicationSubmitted, true, false, false)]
        [InlineData(NotificationTypes.VacancyClosingSoon, false, true, false)]
        [InlineData(NotificationTypes.VacancyRejected, false, false, true)]
        [InlineData(NotificationTypes.VacancyRejected | NotificationTypes.VacancyClosingSoon | NotificationTypes.ApplicationSubmitted, true, true, true)]
        [InlineData(NotificationTypes.VacancyClosingSoon | NotificationTypes.ApplicationSubmitted, true, true, false)]
        [InlineData(NotificationTypes.VacancyRejected | NotificationTypes.VacancyClosingSoon, false, true, true)]
        public async Task WhenUserPreferencesAreSet(NotificationTypes notificationTypes, bool expectedIsApplicationSubmittedSelected, 
            bool expectedIsVacancyClosingSoonSelected, bool expectedIsVacancyRejectedSelected)
        {
            _recruitVacancyClientMock
                .Setup(c => c.GetUserNotificationPreferencesAsync(It.IsAny<string>()))
                .ReturnsAsync(new UserNotificationPreferences { NotificationTypes = notificationTypes });
            var sut = GetSut();
            var result = await sut.GetManageNotificationsViewModelAsync(new VacancyUser());
            result.IsApplicationSubmittedSelected.Should().Be(expectedIsApplicationSubmittedSelected);
            result.IsVacancyClosingSoonSelected.Should().Be(expectedIsVacancyClosingSoonSelected);
            result.IsVacancyRejectedSelected.Should().Be(expectedIsVacancyRejectedSelected);
        }

        private ManageNotificationsOrchestrator GetSut()
        {
            var _loggerMock = new Mock<ILogger<ManageNotificationsOrchestrator>>();
            return new ManageNotificationsOrchestrator(_loggerMock.Object, _recruitVacancyClientMock.Object);
        }
    }
}