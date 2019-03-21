using Esfa.Recruit.Shared.Web.ViewModels;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.ViewModels
{
    public class PagerViewModelTests
    {
        [Fact]
        public void ShouldNotDisplayIfLessThanOnePageOfResults()
        {
            var vm = new PagerViewModel(
                totalItems: 25,
                itemsPerPage: 25,
                currentPage: 1,
                captionFormat: "{0} to {1} of {2}");

            vm.ShowPager.Should().BeFalse();
        }

        [Fact]
        public void ShouldNotDisplayPrevIfOnFirstPage()
        {
            var vm = new PagerViewModel(
                totalItems: 77,
                itemsPerPage: 25,
                currentPage: 1,
                captionFormat: "{0} to {1} of {2}");

            vm.ShowPager.Should().BeTrue();
            vm.ShowPrevious.Should().BeFalse();
            vm.CurrentPage.Should().Be(1);
            vm.Caption.Should().Be("1 to 25 of 77");
            vm.ShowNext.Should().BeTrue();
            vm.NextPage.Should().Be(2);
            vm.TotalPages.Should().Be(4);
        }

        [Fact]
        public void ShouldNotDisplayNextIfOnLastPage()
        {
            var vm = new PagerViewModel(
                totalItems: 77,
                itemsPerPage: 25,
                currentPage: 4,
                captionFormat: "{0} to {1} of {2}");

            vm.ShowPager.Should().BeTrue();
            vm.ShowNext.Should().BeFalse();
            vm.CurrentPage.Should().Be(4);
            vm.Caption.Should().Be("76 to 77 of 77");
            vm.ShowPrevious.Should().BeTrue();
            vm.PreviousPage.Should().Be(3);
            vm.TotalPages.Should().Be(4);
        }

        [Fact]
        public void ShouldDisplayPrevAndNextIfMiddle()
        {
            var vm = new PagerViewModel(
                totalItems: 77,
                itemsPerPage: 25,
                currentPage: 2,
                captionFormat: "{0} to {1} of {2}");

            vm.ShowPager.Should().BeTrue();
            vm.ShowNext.Should().BeTrue();
            vm.NextPage.Should().Be(3);
            vm.CurrentPage.Should().Be(2);
            vm.Caption.Should().Be("26 to 50 of 77");
            vm.ShowPrevious.Should().BeTrue();
            vm.PreviousPage.Should().Be(1);
            vm.TotalPages.Should().Be(4);
        }
    }
}
