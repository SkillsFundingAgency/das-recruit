using System;
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

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Communications
{
    public class UserPreferencesProviderPluginTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IUserNotificationPreferencesRepository> _repositoryMock = new Mock<IUserNotificationPreferencesRepository>();

        private UserPreferencesProviderPlugin GetSut() => new UserPreferencesProviderPlugin(_repositoryMock.Object);

        [Theory]
        [InlineData(CommunicationConstants.RequestType.VacancyRejected, DeliveryChannelPreferences.None, DeliveryFrequency.Default)]
        [InlineData(CommunicationConstants.RequestType.ApplicationSubmitted, DeliveryChannelPreferences.None, DeliveryFrequency.Default)]
        [InlineData(CommunicationConstants.RequestType.VacancyWithdrawnByQa, DeliveryChannelPreferences.EmailOnly, DeliveryFrequency.Immediate)]
        [InlineData(CommunicationConstants.RequestType.ProviderBlockedProviderNotification, DeliveryChannelPreferences.EmailOnly, DeliveryFrequency.Immediate)]
        [InlineData(CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForTransferredVacancies, DeliveryChannelPreferences.EmailOnly, DeliveryFrequency.Immediate)]
        [InlineData(CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForLiveVacancies, DeliveryChannelPreferences.EmailOnly, DeliveryFrequency.Immediate)]
        [InlineData(CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForPermissionOnly, DeliveryChannelPreferences.EmailOnly, DeliveryFrequency.Immediate)]
        public async Task WhenUserPreferenceIsNotSet(string requestType, DeliveryChannelPreferences channel, DeliveryFrequency frequency)
        {
            _repositoryMock
                .Setup(u => u.GetAsync(It.IsAny<string>()))
                .ReturnsAsync((UserNotificationPreferences)null);

            var sut = GetSut();

            var user = _fixture.Create<CommunicationUser>();

            var pref = await sut.GetUserPreferenceAsync(requestType, user);

            pref.Channels.Should().Be(channel);
            pref.Frequency.Should().Be(frequency);
        }

        [Fact]
        public async Task WhenRequestTypeIsNotKnown_ShouldReturnNoneChannel()
        {
            var sut = GetSut();

            var user = _fixture.Create<CommunicationUser>();

            await Assert.ThrowsAsync<NotImplementedException>(() =>  sut.GetUserPreferenceAsync("UnknownRequestType", user));
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

        [Theory]
        [InlineData(CommunicationConstants.RequestType.VacancyWithdrawnByQa)]
        [InlineData(CommunicationConstants.RequestType.ProviderBlockedProviderNotification)]
        [InlineData(CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForTransferredVacancies)]
        [InlineData(CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForLiveVacancies)]
        [InlineData(CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForPermissionOnly)]
        public async Task WhenRequestTypeIsOfHighSeverity_ShouldReturnImmediateEmailPreferenceIrrespectiveToUserPreference(string requestType)
        {
            var userPref = _fixture
                .Build<UserNotificationPreferences>()
                .With(p => p.NotificationTypes, NotificationTypes.None)
                .Create();

            _repositoryMock
                .Setup(u => u.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(userPref);

            var sut = GetSut();

            var user = _fixture.Create<CommunicationUser>();

            var pref = await sut.GetUserPreferenceAsync(requestType, user);

            pref.Channels.Should().Be(DeliveryChannelPreferences.EmailOnly);
            pref.Frequency.Should().Be(DeliveryFrequency.Immediate);
        }
    }
}