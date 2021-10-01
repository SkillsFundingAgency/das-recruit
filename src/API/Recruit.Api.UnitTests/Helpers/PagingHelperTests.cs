using FluentAssertions;
using SFA.DAS.Recruit.Api.Helpers;
using Xunit;

namespace SFA.DAS.Recruit.Api.UnitTests.Helpers
{
    public class PagingHelperTests
    {
        [Fact]
        public void WhenPagingGivenAPageSizeAndZeroResults_ShouldReturnZeroPages()
        {
            var resultPage = PagingHelper.GetTotalNoOfPages(pageSize: 10, noOfTotalResults: 0);
            resultPage.Should().Be(0);
        }

        [Fact]
        public void WhenPagingGivenAPageSizeAndOneResult_ShouldReturnOnePage()
        {
            var resultPage = PagingHelper.GetTotalNoOfPages(pageSize: 10, noOfTotalResults: 1);
            resultPage.Should().Be(1);
        }

        [Theory]
        [InlineData(25, 100, 4)]
        [InlineData(10, 360, 36)]
        [InlineData(10, 365, 37)]
        [InlineData(10, 10, 1)]
        public void WhenPagingGivenAPageSizeAndTotalResultNo_ShouldReturnTotalPages(int pageSize, int noOfResults, int expectedPageNo)
        {
            var resultPage = PagingHelper.GetTotalNoOfPages(pageSize, noOfResults);
            resultPage.Should().Be(expectedPageNo);
        }
    }
}