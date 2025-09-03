using System.Threading;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Queries.ManageNotifications.GetProviderNotificationPreferences;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.ManageNotifications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.ManageNotifications;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Queries.ManageNotifications.GetProviderNotificationPreferences;

public class WhenHandlingGetProviderNotificationPreferencesQuery
{
    [Test, MoqAutoData]
    public async Task None_Is_Returned_When_User_Not_Found(
        GetProviderNotificationPreferencesQuery query,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Greedy] GetProviderNotificationPreferencesQueryHandler sut)
    {
        // arrange
        GetUserNotificationPreferencesByDfEUserIdRequest? capturedRequest = null;
        outerApiClient
            .Setup(x => x.Get<GetUserNotificationPreferencesByDfEUserIdResponse>(It.IsAny<GetUserNotificationPreferencesByDfEUserIdRequest>()))
            .Callback<IGetApiRequest>(x => capturedRequest = x as GetUserNotificationPreferencesByDfEUserIdRequest)
            .ReturnsAsync((GetUserNotificationPreferencesByDfEUserIdResponse)null!);
        
        // act
        var result = await sut.Handle(query, CancellationToken.None);

        // assert
        result.Should().Be(GetProviderNotificationPreferencesQueryResult.None);
        capturedRequest.Should().NotBeNull();
        capturedRequest.GetUrl.Should().Be($"managenotifications/provider/{query.DfEUserId}");
    }
    
    [Test, MoqAutoData]
    public async Task Preferences_Are_Returned_When_User_Found(
        GetProviderNotificationPreferencesQuery query,
        GetUserNotificationPreferencesByDfEUserIdResponse response,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Greedy] GetProviderNotificationPreferencesQueryHandler sut)
    {
        // arrange
        outerApiClient
            .Setup(x => x.Get<GetUserNotificationPreferencesByDfEUserIdResponse>(It.IsAny<GetUserNotificationPreferencesByDfEUserIdRequest>()))
            .ReturnsAsync(response);
        
        // act
        var result = await sut.Handle(query, CancellationToken.None);

        // assert
        result.Id.Should().Be(response.Id);
        result.DfEUserId.Should().Be(response.DfEUserId);
        result.NotificationPreferences.Should().Be(response.NotificationPreferences);
    }
}