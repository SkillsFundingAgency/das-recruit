using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
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
            var expectedMessage = "0 Vacancies in Live Status with 'nurse'";
            var sut = GetSut(new List<VacancySummary>());
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "Live", 1, User, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Fact]
        public async Task WhenThereIsOneVacancy()
        {
            var expectedMessage = "1 Vacancy in Live Status with 'nurse'";
            var sut = GetSut(GenerateVacancySummaries(1, "nurse", string.Empty, VacancyStatus.Live));
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "Live", 1, User, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Fact]
        public async Task WhenThereIsMoreThanOneVacancy()
        {
            var expectedMessage = "2 Vacancies in Live Status with 'nurse'";
            var sut = GetSut(GenerateVacancySummaries(2, "nurse", string.Empty, VacancyStatus.Live));
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "Live", 1, User, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Theory]
        [InlineData("2 Vacancies in Live Status with 'nurse'", "Live","nurse", 2, VacancyStatus.Live)]
        [InlineData("1 Vacancy in Live Status", "Live", "", 1, VacancyStatus.Live)]
        [InlineData("2 Vacancies in Draft Status with 'nurse'", "Draft", "nurse", 2, VacancyStatus.Draft)]
        [InlineData("2 Vacancies in Pending Review Status with 'nurse'", "Submitted", "nurse", 2, VacancyStatus.Submitted)]
        [InlineData("2 Vacancies in Rejected Status", "Referred", "", 2, VacancyStatus.Referred)]
        [InlineData("2 Vacancies in Closed Status", "Closed", "", 2, VacancyStatus.Closed)]
        [InlineData("2 live vacancies in closing soon Status with 'nurse'", "ClosingSoon", "nurse", 2, VacancyStatus.Live)]
        [InlineData("0 live vacancies in closing soon with no applications Status with 'nurse'", "ClosingSoonWithNoApplications", "nurse", 2, VacancyStatus.Live)]
        [InlineData("2 Vacancies in Transferred Status", "Transferred", "", 2, VacancyStatus.Live)]
        public async Task WhenThereIsMoreThanOneVacancy_WithFilters(string expectedMessage, string filter,string searchTerm,int count,VacancyStatus status)
        {
            var sut = GetSut(GenerateVacancySummaries(count, searchTerm, string.Empty,status));
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, filter, 1, User, searchTerm);
            vm.ResultsHeading.Should().Be(expectedMessage);
        }
    }
}