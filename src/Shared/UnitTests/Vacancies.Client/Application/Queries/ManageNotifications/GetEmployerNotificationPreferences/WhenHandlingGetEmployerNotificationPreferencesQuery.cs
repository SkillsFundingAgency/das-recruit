using System.Threading;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Queries.ManageNotifications.GetEmployerNotificationPreferences;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.ManageNotifications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.ManageNotifications;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Queries.ManageNotifications.GetEmployerNotificationPreferences;

public class WhenHandlingGetEmployerNotificationPreferencesQuery
{
    [Test, MoqAutoData]
    public async Task None_Is_Returned_When_User_Not_Found(
        GetEmployerNotificationPreferencesQuery query,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Greedy] GetEmployerNotificationPreferencesQueryHandler sut)
    {
        // arrange
        GetUserNotificationPreferencesByIdamsRequest? capturedRequest = null;
        outerApiClient
            .Setup(x => x.Get<GetUserNotificationPreferencesByIdamsResponse>(It.IsAny<GetUserNotificationPreferencesByIdamsRequest>()))
            .Callback<IGetApiRequest>(x => capturedRequest = x as GetUserNotificationPreferencesByIdamsRequest)
            .ReturnsAsync((GetUserNotificationPreferencesByIdamsResponse)null!);
        
        // act
        var result = await sut.Handle(query, CancellationToken.None);

        // assert
        result.Should().Be(GetEmployerNotificationPreferencesQueryResult.None);
        capturedRequest.Should().NotBeNull();
        capturedRequest.GetUrl.Should().Be($"managenotifications/employer/{query.IdamsId}");
    }
    
    [Test, MoqAutoData]
    public async Task Preferences_Are_Returned_When_User_Found(
        GetEmployerNotificationPreferencesQuery query,
        GetUserNotificationPreferencesByIdamsResponse response,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Greedy] GetEmployerNotificationPreferencesQueryHandler sut)
    {
        // arrange
        outerApiClient
            .Setup(x => x.Get<GetUserNotificationPreferencesByIdamsResponse>(It.IsAny<GetUserNotificationPreferencesByIdamsRequest>()))
            .ReturnsAsync(response);
        
        // act
        var result = await sut.Handle(query, CancellationToken.None);

        // assert
        result.Id.Should().Be(response.Id);
        result.IdamsId.Should().Be(response.IdamsId);
        result.NotificationPreferences.Should().Be(response.NotificationPreferences);
    }
}