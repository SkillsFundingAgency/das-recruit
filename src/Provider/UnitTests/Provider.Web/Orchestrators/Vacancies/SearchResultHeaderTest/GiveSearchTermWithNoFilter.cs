using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.UnitTests.Provider.Web.Orchestrators.Vacancies.SearchResultHeaderTest;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Vacancies.SearchResultHeaderTest
{
    public class GiveSearchTermWithNoFilter : SearchResultHeaderTestBase
    {
        [Fact]
        public async Task WhenThereAreNoVacancies()
        {
            var expectedMessage = "0 vacancies with 'nurse'";
            var sut = GetSut(new List<VacancySummary>(), null,"nurse", 0);
            var vm = await sut.GetVacanciesViewModelAsync(User, "All", 1, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Fact]
        public async Task WhenThereIsOneVacancy()
        {
            var expectedMessage = "1 vacancy with 'nurse'";
            var sut = GetSut(GenerateVacancySummaries(1, "nurse", string.Empty,VacancyStatus.Draft), FilteringOptions.All,"nurse", 1);
            var vm = await sut.GetVacanciesViewModelAsync(User, "All", 1, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Fact]
        public async Task WhenThereIsMoreThanOneVacancy()
        {
            var expectedMessage = "2 vacancies with 'nurse'";
            var sut = GetSut(GenerateVacancySummaries(2, "nurse", string.Empty,VacancyStatus.Draft), FilteringOptions.All,"nurse", 2);
            var vm = await sut.GetVacanciesViewModelAsync(User, "All", 1, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }
    }
}