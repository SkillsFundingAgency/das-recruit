using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Vacancies.SearchResultHeaderTest
{
    public class GiveSearchTermWithNoFilter : SearchResultHeaderTestBase
    {
        [Test]
        public async Task WhenThereAreNoVacancies()
        {
            var expectedMessage = "0 adverts with 'nurse'";
            var sut = GetSut(new List<VacancySummary>(), FilteringOptions.All, "nurses", 0);
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "All", 1, User, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Test]
        public async Task WhenThereIsOneVacancy()
        {
            var expectedMessage = "1 advert with 'nurse'";
            var sut = GetSut(GenerateVacancySummaries(1, "nurse", string.Empty, VacancyStatus.Live), FilteringOptions.All, "nurse", 1);
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "All", 1, User, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }

        [Test]
        public async Task WhenThereIsMoreThanOneVacancy()
        {
            var expectedMessage = "2 adverts with 'nurse'";
            var sut = GetSut(GenerateVacancySummaries(2, "nurse", string.Empty, VacancyStatus.Live), FilteringOptions.All, "nurse", 2);
            var vm = await sut.GetVacanciesViewModelAsync(EmployerAccountId, "All", 1, User, "nurse");
            vm.ResultsHeading.Should().Be(expectedMessage);
        }
    }
}