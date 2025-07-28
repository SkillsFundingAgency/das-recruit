using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.User;
using Esfa.Recruit.Vacancies.Client.Infrastructure.User.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client.User.Requests;

public class WhenBuildingPostUserRequest
{
    [Test, AutoData]
    public void Then_The_Request_Is_Correctly_Built_And_Data_Populated(Recruit.Vacancies.Client.Domain.Entities.User user)
    {
        var actual = new PostUserRequest(user.Id, (UserDto)user);

        actual.PostUrl.Should().Be($"users/{user.Id}");
        ((UserDto)actual.Data).Should().BeEquivalentTo(user, options => options
                .Excluding(x => x.UserType)
                .Excluding(x=>x.Id)
            );
        ((UserDto)actual.Data).UserType.Should().Be(user.UserType.ToString());
    }
}