using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using NUnit.Framework;

public class WhenConstructingPostGetAccountLegalEntitiesForAccountRequest
{
    [Test, AutoData]
    public void Then_It_Is_Correctly_Constructed(List<long> accountId)
    {
        //Arrange
        var actual = new PostGetAccountLegalEntitiesRequest(accountId);
            
        //Assert
        actual.PostUrl.Should().Be($"employeraccounts/legalentities");
    }
}