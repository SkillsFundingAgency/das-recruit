using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Shared.Web.ViewModels
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
                captionFormat: "{0} to {1} of {2}",
                routeName: "route name",
                otherRouteValues: null);

            vm.ShowPager.Should().BeFalse();
        }

        [Fact]
        public void ShouldNotDisplayPrevIfOnFirstPage()
        {
            var vm = new PagerViewModel(
                totalItems: 77,
                itemsPerPage: 25,
                currentPage: 1,
                captionFormat: "{0} to {1} of {2}",
                routeName: "route name",
                otherRouteValues: new Dictionary<string, string>{{"filter","Live"}});

            vm.ShowPager.Should().BeTrue();
            vm.ShowPrevious.Should().BeFalse();
            vm.CurrentPage.Should().Be(1);
            vm.Caption.Should().Be("1 to 25 of 77");
            vm.ShowNext.Should().BeTrue();
            vm.TotalPages.Should().Be(4);
            vm.RouteName.Should().Be("route name");

            vm.NextPageRouteData.Count.Should().Be(2);
            vm.NextPageRouteData["filter"].Should().Be("Live");
            vm.NextPageRouteData["page"].Should().Be("2");
        }

        [Fact]
        public void ShouldNotDisplayNextIfOnLastPage()
        {
            var vm = new PagerViewModel(
                totalItems: 77,
                itemsPerPage: 25,
                currentPage: 4,
                captionFormat: "{0} to {1} of {2}",
                routeName: "route name",
                otherRouteValues: new Dictionary<string, string> { { "filter", "Live" } });

            vm.ShowPager.Should().BeTrue();
            vm.ShowNext.Should().BeFalse();
            vm.CurrentPage.Should().Be(4);
            vm.Caption.Should().Be("76 to 77 of 77");
            vm.ShowPrevious.Should().BeTrue();
            vm.TotalPages.Should().Be(4);
            vm.RouteName.Should().Be("route name");

            vm.PreviousPageRouteData.Count.Should().Be(2);
            vm.PreviousPageRouteData["filter"].Should().Be("Live");
            vm.PreviousPageRouteData["page"].Should().Be("3");
        }

        [Fact]
        public void ShouldDisplayPrevAndNextIfMiddle()
        {
            var vm = new PagerViewModel(
                totalItems: 77,
                itemsPerPage: 25,
                currentPage: 2,
                captionFormat: "{0} to {1} of {2}",
                routeName: "route name",
                otherRouteValues: new Dictionary<string, string> { { "filter", "Live" } });

            vm.ShowPager.Should().BeTrue();
            vm.ShowNext.Should().BeTrue();
            vm.CurrentPage.Should().Be(2);
            vm.Caption.Should().Be("26 to 50 of 77");
            vm.ShowPrevious.Should().BeTrue();
            vm.TotalPages.Should().Be(4);
            vm.RouteName.Should().Be("route name");

            vm.NextPageRouteData.Count.Should().Be(2);
            vm.NextPageRouteData["filter"].Should().Be("Live");
            vm.NextPageRouteData["page"].Should().Be("3");

            vm.PreviousPageRouteData.Count.Should().Be(2);
            vm.PreviousPageRouteData["filter"].Should().Be("Live");
            vm.PreviousPageRouteData["page"].Should().Be("1");
        }
    }
}
