using NUnit.Framework;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;

namespace Esfa.Recruit.Shared.Web.UnitTests.Extensions;

public class EnumExtensionsTests
{
    [Test]
    [TestCase(FilteringOptions.ClosingSoon, true)]
    [TestCase(FilteringOptions.ClosingSoonWithNoApplications, true)]
    [TestCase(FilteringOptions.Closed, false)]
    [TestCase(FilteringOptions.Approved, false)]
    [TestCase(FilteringOptions.Referred, false)]
    [TestCase(FilteringOptions.Submitted, false)]
    public void Check_IsInLiveVacancyOptions(FilteringOptions filteringOptions, bool expectedOutput)
    {
        filteringOptions.IsInLiveVacancyOptions().Should().Be(expectedOutput);
    }
}
