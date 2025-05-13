using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests
{
    public class WhenConstructingGetTrainingProgrammesUrl
    {
        [Fact]
        public void Then_It_Is_Correctly_Constructed()
        {
            //Arrange
            var actual = new GetTrainingProgrammesRequest();
            
            //Assert
            actual.GetUrl.Should().Be("trainingprogrammes?includeFoundationApprenticeships=False");
        }
    }
}