using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;

public class WhenMappingFromApiResponseToApprenticeshipProgramme
{
    [Test]
    public void Then_The_Fields_Are_Correctly_Mapped()
    {
        var fixture = new Fixture();
        var source = fixture.Create<GetTrainingProgrammesResponseItem>();

        var actual = (ApprenticeshipProgramme) source;
            
        actual.Should().BeEquivalentTo(source);
    }
}
