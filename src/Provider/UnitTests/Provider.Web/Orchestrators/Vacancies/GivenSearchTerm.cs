using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Provider.Web.Orchestrators.Vacancies
{
    public class GivenSearchTerm
    {
        private VacancySummary[] _testVacancies = new[] 
        {
            new VacancySummary(){Title="The quick brown", EmployerName="20th Century Fox", VacancyReference=1000000101, Status = VacancyStatus.Closed},
            new VacancySummary(){Title="fox jumped over", EmployerName="20th Century Fox", VacancyReference=1000000102, Status = VacancyStatus.Live},
            new VacancySummary(){Title="the lazy dog", EmployerName="Black & Brown Ltd", VacancyReference=1000000103, Status = VacancyStatus.Live},
            new VacancySummary(){Title="The quick brown fox", EmployerName="Black & Brown Ltd", VacancyReference=1000000104, Status = VacancyStatus.Live},
            new VacancySummary(){Title="the lazy fox", EmployerName="Black & Brown Ltd", VacancyReference=1000000105, Status = VacancyStatus.Closed},
            new VacancySummary(){Title="brown dog", EmployerName="Black & Brown Ltd", VacancyReference=2000000105, Status = VacancyStatus.Referred}
        };

        [Theory]
        [InlineData("Closed", "lazy", "1000000105")]
        [InlineData("Live", "fox", "1000000102,1000000104")]
        [InlineData("", "fox", "1000000101,1000000102,1000000104,1000000105")]
        [InlineData("", "105", "2000000105,1000000105")]
        [InlineData("Closed", "VAC100", "1000000101,1000000105")]
        [InlineData("Referred", "", "2000000105")]
        public async Task ThenReturnResults(string status, string searchTerm, string references)
        {
            var expectedReferences = references.Split(',');
            var sut = GetSut(); 
            var result = await sut.GetVacanciesViewModelAsync(1234, status, 1, searchTerm);
            result.Vacancies.Any().Should().BeTrue();
            result.Vacancies.Count.Should().Be(expectedReferences.Count());
            result.Vacancies
                .Select(v => v.VacancyReference.ToString())
                .All(r => expectedReferences.Contains(r))
                .Should().BeTrue();
        }

        [Theory]
        [InlineData("Submitted", "fox")]
        [InlineData("Live","xyz")]
        [InlineData("Submitted","")]
        public async Task ThenReturnNoResults(string status, string searchTerm)
        {
            var sut = GetSut(); 
            var result = await sut.GetVacanciesViewModelAsync(1234, status, 1, searchTerm);
            result.Vacancies.Any().Should().BeFalse();
        }

        private VacanciesOrchestrator GetSut()
        {
            var clientMock = new Mock<IProviderVacancyClient>();
            var timeProviderMock = new Mock<ITimeProvider>();
            clientMock.Setup(c => c.GetDashboardAsync(It.IsAny<long>()))
                .ReturnsAsync(new ProviderDashboard {
                    Vacancies = _testVacancies
                });
            return new VacanciesOrchestrator(clientMock.Object, timeProviderMock.Object);
        }
    }
}