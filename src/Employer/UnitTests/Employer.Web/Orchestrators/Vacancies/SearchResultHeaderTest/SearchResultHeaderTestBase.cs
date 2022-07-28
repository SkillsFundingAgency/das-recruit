using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Moq;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Vacancies.SearchResultHeaderTest
{
    public class SearchResultHeaderTestBase
    {
        protected const string EmployerAccountId = "XXXXXX";
        protected const int ClosingSoonDays = 5;
        protected VacancyUser User;
        protected User UserDetails;
        protected Mock<IRecruitVacancyClient> RecruitVacancyClientMock;
        protected Mock<IEmployerAlertsViewModelFactory> EmployerAlertsViewModelFactoryMock;
        protected Mock<ITimeProvider> TimeProvider;

        protected VacanciesOrchestrator GetSut(IEnumerable<VacancySummary> vacancySummaries)
        {
            var clientMock = new Mock<IEmployerVacancyClient>();
            TimeProvider = new Mock<ITimeProvider>();
            clientMock.Setup(c => c.GetDashboardAsync(EmployerAccountId))
                .ReturnsAsync(new EmployerDashboard {
                    Vacancies = vacancySummaries
                });
            return new VacanciesOrchestrator(
                clientMock.Object,
                TimeProvider.Object,
                RecruitVacancyClientMock.Object,
                EmployerAlertsViewModelFactoryMock.Object);
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
                        TransferInfoTransferredDate = TimeProvider.Object.Today
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

            EmployerAlertsViewModelFactoryMock = new Mock<IEmployerAlertsViewModelFactory>();
        }

    }
}