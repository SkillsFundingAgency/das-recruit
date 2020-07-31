using Esfa.Recruit.Shared.Web.Helpers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Helpers
{
    public class VacancyFilterHeadingHelperTests
    {
        [Theory]
        [InlineData(0, FilteringOptions.All, "", "0 adverts")]
        [InlineData(0, FilteringOptions.Live, "", "0 live adverts")]
        [InlineData(0, FilteringOptions.Referred, "", "0 rejected adverts")]
        [InlineData(0, FilteringOptions.Submitted, "", "0 adverts pending review")]
        [InlineData(0, FilteringOptions.Draft, "", "0 draft adverts")]
        [InlineData(0, FilteringOptions.Closed, "", "0 closed adverts")]
        [InlineData(1, FilteringOptions.All, "", "1 advert")]
        [InlineData(1, FilteringOptions.Live, "", "1 live advert")]
        [InlineData(1, FilteringOptions.Referred, "", "1 rejected advert")]
        [InlineData(1, FilteringOptions.Submitted, "", "1 advert pending review")]
        [InlineData(1, FilteringOptions.Draft, "", "1 draft advert")]
        [InlineData(1, FilteringOptions.Closed, "", "1 closed advert")]
        [InlineData(6, FilteringOptions.All, "", "6 adverts")]
        [InlineData(6, FilteringOptions.Live, "", "6 live adverts")]
        [InlineData(6, FilteringOptions.Referred, "", "6 rejected adverts")]
        [InlineData(6, FilteringOptions.Submitted, "", "6 adverts pending review")]
        [InlineData(6, FilteringOptions.Draft, "", "6 draft adverts")]
        [InlineData(6, FilteringOptions.Closed, "", "6 closed adverts")]
        [InlineData(0, FilteringOptions.NewApplications, "", "0 adverts with new applications")]
        [InlineData(0, FilteringOptions.AllApplications, "", "0 adverts with applications")]
        [InlineData(0, FilteringOptions.ClosingSoonWithNoApplications, "", "0 adverts closing soon without applications")]
        [InlineData(0, FilteringOptions.ClosingSoon, "", "0 adverts closing soon")]
        [InlineData(0, FilteringOptions.Transferred, "", "0 adverts transferred from provider")]
        [InlineData(1, FilteringOptions.NewApplications, "", "1 advert with new applications")]
        [InlineData(1, FilteringOptions.AllApplications, "", "1 advert with applications")]
        [InlineData(1, FilteringOptions.ClosingSoonWithNoApplications, "", "1 advert closing soon without applications")]
        [InlineData(1, FilteringOptions.ClosingSoon, "", "1 advert closing soon")]
        [InlineData(1, FilteringOptions.Transferred, "", "1 advert transferred from provider")]
        [InlineData(6, FilteringOptions.NewApplications, "", "6 adverts with new applications")]
        [InlineData(6, FilteringOptions.AllApplications, "", "6 adverts with applications")]
        [InlineData(6, FilteringOptions.ClosingSoonWithNoApplications, "", "6 adverts closing soon without applications")]
        [InlineData(6, FilteringOptions.ClosingSoon, "", "6 adverts closing soon")]
        [InlineData(6, FilteringOptions.Transferred, "", "6 adverts transferred from provider")]

        [InlineData(0, FilteringOptions.All, "search", "0 adverts with 'search'")]
        [InlineData(0, FilteringOptions.Live, "search", "0 live adverts with 'search'")]
        [InlineData(0, FilteringOptions.Referred, "search", "0 rejected adverts with 'search'")]
        [InlineData(0, FilteringOptions.Submitted, "search", "0 adverts pending review with 'search'")]
        [InlineData(0, FilteringOptions.Draft, "search", "0 draft adverts with 'search'")]
        [InlineData(0, FilteringOptions.Closed, "search", "0 closed adverts with 'search'")]
        [InlineData(1, FilteringOptions.All, "search", "1 advert with 'search'")]
        [InlineData(1, FilteringOptions.Live, "search", "1 live advert with 'search'")]
        [InlineData(1, FilteringOptions.Referred, "search", "1 rejected advert with 'search'")]
        [InlineData(1, FilteringOptions.Submitted, "search", "1 advert pending review with 'search'")]
        [InlineData(1, FilteringOptions.Draft, "search", "1 draft advert with 'search'")]
        [InlineData(1, FilteringOptions.Closed, "search", "1 closed advert with 'search'")]
        [InlineData(6, FilteringOptions.All, "search", "6 adverts with 'search'")]
        [InlineData(6, FilteringOptions.Live, "search", "6 live adverts with 'search'")]
        [InlineData(6, FilteringOptions.Referred, "search", "6 rejected adverts with 'search'")]
        [InlineData(6, FilteringOptions.Submitted, "search", "6 adverts pending review with 'search'")]
        [InlineData(6, FilteringOptions.Draft, "search", "6 draft adverts with 'search'")]
        [InlineData(6, FilteringOptions.Closed, "search", "6 closed adverts with 'search'")]
        [InlineData(0, FilteringOptions.NewApplications, "search", "0 adverts with new applications with 'search'")]
        [InlineData(0, FilteringOptions.AllApplications, "search", "0 adverts with applications with 'search'")]
        [InlineData(0, FilteringOptions.ClosingSoonWithNoApplications, "search", "0 adverts closing soon without applications with 'search'")]
        [InlineData(0, FilteringOptions.ClosingSoon, "search", "0 adverts closing soon with 'search'")]
        [InlineData(0, FilteringOptions.Transferred, "search", "0 adverts transferred from provider with 'search'")]
        [InlineData(1, FilteringOptions.NewApplications, "search", "1 advert with new applications with 'search'")]
        [InlineData(1, FilteringOptions.AllApplications, "search", "1 advert with applications with 'search'")]
        [InlineData(1, FilteringOptions.ClosingSoonWithNoApplications, "search", "1 advert closing soon without applications with 'search'")]
        [InlineData(1, FilteringOptions.ClosingSoon, "search", "1 advert closing soon with 'search'")]
        [InlineData(1, FilteringOptions.Transferred, "search", "1 advert transferred from provider with 'search'")]
        [InlineData(6, FilteringOptions.NewApplications, "search", "6 adverts with new applications with 'search'")]
        [InlineData(6, FilteringOptions.AllApplications, "search", "6 adverts with applications with 'search'")]
        [InlineData(6, FilteringOptions.ClosingSoonWithNoApplications, "search", "6 adverts closing soon without applications with 'search'")]
        [InlineData(6, FilteringOptions.ClosingSoon, "search", "6 adverts closing soon with 'search'")]
        [InlineData(6, FilteringOptions.Transferred, "search", "6 adverts transferred from provider with 'search'")]

        public void GetFilterHeading_ShouldBeExpected(int totalVacancies, FilteringOptions filteringOption, string searchTerm, string expectedResult)
        {
            var actualResult = VacancyFilterHeadingHelper.GetFilterHeading(totalVacancies, filteringOption, searchTerm);
                
            actualResult.Should().Be(expectedResult);
        }
    }
}
