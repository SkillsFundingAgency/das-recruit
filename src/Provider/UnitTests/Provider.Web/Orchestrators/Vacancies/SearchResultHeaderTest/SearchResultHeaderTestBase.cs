using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Moq;

namespace Esfa.Recruit.UnitTests.Provider.Web.Orchestrators.Vacancies.SearchResultHeaderTest
{
    public class SearchResultHeaderTestBase
    {
        protected VacanciesOrchestrator GetSut(IEnumerable<VacancySummary> vacancySummaries)
        {
            var clientMock = new Mock<IProviderVacancyClient>();
            var timeProviderMock = new Mock<ITimeProvider>();
            clientMock.Setup(c => c.GetDashboardAsync(It.IsAny<long>()))
                .ReturnsAsync(new ProviderDashboard {
                    Vacancies = vacancySummaries
                });
            return new VacanciesOrchestrator(clientMock.Object, timeProviderMock.Object);
        }

        protected IEnumerable<VacancySummary> GenerateVacancySummaries(int count, string employerName, string term)
        {
            return Enumerable.Range(1, count)
                .Select(r =>
                    new VacancySummary()
                    {
                        Title = $"{term}  {Guid.NewGuid()}",
                        Status = VacancyStatus.Live,
                        EmployerName = employerName,
                        VacancyReference = 1000000100 + r,
                        CreatedDate = DateTime.Now
                    });
        }
    }
}