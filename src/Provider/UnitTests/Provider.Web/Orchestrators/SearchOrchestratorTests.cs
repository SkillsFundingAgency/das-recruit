using Esfa.Recruit.Provider.Web.Orchestrators;
using Xunit;
using System.Threading.Tasks;
using System.Linq;
using FluentAssertions;
using Moq;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Esfa.Recruit.UnitTests.Provider.Web.Orchestrators
{
    public class SearchOrchestratorTests
    {
        private readonly Mock<IProviderVacancyClient> _mockClient = new Mock<IProviderVacancyClient>();
        const long Ukprn = 1234;

        const string VacancyReferenceRegex = @"^VAC\d{10}$";

        private VacancySummary[] _testVacancies = new[] 
        {
            new VacancySummary(){Title="The quick brown", EmployerName="20th Century Fox", VacancyReference=1000000101},
            new VacancySummary(){Title="fox jumped over", EmployerName="20th Century Fox", VacancyReference=1000000102},
            new VacancySummary(){Title="the lazy dog", EmployerName="Black & Brown Ltd", CreatedDate=DateTime.Now, VacancyReference=1000000103},
            new VacancySummary(){Title="The quick brown", EmployerName="Black & Brown Ltd", VacancyReference=1000000104},
            new VacancySummary(){Title="the lazy fox", EmployerName="Black & Brown Ltd", CreatedDate=DateTime.Now.AddDays(-1), VacancyReference=1000000105},
            new VacancySummary(){Title="fox is brown", EmployerName=null, VacancyReference=1000000106},
            new VacancySummary(){Title="The quick brown fox", EmployerName="Fox 20th Century", VacancyReference=1000000107},
            new VacancySummary(){Title="in this century", EmployerName="The quick Brown ltd", VacancyReference=1000000108}
        };

        [Fact]
        public async Task WhenTermIsTiny_ThenReturnEmptyList()
        {
            var orch = GetSut(_testVacancies);
            var result = await orch.GetAutoCompleteListAsync("x", Ukprn); 
            result.Any().Should().BeFalse();
        }

        [Fact]
        public async Task WhenTermHasNoMatch_ThenReturnEmptyList()
        {
            var orch = GetSut(_testVacancies);
            var result = await orch.GetAutoCompleteListAsync("xxx", Ukprn);
            result.Any().Should().BeFalse();
        }

        [Fact]
        public async Task ShouldIgnoreNullEmployerName()
        {
            var orch = GetSut(_testVacancies);
            var result = await orch.GetAutoCompleteListAsync("fox", Ukprn);
            result.Count().Should().Be(6);
            result.Any(s => s.Equals("fox jumped over")).Should().BeTrue();
            result.Any(s => s.Equals("fox is brown")).Should().BeTrue();
            result.Any(s => s.Equals("Fox 20th Century")).Should().BeTrue();
            result.Any(s => s.Equals("20th Century Fox")).Should().BeTrue();
            result.Any(s => s.Equals("the lazy fox")).Should().BeTrue();
            result.Any(s => s.Equals("The quick brown fox")).Should().BeTrue();
        }

        [Fact]
        public async Task ShouldListLatestOnTop()
        {
            var orch = GetSut(_testVacancies);
            var result = await orch.GetAutoCompleteListAsync("lazy", Ukprn);
            result.Count().Should().Be(2);
            result.First().Should().Contain("the lazy dog");
            result.Last().Should().Contain("the lazy fox");
        }

        [Fact]
        public async Task ShouldMatchTitleAndNameThatContainsTheSearchTerm()
        {
            var orch = GetSut(_testVacancies);
            var result = await orch.GetAutoCompleteListAsync("century", Ukprn);
            result.Count().Should().Be(3);
            result.Any(s => s.Equals("Fox 20th Century")).Should().BeTrue();
            result.Any(s => s.Equals("20th Century Fox")).Should().BeTrue();
            result.Any(s => s.Equals("in this century")).Should().BeTrue();
        }

        [Fact]
        public async Task ShouldReturnDistinctMatchingTitleAndNameList()
        {
            var orch = GetSut(_testVacancies);
            var result = await orch.GetAutoCompleteListAsync("the quick", Ukprn);
            result.Count().Should().Be(3);
            result.Any(s => s.Equals("The quick brown")).Should().BeTrue();
            result.Any(s => s.Equals("The quick brown fox")).Should().BeTrue();
            result.Any(s => s.Equals("The quick Brown ltd")).Should().BeTrue();
        }
        
        [Fact]
        public async Task WhenTermMatchesMoreThan50Title_ThenEmployerNameWillBeFilteredOut()
        {
            var employerName = "20th Century Fox";
            var searchTerm = "fox";
            var orch = GetSut(GenerateVacancySummaries(100, employerName, searchTerm));
            var result = await orch.GetAutoCompleteListAsync(searchTerm, Ukprn);
            result.Count().Should().Be(SearchHelperOrchestrator.MaxRowsInResult);
            result.Any(s => s.Equals(employerName)).Should().BeFalse();
        }

        [Fact]
        public async Task WhenTermMatchesVacancyReference_ThenListVacancyReferences()
        {            
            var employerName = "Exotic Vacations limited";
            var orch = GetSut(GenerateVacancySummaries(100, employerName, "vac"));
            var result = await orch.GetAutoCompleteListAsync("vac1", Ukprn);
            result.Count().Should().Be(SearchHelperOrchestrator.MaxRowsInResult);
            result.Any(s => s.Equals(employerName)).Should().BeFalse();
            result.All(s => Regex.IsMatch(s, VacancyReferenceRegex)).Should().BeTrue();
            result.First().Should().Be("VAC1000000151");
            result.Last().Should().Be("VAC1000000200");
        }

        [Fact]
        public async Task WhenTermMatchesTitleAndName_ThenReturnMaxAllowedRowsOnly()
        {
            var orch = GetSut(GenerateVacancySummaries(20, "vac", "vac"));
            var result = await orch.GetAutoCompleteListAsync("vac", Ukprn);
            result.Count().Should().Be(SearchHelperOrchestrator.MaxRowsInResult);
            result.Count(c => Regex.IsMatch(c, VacancyReferenceRegex)).Should().Be(10);
        }

        private SearchHelperOrchestrator GetSut(IEnumerable<VacancySummary> vacancies)
        {
            var dashboard = new ProviderDashboard()
            {
                Vacancies = vacancies
            };
            
            _mockClient.Setup(c => c.GetDashboardAsync(It.IsAny<long>())).ReturnsAsync(dashboard);
            return new SearchHelperOrchestrator(_mockClient.Object);
        }

        private IEnumerable<VacancySummary> GenerateVacancySummaries(int count, string employerName, string term)
        {
            return Enumerable.Range(1, count)
                .Select(r => new VacancySummary() 
                { 
                    Title = $"{term} {Guid.NewGuid()}",
                    EmployerName = $"{employerName} {Guid.NewGuid()}", VacancyReference = 1000000100 + r,
                    CreatedDate = DateTime.Now.AddSeconds(r) 
                });
        }
    }
}