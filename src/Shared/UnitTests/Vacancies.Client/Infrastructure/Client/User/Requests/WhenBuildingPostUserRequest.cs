using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.User;
using Esfa.Recruit.Vacancies.Client.Infrastructure.User.Requests;
using NUnit.Framework;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client.User.Requests;

public class WhenBuildingPostUserRequest
{
    [Test, MoqAutoData]
    public void Then_The_Request_Is_Correctly_Built_And_Data_Populated(
        List<long> employerAccountIds,
        Mock<IEncodingService> encodingService,
        Recruit.Vacancies.Client.Domain.Entities.User user)
    {
        // arrange
        encodingService
            .SetupSequence(x => x.Decode(It.IsAny<string>(), EncodingType.AccountId))
            .Returns(employerAccountIds[0])
            .Returns(employerAccountIds[1])
            .Returns(employerAccountIds[2]);
        
        // act
        var actual = new PostUserRequest(user.Id, UserDto.From(user, encodingService.Object));

        // assert
        actual.PostUrl.Should().Be($"users/{user.Id}");
        var userDto = (UserDto)actual.Data;
        userDto.Should().NotBeNull();
        userDto.Should().BeEquivalentTo(user, options => options
                .Excluding(x => x.UserType)
                .Excluding(x => x.Id)
                .Excluding(x => x.EmployerAccountIds)
            );
        userDto.UserType.Should().Be(user.UserType);
        userDto.EmployerAccountIds.Should().BeEquivalentTo(employerAccountIds);
    }
}