using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.User;
using Esfa.Recruit.Vacancies.Client.Infrastructure.User.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client.User;

public class UserServiceTests
{
    [Test, MoqAutoData]
    public async Task When_Calling_UpsertUserAsync_The_Outer_Api_Is_Called(
        Recruit.Vacancies.Client.Domain.Entities.User user,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        UserService userService)
    {
        outerApiClient.Setup(x => x.Post(It.Is<PostUserRequest>(y => y.PostUrl.Contains(user.Id.ToString())), false));
        
        await userService.UpsertUserAsync(user);
        
        outerApiClient.Verify(x => x.Post(It.Is<PostUserRequest>(y => 
            y.PostUrl.Contains(user.Id.ToString())
            && ((UserDto)y.Data).IdamsUserId == user.IdamsUserId
            ), false),Times.Once());
        
    }
}