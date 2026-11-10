using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Helpers;

public class EducationLevelNumberHelperTests
{
    [Test]
    [TestCase(2, "Level 2 (GCSE)")]
    [TestCase(3, "Level 3 (A level)")]
    [TestCase(4, "Level 4 (Higher national certificate)")]
    [TestCase(5, "Level 5 (Higher national diploma)")]
    [TestCase(6, "Level 6 (Degree with honours)")]
    [TestCase(7, "Level 7 (Master's degree)")]
    public void WhenEducationLevelNumberIsNotNull_ShouldReturnCorrectDescription(int level, string expectedDescription)
    {
        string result = EducationLevelNumberHelper.GetEducationLevelNameOrDefault(level, ApprenticeshipLevel.Degree);
        result.Should().Be(expectedDescription);
    }

    [Test]
    [TestCase(ApprenticeshipLevel.Intermediate, "Level 2 (Intermediate)")]
    [TestCase(ApprenticeshipLevel.Advanced, "Level 3 (Advanced)")]
    [TestCase(ApprenticeshipLevel.Higher, "Level 4 (Higher)")]
    [TestCase(ApprenticeshipLevel.Degree, "Level 6 (Degree)")]
    public void WhenEducationLevelNumberIsNull_ShouldReturnLevelName(ApprenticeshipLevel level, string expectedDescription)
    {
        string result =
            EducationLevelNumberHelper.GetEducationLevelNameOrDefault(null, level);
        result.Should().Be(expectedDescription);
    }
}
