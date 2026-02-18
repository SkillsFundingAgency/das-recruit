using System.Linq;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using MediatR;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.ManageNotificationsOrchestratorTests
{
    public class UpdateUserNotificationPreferencesAsyncTests
    {
        private readonly Mock<IRecruitVacancyClient> _recruitVacancyClientMock = new Mock<IRecruitVacancyClient>();
        
        [Fact]
        public async Task GiveAllTheTypesAreUnselectedAndPersistedPreferencesAreEmpty_ThenReturnValidationError()
        {
            var emptyPreferences = new UserNotificationPreferences { NotificationTypes = NotificationTypes.None };
            _recruitVacancyClientMock.Setup(c => c.GetUserNotificationPreferencesAsync(It.IsAny<string>(),It.IsAny<string>())).ReturnsAsync(emptyPreferences);
            var sut = GetSut();
            var result =await sut.UpdateUserNotificationPreferencesAsync(new ManageNotificationsEditModel(), new VacancyUser());
            result.Errors.HasErrors.Should().BeTrue();
            result.Errors.Errors.Count.Should().Be(1);
            result.Errors.Errors.First().ErrorMessage.Should().Be("Choose when you'd like to receive emails");
        }
        
        private ManageNotificationsOrchestrator GetSut()
        {
            var _loggerMock = new Mock<ILogger<ManageNotificationsOrchestrator>>();
            return new ManageNotificationsOrchestrator(_loggerMock.Object, _recruitVacancyClientMock.Object, Mock.Of<IMediator>());
        }
    }
}