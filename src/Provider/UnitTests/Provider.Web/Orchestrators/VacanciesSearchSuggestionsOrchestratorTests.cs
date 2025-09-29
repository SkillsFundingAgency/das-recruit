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
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.UnitTests.Provider.Web.Orchestrators
{
    public class VacanciesSearchSuggestionsOrchestratorTests
    {
        private readonly Mock<IProviderVacancyClient> _mockClient = new Mock<IProviderVacancyClient>();
        const long Ukprn = 1234;

        private readonly VacancySummary[] _testVacancies = {
            new VacancySummary(){Title="The quick brown", LegalEntityName="20th Century Fox", VacancyReference=1000000101},
            new VacancySummary(){Title="fox jumped over", LegalEntityName="20th Century Fox", VacancyReference=1000000102},
            new VacancySummary(){Title="the lazy dog", LegalEntityName="Black & Brown Ltd", CreatedDate=DateTime.Now, VacancyReference=1000000103},
            new VacancySummary(){Title="The quick brown", LegalEntityName="Black & Brown Ltd", VacancyReference=1000000104},
            new VacancySummary(){Title="the lazy fox", LegalEntityName="Black & Brown Ltd", CreatedDate=DateTime.Now.AddDays(-1), VacancyReference=1000000105},
            new VacancySummary(){Title="fox is brown", LegalEntityName=null, VacancyReference=1000000106},
            new VacancySummary(){Title="The quick brown fox", LegalEntityName="Fox 20th Century", VacancyReference=1000000107},
            new VacancySummary(){Title="in this century", LegalEntityName="The quick Brown ltd", VacancyReference=1000000108}
        };

        [Theory]
        [InlineData( "x", true)]
        [InlineData( "xx", true)]
        [InlineData( "xxx", true)]
        [InlineData( "xxxx", true)]
        [InlineData( "xxxxx", false)]
        [InlineData( "xxxxxx", false)]
        public async Task When_The_Search_Term_Is_Less_Than_The_Minimum_Search_Threshold_Then_Empty_List_Returned(string searchTerm, bool shouldBeEmptyList)
        {
            var orch = GetSut(_testVacancies, searchTerm);
            var result = await orch.GetSearchSuggestionsAsync(searchTerm, Ukprn); 
            result.Any().Should().Be(!shouldBeEmptyList);
        }

        [Fact]
        public async Task WhenTermMatchesMoreThan50Title_ThenLegalEntityNameWillBeFilteredOut()
        {
            var LegalEntityName = "20th Century Fox";
            var searchTerm = "century";
            var orch = GetSut(GenerateVacancySummaries(100, LegalEntityName, searchTerm), searchTerm);
            var result = await orch.GetSearchSuggestionsAsync(searchTerm, Ukprn);
            result.Count().Should().Be(50);
            result.Any(s => s.Equals(LegalEntityName)).Should().BeFalse();
        }


        private VacanciesSearchSuggestionsOrchestrator GetSut(IEnumerable<VacancySummary> vacancies, string searchTerm)
        {
            var dashboard = new ProviderDashboard()
            {
                Vacancies = vacancies
            };
            
            _mockClient.Setup(c => c.GetDashboardAsync(Ukprn, "", 1, 25, "", "", null,searchTerm)).ReturnsAsync(dashboard);
            return new VacanciesSearchSuggestionsOrchestrator(_mockClient.Object);
        }

        private IEnumerable<VacancySummary> GenerateVacancySummaries(int count, string legalEntityName, string term)
        {
            return Enumerable.Range(1, count)
                .Select(r => new VacancySummary() 
                { 
                    Title = $"{term} {Guid.NewGuid()}",
                    LegalEntityName = $"{legalEntityName} {Guid.NewGuid()}", VacancyReference = 1000000100 + r,
                    CreatedDate = DateTime.Now.AddSeconds(r) 
                });
        }
    }
}