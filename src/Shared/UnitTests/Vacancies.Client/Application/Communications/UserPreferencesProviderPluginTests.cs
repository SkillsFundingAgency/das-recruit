using System.Threading.Tasks;
using AutoFixture;
using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;
using static Esfa.Recruit.Vacancies.Client.Application.Communications.CommunicationConstants;

namespace UnitTests.Vacancies.Client.Application.Communications
{
    public class UserPreferencesProviderPluginTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IUserNotificationPreferencesRepository> _repositoryMock = new Mock<IUserNotificationPreferencesRepository>();

        private UserPreferencesProviderPlugin GetSut() => new UserPreferencesProviderPlugin(_repositoryMock.Object);

        [Fact]
        public async Task WhenUserPreferenceIsNotSet_ShouldReturnNoneChannel()
        {
            _repositoryMock
                .Setup(u => u.GetAsync(It.IsAny<string>()))
                .ReturnsAsync((UserNotificationPreferences)null);
            
            var sut = GetSut();

            var user = _fixture.Create<CommunicationUser>();

            var pref = await sut.GetUserPreferenceAsync(RequestType.VacancyRejected, user);

            pref.Channels.Should().Be(DeliveryChannelPreferences.None);
        }

        [Fact]
        public async Task WhenRequestTypeIsNotKnown_ShouldReturnNoneChannel()
        {
            var sut = GetSut();

            var user = _fixture.Create<CommunicationUser>();

            var pref = await sut.GetUserPreferenceAsync("UnknownRequestType", user);

            pref.Channels.Should().Be(DeliveryChannelPreferences.None);
        }

        [Fact]
        public async Task WhenRequestTypeIsVacancyRejected_ShouldReturnRespectivePreferences()
        {
            var userPref = _fixture
                .Build<UserNotificationPreferences>()
                .With(p => p.NotificationTypes, NotificationTypes.VacancyRejected)
                .With(p => p.NotificationScope, Esfa.Recruit.Vacancies.Client.Domain.Entities.NotificationScope.OrganisationVacancies)
                .Create();

            _repositoryMock
                .Setup(u => u.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(userPref);

            var sut = GetSut();

            var user = _fixture.Create<CommunicationUser>();

            var pref = await sut.GetUserPreferenceAsync(RequestType.VacancyRejected, user);

            pref.Channels.Should().Be(DeliveryChannelPreferences.EmailOnly);
            pref.Frequency.Should().Be(DeliveryFrequency.Immediate);
            pref.Scope.Should().Be(Communication.Types.NotificationScope.Organisation);
        }

        [Fact]
        public async Task WhenRequestTypeIsApplicationSubmitted_ShouldReturnRespectivePreferences()
        {
            var userPref = _fixture
                .Build<UserNotificationPreferences>()
                .With(p => p.NotificationTypes, NotificationTypes.ApplicationSubmitted)
                .With(p => p.NotificationScope, Esfa.Recruit.Vacancies.Client.Domain.Entities.NotificationScope.UserSubmittedVacancies)
                .Create();

            _repositoryMock
                .Setup(u => u.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(userPref);

            var sut = GetSut();

            var user = _fixture.Create<CommunicationUser>();

            var pref = await sut.GetUserPreferenceAsync(RequestType.ApplicationSubmitted, user);

            pref.Channels.Should().Be(DeliveryChannelPreferences.EmailOnly);
            pref.Frequency.Should().Be(DeliveryFrequency.Immediate);
            pref.Scope.Should().Be(Communication.Types.NotificationScope.Individual);
        }
    }
}