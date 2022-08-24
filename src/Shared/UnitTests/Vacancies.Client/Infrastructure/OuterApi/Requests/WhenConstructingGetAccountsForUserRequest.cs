using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests
{
    public class WhenConstructingGetAccountsForUserRequest
    {
        [Test, AutoData]
        public void Then_It_Is_Correctly_Constructed(string userId)
        {
            //Arrange
            var actual = new GetUserAccountsRequest(userId);
            
            //Assert
            actual.GetUrl.Should().Be($"users/{userId}/accounts");
        }
    }
}