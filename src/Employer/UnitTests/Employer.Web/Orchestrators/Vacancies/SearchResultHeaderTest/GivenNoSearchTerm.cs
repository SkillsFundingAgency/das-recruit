using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Employer.Web.Orchestrators.Vacancies.SearchResultHeaderTest
{
    public class GivenNoSearchTerm : SearchResultHeaderTestBase
    {
        [Fact]
        public async Task WhenFilterIsAll_AndThereAreNoVacancies()
        {
            var expectedMessage = "0 vacancies";
            var sut = GetSut(new List<VacancySummary>());
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "All", 1, User, string.Empty);
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Fact]
        public async Task WhenFilterIsAll_AndThereIsOneVacancy()
        {
            var expectedMessage = "1 vacancy";
            var sut = GetSut(GenerateVacancySummaries(1, "", string.Empty));
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "All", 1, User, string.Empty);
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Fact]
        public async Task WhenFilterIsAll_AndThereIsMoreThanOneVacancy()
        {
            var expectedMessage = "2 vacancies";
            var sut = GetSut(GenerateVacancySummaries(2, "", string.Empty));
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "All", 1, User, string.Empty);
            vm.ResultsHeading.Should().Be(expectedMessage);
        }
    }
}