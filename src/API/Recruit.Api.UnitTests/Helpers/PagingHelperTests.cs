using SFA.DAS.Recruit.Api.Helpers;

namespace SFA.DAS.Recruit.Api.UnitTests.Helpers
{
    public class PagingHelperTests
    {
        [Test]
        public void WhenPagingGivenAPageSizeAndZeroResults_ShouldReturnZeroPages()
        {
            var resultPage = PagingHelper.GetTotalNoOfPages(pageSize: 10, noOfTotalResults: 0);
            resultPage.Should().Be(0);
        }

        [Test]
        public void WhenPagingGivenAPageSizeAndOneResult_ShouldReturnOnePage()
        {
            var resultPage = PagingHelper.GetTotalNoOfPages(pageSize: 10, noOfTotalResults: 1);
            resultPage.Should().Be(1);
        }

        [TestCase(25, 100, 4)]
        [TestCase(10, 360, 36)]
        [TestCase(10, 365, 37)]
        [TestCase(10, 10, 1)]
        public void WhenPagingGivenAPageSizeAndTotalResultNo_ShouldReturnTotalPages(int pageSize, int noOfResults, int expectedPageNo)
        {
            var resultPage = PagingHelper.GetTotalNoOfPages(pageSize, noOfResults);
            resultPage.Should().Be(expectedPageNo);
        }
    }
}