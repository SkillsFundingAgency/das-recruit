using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Helpers
{
    public class EducationLevelNumberHelperTests
    {
        [Theory]
        [InlineData(2, "Level 2 (GCSE)")]
        [InlineData(3, "Level 3 (A level)")]
        [InlineData(4, "Level 4 (Higher national certificate)")]
        [InlineData(5, "Level 5 (Higher national diploma)")]
        [InlineData(6, "Level 6 (Degree with honours)")]
        [InlineData(7, "Level 7 (Master's degree)")]
        public void WhenEducationLevelNumberIsNotNull_ShouldReturnCorrectDescription(int level, string expectedDescription)
        {
            string result = EducationLevelNumberHelper.GetEducationLevelNameOrDefault(level, ApprenticeshipLevel.Degree);
            result.Should().Be(expectedDescription);
        }

        [Theory]
        [InlineData(ApprenticeshipLevel.Intermediate, "Level 2 (Intermediate)")]
        [InlineData(ApprenticeshipLevel.Advanced, "Level 3 (Advanced)")]
        [InlineData(ApprenticeshipLevel.Higher, "Level 4 (Higher)")]
        [InlineData(ApprenticeshipLevel.Degree, "Level 6 (Degree)")]
        public void WhenEducationLevelNumberIsNull_ShouldReturnLevelName(ApprenticeshipLevel level, string expectedDescription)
        {
            string result =
                EducationLevelNumberHelper.GetEducationLevelNameOrDefault(null, level);
            result.Should().Be(expectedDescription);
        }
    }
}
