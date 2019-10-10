using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Employer.Web.Orchestrators.Vacancies.SearchResultHeaderTest
{
    public class GivenSearchTermWithStatusFilter : SearchResultHeaderTestBase
    {
        [Fact]
        public async Task WhenThereAreNoVacancies()
        {
            var expectedMessage = "0 vacancies live with 'nurse'";
            var sut = GetSut(new List<VacancySummary>());
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "Live", 1, User, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Fact]
        public async Task WhenThereIsOneVacancy()
        {
            var expectedMessage = "1 vacancy live with 'nurse'";
            var sut = GetSut(GenerateVacancySummaries(1, "nurse", string.Empty));
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "Live", 1, User, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Fact]
        public async Task WhenThereIsMoreThanOneVacancy()
        {
            var expectedMessage = "2 vacancies live with 'nurse'";
            var sut = GetSut(GenerateVacancySummaries(2, "nurse", string.Empty));
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "Live", 1, User, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }
    }
}