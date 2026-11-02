using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class WhenConstructingGetAccountLegalEntitiesForAccountRequest
{
    [Test, AutoData]
    public void Then_It_Is_Correctly_Constructed(long accountId)
    {
        //Arrange
        var actual = new GetAccountLegalEntitiesRequest(accountId);
            
        //Assert
        actual.GetUrl.Should().Be($"employeraccounts/{accountId}/legalentities");
    }
}