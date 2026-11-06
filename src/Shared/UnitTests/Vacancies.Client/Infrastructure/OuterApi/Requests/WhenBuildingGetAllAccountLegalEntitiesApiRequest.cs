using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests;

[TestFixture]
public class WhenBuildingGetAllAccountLegalEntitiesApiRequest
{
    [Test, MoqAutoData]
    public void Then_It_Is_Correctly_Constructed(
        GetAllAccountLegalEntitiesApiRequest.GetAllAccountLegalEntitiesApiRequestData payload)
    {
        //Arrange
        var expectedUrl = "accountlegalentities/getalllegalentities";
            
        //Act
        var actual = new GetAllAccountLegalEntitiesApiRequest(payload);
            
        //Assert
        actual.PostUrl.Should().Be(expectedUrl);
        actual.Data.Should().Be(payload);
    }
}