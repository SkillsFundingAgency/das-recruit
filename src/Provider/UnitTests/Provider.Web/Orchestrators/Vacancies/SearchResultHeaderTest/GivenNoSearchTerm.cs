using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Provider.Web.Orchestrators.Vacancies.SearchResultHeaderTest
{
    public class GivenNoSearchTerm : SearchResultHeaderTestBase
    {
        [Fact]
        public void WhenFilterIsAll_AndThereAreNoVacancies()
        {
            var expectedMessage = "0 vacancies";
            var sut = GetSut(new List<VacancySummary>());
            var vm = sut.GetVacanciesViewModelAsync(It.IsAny<long>(), "All", 1, string.Empty).Result;
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Fact]
        public void WhenFilterIsAll_AndThereIsOneVacancy()
        {
            var expectedMessage = "1 vacancy";
            var sut = GetSut(GenerateVacancySummaries(1, "", string.Empty));
            var vm = sut.GetVacanciesViewModelAsync(It.IsAny<long>(), "All", 1, string.Empty).Result;
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Fact]
        public void WhenFilterIsAll_AndThereIsMoreThanOneVacancy()
        {
            var expectedMessage = "2 vacancies";
            var sut = GetSut(GenerateVacancySummaries(2, "", string.Empty));
            var vm = sut.GetVacanciesViewModelAsync(It.IsAny<long>(), "All", 1, string.Empty).Result;
            vm.ResultsHeading.Should().Be(expectedMessage);
        }
    }
}