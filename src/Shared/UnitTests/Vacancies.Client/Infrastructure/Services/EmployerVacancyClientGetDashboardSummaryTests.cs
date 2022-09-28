using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class EmployerVacancyClientGetDashboardSummaryTests
    {
        [Test, MoqAutoData]
        public async Task Then_Maps_Statuses_To_ProviderDashboardSummary(
            int closedCount,
            int draftCount,
            int reviewCount,
            int referredCount,
            int liveCount,
            int submittedCount,
            int numberOfNewApplications,
            int numberOfUnsuccessfulApplications,
            int numberOfSuccessfulApplications,
            int closedSuccessfulApplications,
            int closedUnsuccessfulApplications,
            int closingSoon,
            int closingSoonNoApplications,
            int rejectedCount,
            string employerAccountId,
            VacancyType vacancyType,
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            VacancyClient vacancyClient)
        {
            var vacancyApplicationsDashboard = new List<VacancyApplicationsDashboard>
            {
                new VacancyApplicationsDashboard { Status = VacancyStatus.Closed, StatusCount = closedCount, NoOfNewApplications = numberOfNewApplications, NoOfSuccessfulApplications = closedSuccessfulApplications, NoOfUnsuccessfulApplications = closedUnsuccessfulApplications},
                new VacancyApplicationsDashboard { Status = VacancyStatus.Closed, StatusCount = closedCount, NoOfSuccessfulApplications = closedSuccessfulApplications, NoOfUnsuccessfulApplications = closedUnsuccessfulApplications},
                new VacancyApplicationsDashboard { Status = VacancyStatus.Live,ClosingSoon = false, StatusCount = liveCount, NoOfNewApplications = numberOfNewApplications, NoOfSuccessfulApplications = numberOfSuccessfulApplications,NoOfUnsuccessfulApplications = numberOfUnsuccessfulApplications},
                new VacancyApplicationsDashboard { Status = VacancyStatus.Live,ClosingSoon = false, StatusCount = liveCount, NoOfNewApplications = numberOfNewApplications, NoOfSuccessfulApplications = numberOfSuccessfulApplications,NoOfUnsuccessfulApplications = numberOfUnsuccessfulApplications},
                new VacancyApplicationsDashboard { Status = VacancyStatus.Live,ClosingSoon = true, StatusCount = closingSoon, NoOfNewApplications = numberOfNewApplications},
                new VacancyApplicationsDashboard { Status = VacancyStatus.Live,ClosingSoon = true, StatusCount = closingSoonNoApplications},
            };
            
            var vacancyDashboards =new List<VacancyStatusDashboard>
            {
                new VacancyStatusDashboard { Status = VacancyStatus.Closed, StatusCount = closedCount},
                new VacancyStatusDashboard { Status = VacancyStatus.Closed, StatusCount = closedCount},
                new VacancyStatusDashboard { Status = VacancyStatus.Draft, StatusCount = draftCount },
                new VacancyStatusDashboard { Status = VacancyStatus.Review, StatusCount = reviewCount },
                new VacancyStatusDashboard { Status = VacancyStatus.Referred, StatusCount = referredCount },
                new VacancyStatusDashboard { Status = VacancyStatus.Rejected, StatusCount = rejectedCount },
                new VacancyStatusDashboard { Status = VacancyStatus.Submitted, StatusCount = submittedCount },
                new VacancyStatusDashboard { Status = VacancyStatus.Live,ClosingSoon = false, StatusCount = liveCount},
                new VacancyStatusDashboard { Status = VacancyStatus.Live,ClosingSoon = false, StatusCount = liveCount},
                new VacancyStatusDashboard { Status = VacancyStatus.Live,ClosingSoon = true, StatusCount = closingSoon},
                new VacancyStatusDashboard { Status = VacancyStatus.Live,ClosingSoon = true, StatusCount = closingSoonNoApplications },
            };
            vacanciesSummaryProvider.Setup(x => x.GetEmployerOwnedVacancyDashboardByEmployerAccountIdAsync(employerAccountId, vacancyType)).ReturnsAsync(new VacancyDashboard
            {
                VacancyApplicationsDashboard = vacancyApplicationsDashboard,
                VacancyStatusDashboard = vacancyDashboards
            });
            
            var actual = await vacancyClient.GetDashboardSummary(employerAccountId);

            actual.Closed.Should().Be(closedCount);
            actual.Draft.Should().Be(draftCount);
            actual.Review.Should().Be(reviewCount);
            actual.Referred.Should().Be(referredCount + rejectedCount);
            actual.Live.Should().Be(liveCount*2 + closingSoon + closingSoonNoApplications);
            actual.NumberOfNewApplications.Should().Be(numberOfNewApplications*4);
            actual.NumberOfUnsuccessfulApplications.Should().Be(numberOfUnsuccessfulApplications*2 + closedUnsuccessfulApplications*2);
            actual.NumberOfSuccessfulApplications.Should().Be(numberOfSuccessfulApplications*2 + closedSuccessfulApplications*2);
            actual.NumberClosingSoon.Should().Be(closingSoon);
            actual.NumberClosingSoonWithNoApplications.Should().Be(closingSoonNoApplications);

            actual.HasVacancies.Should().BeTrue();
            actual.HasOneVacancy.Should().BeFalse();
            actual.HasApplications.Should().BeTrue();
            actual.NumberOfVacancies.Should().Be(closedCount + draftCount + reviewCount + referredCount + rejectedCount + submittedCount + liveCount + liveCount + closingSoon + closingSoonNoApplications);
        }

        [Test, MoqAutoData]
        public async Task Then_Model_Values_Set_For_No_Vacancies(
            string employerAccountId,
            VacancyType vacancyType,
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            VacancyClient vacancyClient)
        {
            vacanciesSummaryProvider.Setup(x => x.GetEmployerOwnedVacancyDashboardByEmployerAccountIdAsync(employerAccountId, vacancyType)).ReturnsAsync(new VacancyDashboard
            {
                VacancyApplicationsDashboard = new List<VacancyApplicationsDashboard>(),
                VacancyStatusDashboard = new List<VacancyStatusDashboard>()
            });
            
            var actual = await vacancyClient.GetDashboardSummary(employerAccountId);
            
            actual.HasVacancies.Should().BeFalse();
            actual.HasOneVacancy.Should().BeFalse();
            actual.HasApplications.Should().BeFalse();
            actual.NumberOfVacancies.Should().Be(0);
        }
        
        
        [Test, MoqAutoData]
        public async Task Then_Model_Values_Set_For_One_Vacancies(
            string employerAccountId,
            VacancyType vacancyType,
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            VacancyClient vacancyClient)
        {
            vacanciesSummaryProvider.Setup(x => x.GetEmployerOwnedVacancyDashboardByEmployerAccountIdAsync(employerAccountId, vacancyType)).ReturnsAsync(new VacancyDashboard
            {
                VacancyApplicationsDashboard = new List<VacancyApplicationsDashboard>(),
                VacancyStatusDashboard = new List<VacancyStatusDashboard>()
            });
            
            var actual = await vacancyClient.GetDashboardSummary(employerAccountId);
            
            actual.HasVacancies.Should().BeFalse();
            actual.HasOneVacancy.Should().BeFalse();
            actual.HasApplications.Should().BeFalse();
            actual.NumberOfVacancies.Should().Be(0);
        }
       
    }
}