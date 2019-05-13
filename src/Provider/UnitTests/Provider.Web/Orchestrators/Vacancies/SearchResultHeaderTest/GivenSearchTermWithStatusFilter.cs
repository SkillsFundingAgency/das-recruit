using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Provider.Web.Orchestrators.Vacancies.SearchResultHeaderTest
{
    public class GivenSearchTermWithStatusFilter : SearchResultHeaderTestBase
    {
        [Fact]
        public void WhenThereAreNoVacancies()
        {
            var expectedMessage = "0 live vacancies with 'nurse'";
            var sut = GetSut(new List<VacancySummary>());
            var vm = sut.GetVacanciesViewModelAsync(It.IsAny<long>(), "Live", 1, "nurse").Result;
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Fact]
        public void WhenThereIsOneVacancy()
        {
            var expectedMessage = "1 live vacancy with 'nurse'";
            var sut = GetSut(GenerateVacancySummaries(1, "nurse", string.Empty));
            var vm = sut.GetVacanciesViewModelAsync(It.IsAny<long>(), "Live", 1, "nurse").Result;
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Fact]
        public void WhenThereIsMoreThanOneVacancy()
        {
            var expectedMessage = "2 live vacancies with 'nurse'";
            var sut = GetSut(GenerateVacancySummaries(2, "nurse", string.Empty));
            var vm = sut.GetVacanciesViewModelAsync(It.IsAny<long>(), "Live", 1, "nurse").Result;
            vm.ResultsHeading.Should().Be(expectedMessage);
        }
    }
}