using Xunit;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;

namespace Esfa.Recruit.Shared.Web.UnitTests.Extensions
{
    public class EnumExtensionsTests
    {
        [Theory]
        [InlineData(FilteringOptions.ClosingSoon, true)]
        [InlineData(FilteringOptions.ClosingSoonWithNoApplications, true)]
        [InlineData(FilteringOptions.Closed, false)]
        [InlineData(FilteringOptions.Approved, false)]
        [InlineData(FilteringOptions.Referred, false)]
        [InlineData(FilteringOptions.Submitted, false)]
        public void Check_IsInLiveVacancyOptions(FilteringOptions filteringOptions, bool expectedOutput)
        {
            filteringOptions.IsInLiveVacancyOptions().Should().Be(expectedOutput);
        }
    }
}
