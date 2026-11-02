using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Domain.Entities;

public class WhenBuildingGetProviderAccountRequest
{
    [Test, AutoData]
    public void Then_The_Url_Is_Correctly_Constructed(int ukprn)
    {
        var actual = new GetProviderStatusDetails(ukprn);

        actual.GetUrl.Should().Be($"provideraccounts/{ukprn}");
    }
}