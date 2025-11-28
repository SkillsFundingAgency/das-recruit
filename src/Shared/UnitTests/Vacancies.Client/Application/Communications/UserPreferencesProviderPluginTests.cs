using Communication.Types;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Xunit;
using static Esfa.Recruit.Vacancies.Client.Application.Communications.CommunicationConstants;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Communications;

public class UserPreferencesProviderPluginTests
{
    private readonly Fixture _fixture = new ();
    private readonly Mock<IUserNotificationPreferencesRepository> _repositoryMock = new();
    private UserPreferencesProviderPlugin GetSut() => new();

    [Theory]
    [InlineData(RequestType.VacancyWithdrawnByQa, DeliveryChannelPreferences.EmailOnly, DeliveryFrequency.Immediate)]
    [InlineData(RequestType.ProviderBlockedProviderNotification, DeliveryChannelPreferences.EmailOnly, DeliveryFrequency.Immediate)]
    [InlineData(RequestType.ProviderBlockedEmployerNotificationForTransferredVacancies, DeliveryChannelPreferences.EmailOnly, DeliveryFrequency.Immediate)]
    [InlineData(RequestType.ProviderBlockedEmployerNotificationForLiveVacancies, DeliveryChannelPreferences.EmailOnly, DeliveryFrequency.Immediate)]
    [InlineData(RequestType.ProviderBlockedEmployerNotificationForPermissionOnly, DeliveryChannelPreferences.EmailOnly, DeliveryFrequency.Immediate)]
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

    [Theory]
    [InlineData(RequestType.VacancyWithdrawnByQa)]
    [InlineData(RequestType.ProviderBlockedProviderNotification)]
    [InlineData(RequestType.ProviderBlockedEmployerNotificationForTransferredVacancies)]
    [InlineData(RequestType.ProviderBlockedEmployerNotificationForLiveVacancies)]
    [InlineData(RequestType.ProviderBlockedEmployerNotificationForPermissionOnly)]
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