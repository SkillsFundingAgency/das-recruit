using Esfa.Recruit.Shared.Web.Helpers;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Shared.Web.Helpers
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

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(3, 1)]
        [InlineData(4, 1)]
        [InlineData(5, 1)]
        [InlineData(6, 2)]
        [InlineData(7, 2)]
        [InlineData(8, 2)]
        [InlineData(9, 2)]
        [InlineData(10, 2)]
        [InlineData(11, 3)]
        [InlineData(12, 3)]
        [InlineData(13, 3)]
        [InlineData(14, 3)]
        [InlineData(15, 3)]
        public void WhenPagingGivenASelectedItem_ShouldReturnPageNoSelectedItemsAppearsOn(int nonZeroBasedIndexOfSelectedItem, int expectedPageNo)
        {
            const int TotalNoOfPages = 3;
            const int PageSize = 5;
            var resultPage = PagingHelper.GetPageNoOfSelectedItem(TotalNoOfPages, PageSize, nonZeroBasedIndexOfSelectedItem);
            resultPage.Should().Be(expectedPageNo);
        }
    }
}