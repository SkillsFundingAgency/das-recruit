using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Vacancies.SearchResultHeaderTest;

public class GivenSearchTermWithStatusFilter : SearchResultHeaderTestBase
{
    [Test]
    public async Task WhenThereAreNoVacancies()
    {
        var expectedMessage = "0 live vacancies with 'nurse'";
        var sut = GetSut(new List<VacancySummary>(), FilteringOptions.Live, "nurse", 0);
        var vm = await sut.GetVacanciesViewModelAsync(User, "Live", 1, "nurse");
        vm.ResultsHeading.Should().Be(expectedMessage);
    }

    [Test]
    public async Task WhenThereIsOneVacancy()
    {
        var expectedMessage = "1 live vacancy with 'nurse'";
        var sut = GetSut(GenerateVacancySummaries(1, "nurse", string.Empty,VacancyStatus.Live), FilteringOptions.Live, "nurse", 1);
        var vm = await sut.GetVacanciesViewModelAsync(User, "Live", 1, "nurse");
        vm.ResultsHeading.Should().Be(expectedMessage);
    }

    [Test]
    public async Task WhenThereIsMoreThanOneVacancy()
    {
        var expectedMessage = "2 live vacancies with 'nurse'";
        var sut = GetSut(GenerateVacancySummaries(2, "nurse", string.Empty,VacancyStatus.Live), FilteringOptions.Live, "nurse", 2);
        var vm = await sut.GetVacanciesViewModelAsync(User, "Live", 1, "nurse");
        vm.ResultsHeading.Should().Be(expectedMessage);
    }

    [TestCase("2 live vacancies with 'nurse'", FilteringOptions.Live, "nurse", 2, VacancyStatus.Live)]
    [TestCase("1 live vacancy", FilteringOptions.Live, "", 1, VacancyStatus.Live)]
    [TestCase("2 draft vacancies with 'nurse'", FilteringOptions.Draft, "nurse", 2, VacancyStatus.Draft)]
    [TestCase("2 vacancies pending dfe review with 'nurse'", FilteringOptions.Submitted, "nurse", 2, VacancyStatus.Submitted)]
    [TestCase("2 rejected vacancies", FilteringOptions.Referred, "", 2, VacancyStatus.Referred)]
    [TestCase("2 closed vacancies", FilteringOptions.Closed, "", 2, VacancyStatus.Closed)]
    [TestCase("2 vacancies closing soon with 'nurse'", FilteringOptions.ClosingSoon, "nurse", 2, VacancyStatus.Live)]
    [TestCase("0 vacancies closing soon without applications with 'nurse'", FilteringOptions.ClosingSoonWithNoApplications, "nurse", 0, VacancyStatus.Live)]
    [TestCase("2 vacancies with employer-reviewed applications with 'nurse'", FilteringOptions.EmployerReviewedApplications, "nurse", 2, VacancyStatus.Live)]
    public async Task WhenThereIsMoreThanOneVacancy_WithFilters(string expectedMessage, FilteringOptions filter, string searchTerm, int count, VacancyStatus status)
    {
        var sut = GetSut(GenerateVacancySummaries(count, searchTerm, searchTerm, status), filter, searchTerm, count);
        var vm = await sut.GetVacanciesViewModelAsync(User, filter.ToString(), 1, searchTerm);
        vm.ResultsHeading.Should().Be(expectedMessage);
    }
}