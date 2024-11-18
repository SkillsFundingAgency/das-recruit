using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Moq;

namespace Esfa.Recruit.UnitTests.Provider.Web.Orchestrators.Vacancies.SearchResultHeaderTest
{
    public class SearchResultHeaderTestBase
    {
        protected VacancyUser User;
        protected User UserDetails;
        protected Mock<IRecruitVacancyClient> RecruitVacancyClientMock;
        protected Mock<IProviderAlertsViewModelFactory> ProviderAlertsViewModelFactoryMock;
        private Mock<IProviderRelationshipsService> ProviderRelationshipsServiceMock;
        protected Mock<ITimeProvider> TimeProvider;
        protected const int ClosingSoonDays = 5;

        protected VacanciesOrchestrator GetSut(IEnumerable<VacancySummary> vacancySummaries, FilteringOptions? status, string searchTerm, long vacancyCount)
        {
            var clientMock = new Mock<IProviderVacancyClient>();
            TimeProvider = new Mock<ITimeProvider>();
            clientMock.Setup(c => c.GetDashboardAsync(User.Ukprn.Value, VacancyType.Apprenticeship, 1, status, searchTerm))
                .ReturnsAsync(new ProviderDashboard {
                    Vacancies = vacancySummaries
                });
            clientMock.Setup(c => c.GetVacancyCount(User.Ukprn.Value, VacancyType.Apprenticeship, status, searchTerm))
                .ReturnsAsync(vacancyCount);
            return new VacanciesOrchestrator(
                clientMock.Object,
                RecruitVacancyClientMock.Object,
                ProviderAlertsViewModelFactoryMock.Object,
                ProviderRelationshipsServiceMock.Object, new ServiceParameters());
        }

        protected IEnumerable<VacancySummary> GenerateVacancySummaries(int count, string legalEntityName, string term, VacancyStatus status)
        {
            return Enumerable.Range(1, count)
                .Select(r =>
                    new VacancySummary()
                    {
                        Title = $"{term}  {Guid.NewGuid()}",
                        Status = status,
                        LegalEntityName = legalEntityName,
                        VacancyReference = 1000000100 + r,
                        CreatedDate = DateTime.Now,
                        ClosingDate = TimeProvider.Object.Today.AddDays(ClosingSoonDays),
                        NoOfNewApplications = count,
                        NoOfEmployerReviewedApplications = count
                    });
        }

        public SearchResultHeaderTestBase()
        {
            var userId = Guid.NewGuid();
            User = new VacancyUser
            {
                Email = "me@home.com",
                Name = "Keith Chegwin",
                Ukprn = 12345678,
                UserId = userId.ToString()
            };
            UserDetails = new User
            {
                Email = User.Email,
                Name = User.Name,
                Ukprn = User.Ukprn,
                Id = userId
            };

            RecruitVacancyClientMock = new Mock<IRecruitVacancyClient>();
            RecruitVacancyClientMock
                .Setup(x => x.GetUsersDetailsAsync(User.UserId))
                .ReturnsAsync(UserDetails);

            ProviderAlertsViewModelFactoryMock = new Mock<IProviderAlertsViewModelFactory>();
            ProviderRelationshipsServiceMock = new Mock<IProviderRelationshipsService>();
        }

    }
}