using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders;

public class WhenMappingFromApiResponseToTrainingProvider
{
    [Test]
    public void Then_The_Fields_Are_Correctly_Mapped()
    {
        var fixture = new Fixture();
        var source = fixture.Create<GetProviderResponseItem>();

        var actual = (TrainingProvider) source;
            
        actual.Should().BeEquivalentTo(source, options => options.Excluding(c=>c.Address).Excluding(c =>c.ProviderTypeId).Excluding(c => c.StatusId));

        actual.Address.AddressLine1.Should().Be(source.Address.Address1);
        actual.Address.AddressLine2.Should().Be(source.Address.Address2);
        actual.Address.AddressLine4.Should().Be(source.Address.Town);
        actual.Address.Postcode.Should().Be(source.Address.Postcode);
    }

    [Test]
    [TestCase("one","","one")]
    [TestCase("one","two","one, two")]
    [TestCase("","two","two")]
    [TestCase("","","")]
    public void Then_The_Address3_Is_Concatanated(string address3, string address4, string expected)
    {
        var fixture = new Fixture();
        var source = fixture.Create<GetProviderResponseItem>();
        source.Address.Address3 = address3;
        source.Address.Address4 = address4;

        var actual = (TrainingProvider) source;

        actual.Address.AddressLine3.Should().Be(expected);
    }
}
