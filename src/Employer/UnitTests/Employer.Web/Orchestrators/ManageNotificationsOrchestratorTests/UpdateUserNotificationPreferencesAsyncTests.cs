using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Employer.Web.Orchestrators.ManageNotificationsOrchestratorTests
{
    public class UpdateUserNotificationPreferencesAsyncTests
    {
        private readonly Mock<IRecruitVacancyClient> _recruitVacancyClientMock = new Mock<IRecruitVacancyClient>();
        
        [Fact]
        public async Task GiveAllTheTypesAreUnselectedAndPersistedPreferencesAreEmpty_ThenReturnValidationError()
        {
            var emptyPreferences = new UserNotificationPreferences() { NotificationTypes = NotificationTypes.None };
            _recruitVacancyClientMock.Setup(c => c.GetUserNotificationPreferencesAsync(It.IsAny<string>())).ReturnsAsync(emptyPreferences);
            var sut = GetSut();
            var result =await sut.UpdateUserNotificationPreferencesAsync(new ManageNotificationsEditModel(), new VacancyUser());
            result.Errors.HasErrors.Should().BeTrue();
            result.Errors.Errors.Count.Should().Be(1);
            result.Errors.Errors.First().ErrorMessage.Should().Be("Select when you want to receive emails about your adverts and applications");
        }
        
        private ManageNotificationsOrchestrator GetSut()
        {
            var _loggerMock = new Mock<ILogger<ManageNotificationsOrchestrator>>();
            return new ManageNotificationsOrchestrator(_loggerMock.Object, _recruitVacancyClientMock.Object);
        }
    }
}