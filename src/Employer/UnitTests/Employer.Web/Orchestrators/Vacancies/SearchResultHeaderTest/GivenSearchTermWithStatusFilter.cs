using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Vacancies.SearchResultHeaderTest
{
    public class GivenSearchTermWithStatusFilter : SearchResultHeaderTestBase
    {
        [Test]
        public async Task WhenThereAreNoVacancies()
        {
            var expectedMessage = "0 live adverts with 'nurse'";
            var sut = GetSut(new List<VacancySummary>(), FilteringOptions.Draft, "nurse", 0);
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "Live", 1, User, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Test]
        public async Task WhenThereIsOneVacancy()
        {
            var expectedMessage = "1 live advert with 'nurse'";
            var sut = GetSut(GenerateVacancySummaries(1, "nurse", string.Empty, VacancyStatus.Live),FilteringOptions.Live, "nurse", 1);
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "Live", 1, User, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Test]
        public async Task WhenThereIsMoreThanOneVacancy()
        {
            var expectedMessage = "2 live adverts with 'nurse'";
            var sut = GetSut(GenerateVacancySummaries(2, "nurse", string.Empty, VacancyStatus.Live), FilteringOptions.Live, "nurse", 2);
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "Live", 1, User, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [TestCase("2 live adverts with 'nurse'", "Live","nurse", 2, VacancyStatus.Live)]
        [TestCase("1 live advert", "Live", "", 1, VacancyStatus.Live)]
        [TestCase("2 draft adverts with 'nurse'", "Draft", "nurse", 2, VacancyStatus.Draft)]
        [TestCase("2 adverts pending review with 'nurse'", "Submitted", "nurse", 2, VacancyStatus.Submitted)]
        [TestCase("2 rejected adverts", "Referred", "", 2, VacancyStatus.Referred)]
        [TestCase("2 closed adverts", "Closed", "", 2, VacancyStatus.Closed)]
        [TestCase("2 adverts closing soon with 'nurse'", "ClosingSoon", "nurse", 2, VacancyStatus.Live)]
        [TestCase("2 adverts transferred from provider", "Transferred", "", 2, VacancyStatus.Live)]
        public async Task WhenThereIsMoreThanOneVacancy_WithFilters(string expectedMessage, string filter,string searchTerm,int count,VacancyStatus status)
        {
            Enum.TryParse<FilteringOptions>(filter, out var filterOption);
            var sut = GetSut(GenerateVacancySummaries(count, searchTerm, string.Empty,status),filterOption,  searchTerm, count);
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, filter, 1, User, searchTerm);
            vm.ResultsHeading.Should().Be(expectedMessage);
        }
    }
}