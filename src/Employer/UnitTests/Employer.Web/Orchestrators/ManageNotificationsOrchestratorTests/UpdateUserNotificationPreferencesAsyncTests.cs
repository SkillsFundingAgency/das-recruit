using System.Linq;
using Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UnitTests.Employer.Web.Orchestrators.ManageNotificationsOrchestratorTests
{
    public class UpdateUserNotificationPreferencesAsyncTests
    {
        private readonly Mock<IRecruitVacancyClient> _recruitVacancyClientMock = new Mock<IRecruitVacancyClient>();
        private readonly Mock<IConfiguration> _iConfigurationMock = new Mock<IConfiguration>();
        public const string EmployerAccountId = "EmployerAccountId";

        [Test]
        public async Task GiveAllTheTypesAreUnselectedAndPersistedPreferencesAreEmpty_ThenReturnValidationError()
        {
            var emptyPreferences = new UserNotificationPreferences() { NotificationTypes = NotificationTypes.None };
            _recruitVacancyClientMock.Setup(c => c.GetUserNotificationPreferencesAsync(It.IsAny<string>(),It.IsAny<string>())).ReturnsAsync(emptyPreferences);
            var sut = GetSut();
            var result =await sut.UpdateUserNotificationPreferencesAsync(new ManageNotificationsEditModel(), new VacancyUser());
            result.Errors.HasErrors.Should().BeTrue();
            result.Errors.Errors.Count.Should().Be(1);
            result.Errors.Errors.First().ErrorMessage.Should().Be("Select when you want to receive emails about your adverts and applications");
        }
        
        private ManageNotificationsOrchestrator GetSut()
        {
        
        var _loggerMock = new Mock<ILogger<ManageNotificationsOrchestrator>>();
        
            return new ManageNotificationsOrchestrator(_loggerMock.Object, new RecruitConfiguration(EmployerAccountId), _iConfigurationMock.Object, _recruitVacancyClientMock.Object);
        }
    }
}