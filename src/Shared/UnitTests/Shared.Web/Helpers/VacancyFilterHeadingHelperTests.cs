using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Helpers
{
    public class VacancyFilterHeadingHelperTests
    {
        [Theory]
        [InlineData(0, FilteringOptions.All, "", "0 vacancies")]
        [InlineData(0, FilteringOptions.Live, "", "0 live vacancies")]
        [InlineData(0, FilteringOptions.Referred, "", "0 rejected vacancies")]
        [InlineData(0, FilteringOptions.Submitted, "", "0 vacancies pending review")]
        [InlineData(0, FilteringOptions.Draft, "", "0 draft vacancies")]
        [InlineData(0, FilteringOptions.Closed, "", "0 closed vacancies")]
        [InlineData(1, FilteringOptions.All, "", "1 vacancy")]
        [InlineData(1, FilteringOptions.Live, "", "1 live vacancy")]
        [InlineData(1, FilteringOptions.Referred, "", "1 rejected vacancy")]
        [InlineData(1, FilteringOptions.Submitted, "", "1 vacancy pending review")]
        [InlineData(1, FilteringOptions.Draft, "", "1 draft vacancy")]
        [InlineData(1, FilteringOptions.Closed, "", "1 closed vacancy")]
        [InlineData(6, FilteringOptions.All, "", "6 vacancies")]
        [InlineData(6, FilteringOptions.Live, "", "6 live vacancies")]
        [InlineData(6, FilteringOptions.Referred, "", "6 rejected vacancies")]
        [InlineData(6, FilteringOptions.Submitted, "", "6 vacancies pending review")]
        [InlineData(6, FilteringOptions.Draft, "", "6 draft vacancies")]
        [InlineData(6, FilteringOptions.Closed, "", "6 closed vacancies")]
        [InlineData(0, FilteringOptions.NewApplications, "", "0 vacancies with new applications")]
        [InlineData(0, FilteringOptions.AllApplications, "", "0 vacancies with applications")]
        [InlineData(0, FilteringOptions.ClosingSoonWithNoApplications, "", "0 vacancies closing soon without applications")]
        [InlineData(0, FilteringOptions.ClosingSoon, "", "0 vacancies closing soon")]
        [InlineData(0, FilteringOptions.Transferred, "", "0 vacancies transferred from provider")]
        [InlineData(1, FilteringOptions.NewApplications, "", "1 vacancy with new applications")]
        [InlineData(1, FilteringOptions.AllApplications, "", "1 vacancy with applications")]
        [InlineData(1, FilteringOptions.ClosingSoonWithNoApplications, "", "1 vacancy closing soon without applications")]
        [InlineData(1, FilteringOptions.ClosingSoon, "", "1 vacancy closing soon")]
        [InlineData(1, FilteringOptions.Transferred, "", "1 vacancy transferred from provider")]
        [InlineData(6, FilteringOptions.NewApplications, "", "6 vacancies with new applications")]
        [InlineData(6, FilteringOptions.AllApplications, "", "6 vacancies with applications")]
        [InlineData(6, FilteringOptions.ClosingSoonWithNoApplications, "", "6 vacancies closing soon without applications")]
        [InlineData(6, FilteringOptions.ClosingSoon, "", "6 vacancies closing soon")]
        [InlineData(6, FilteringOptions.Transferred, "", "6 vacancies transferred from provider")]

        [InlineData(0, FilteringOptions.All, "search", "0 vacancies with 'search'")]
        [InlineData(0, FilteringOptions.Live, "search", "0 live vacancies with 'search'")]
        [InlineData(0, FilteringOptions.Referred, "search", "0 rejected vacancies with 'search'")]
        [InlineData(0, FilteringOptions.Submitted, "search", "0 vacancies pending review with 'search'")]
        [InlineData(0, FilteringOptions.Draft, "search", "0 draft vacancies with 'search'")]
        [InlineData(0, FilteringOptions.Closed, "search", "0 closed vacancies with 'search'")]
        [InlineData(1, FilteringOptions.All, "search", "1 vacancy with 'search'")]
        [InlineData(1, FilteringOptions.Live, "search", "1 live vacancy with 'search'")]
        [InlineData(1, FilteringOptions.Referred, "search", "1 rejected vacancy with 'search'")]
        [InlineData(1, FilteringOptions.Submitted, "search", "1 vacancy pending review with 'search'")]
        [InlineData(1, FilteringOptions.Draft, "search", "1 draft vacancy with 'search'")]
        [InlineData(1, FilteringOptions.Closed, "search", "1 closed vacancy with 'search'")]
        [InlineData(6, FilteringOptions.All, "search", "6 vacancies with 'search'")]
        [InlineData(6, FilteringOptions.Live, "search", "6 live vacancies with 'search'")]
        [InlineData(6, FilteringOptions.Referred, "search", "6 rejected vacancies with 'search'")]
        [InlineData(6, FilteringOptions.Submitted, "search", "6 vacancies pending review with 'search'")]
        [InlineData(6, FilteringOptions.Draft, "search", "6 draft vacancies with 'search'")]
        [InlineData(6, FilteringOptions.Closed, "search", "6 closed vacancies with 'search'")]
        [InlineData(0, FilteringOptions.NewApplications, "search", "0 vacancies with new applications with 'search'")]
        [InlineData(0, FilteringOptions.AllApplications, "search", "0 vacancies with applications with 'search'")]
        [InlineData(0, FilteringOptions.ClosingSoonWithNoApplications, "search", "0 vacancies closing soon without applications with 'search'")]
        [InlineData(0, FilteringOptions.ClosingSoon, "search", "0 vacancies closing soon with 'search'")]
        [InlineData(0, FilteringOptions.Transferred, "search", "0 vacancies transferred from provider with 'search'")]
        [InlineData(1, FilteringOptions.NewApplications, "search", "1 vacancy with new applications with 'search'")]
        [InlineData(1, FilteringOptions.AllApplications, "search", "1 vacancy with applications with 'search'")]
        [InlineData(1, FilteringOptions.ClosingSoonWithNoApplications, "search", "1 vacancy closing soon without applications with 'search'")]
        [InlineData(1, FilteringOptions.ClosingSoon, "search", "1 vacancy closing soon with 'search'")]
        [InlineData(1, FilteringOptions.Transferred, "search", "1 vacancy transferred from provider with 'search'")]
        [InlineData(6, FilteringOptions.NewApplications, "search", "6 vacancies with new applications with 'search'")]
        [InlineData(6, FilteringOptions.AllApplications, "search", "6 vacancies with applications with 'search'")]
        [InlineData(6, FilteringOptions.ClosingSoonWithNoApplications, "search", "6 vacancies closing soon without applications with 'search'")]
        [InlineData(6, FilteringOptions.ClosingSoon, "search", "6 vacancies closing soon with 'search'")]
        [InlineData(6, FilteringOptions.Transferred, "search", "6 vacancies transferred from provider with 'search'")]

        public void GetFilterHeading_ShouldBeExpected(int totalVacancies, FilteringOptions filteringOption, string searchTerm, string expectedResult)
        {
            var actualResult = VacancyFilterHeadingHelper.GetFilterHeading(totalVacancies, filteringOption, searchTerm);
                
            actualResult.Should().Be(expectedResult);
        }
    }
}
