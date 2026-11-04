using Esfa.Recruit.Shared.Web.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.UnitTests.Shared.Web.Helpers;

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

    [Test]
    [TestCase(25, 100, 4)]
    [TestCase(10, 360, 36)]
    [TestCase(10, 365, 37)]
    [TestCase(10, 10, 1)]
    public void WhenPagingGivenAPageSizeAndTotalResultNo_ShouldReturnTotalPages(int pageSize, int noOfResults, int expectedPageNo)
    {
        var resultPage = PagingHelper.GetTotalNoOfPages(pageSize, noOfResults);
        resultPage.Should().Be(expectedPageNo);
    }

    [Test]
    [TestCase(1, 1)]
    [TestCase(2, 1)]
    [TestCase(3, 1)]
    [TestCase(4, 1)]
    [TestCase(5, 1)]
    [TestCase(6, 2)]
    [TestCase(7, 2)]
    [TestCase(8, 2)]
    [TestCase(9, 2)]
    [TestCase(10, 2)]
    [TestCase(11, 3)]
    [TestCase(12, 3)]
    [TestCase(13, 3)]
    [TestCase(14, 3)]
    [TestCase(15, 3)]
    public void WhenPagingGivenASelectedItem_ShouldReturnPageNoSelectedItemsAppearsOn(int nonZeroBasedIndexOfSelectedItem, int expectedPageNo)
    {
        const int TotalNoOfPages = 3;
        const int PageSize = 5;
        var resultPage = PagingHelper.GetPageNoOfSelectedItem(TotalNoOfPages, PageSize, nonZeroBasedIndexOfSelectedItem);
        resultPage.Should().Be(expectedPageNo);
    }
}
