using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Vacancies.SearchResultHeaderTest
{
    public class GivenNoSearchTerm : SearchResultHeaderTestBase
    {
        [Test]
        public async Task WhenFilterIsAll_AndThereAreNoVacancies()
        {
            var expectedMessage = "0 adverts";
            var sut = GetSut(new List<VacancySummary>(), FilteringOptions.All, string.Empty, 0);
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "All", 1, User, string.Empty);
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Test]
        public async Task WhenFilterIsAll_AndThereIsOneVacancy()
        {
            var expectedMessage = "1 advert";
            var sut = GetSut(GenerateVacancySummaries(1, "", string.Empty,VacancyStatus.Live), FilteringOptions.All, string.Empty, 1);
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "All", 1, User, string.Empty);
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Test]
        public async Task WhenFilterIsAll_AndThereIsMoreThanOneVacancy()
        {
            var expectedMessage = "2 adverts";
            var sut = GetSut(GenerateVacancySummaries(2, "", string.Empty, VacancyStatus.Live), FilteringOptions.All, string.Empty, 2);
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "All", 1, User, string.Empty);
            vm.ResultsHeading.Should().Be(expectedMessage);
        }
    }
}