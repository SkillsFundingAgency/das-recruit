using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using FluentAssertions;
using Microsoft.Identity.Client;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;
using FeatureNames = Esfa.Recruit.Vacancies.Client.Infrastructure.Configuration.FeatureNames;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services.ProviderVacancyClient
{
    public class ProviderVacancyClientGetDashboardSummaryTests
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
            int numberOfEmployerReviewedApplications,
            int numberOfUnsuccessfulApplications,
            int numberOfSuccessfulApplications,
            int closedSuccessfulApplications,
            int closedUnsuccessfulApplications,
            int closingSoon,
            int closingSoonNoApplications,
            int rejectedCount,
            long ukprn,
            [Frozen] Mock<IFeature> feature, 
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            VacancyClient vacancyClient)
        {
            var vacancyApplicationsDashboard = new List<VacancyApplicationsDashboard>
            {
                new VacancyApplicationsDashboard { Status = VacancyStatus.Closed, StatusCount = closedCount, NoOfNewApplications = numberOfNewApplications,NumberOfEmployerReviewedApplications=numberOfEmployerReviewedApplications, NoOfSuccessfulApplications = closedSuccessfulApplications, NoOfUnsuccessfulApplications = closedUnsuccessfulApplications},
                new VacancyApplicationsDashboard { Status = VacancyStatus.Closed, StatusCount = closedCount, NoOfSuccessfulApplications = closedSuccessfulApplications, NoOfUnsuccessfulApplications = closedUnsuccessfulApplications},
                new VacancyApplicationsDashboard { Status = VacancyStatus.Live,ClosingSoon = false,NumberOfEmployerReviewedApplications=numberOfEmployerReviewedApplications, StatusCount = liveCount, NoOfNewApplications = numberOfNewApplications, NoOfSuccessfulApplications = numberOfSuccessfulApplications,NoOfUnsuccessfulApplications = numberOfUnsuccessfulApplications},
                new VacancyApplicationsDashboard { Status = VacancyStatus.Live,ClosingSoon = false,NumberOfEmployerReviewedApplications=numberOfEmployerReviewedApplications, StatusCount = liveCount, NoOfNewApplications = numberOfNewApplications, NoOfSuccessfulApplications = numberOfSuccessfulApplications,NoOfUnsuccessfulApplications = numberOfUnsuccessfulApplications},
                new VacancyApplicationsDashboard { Status = VacancyStatus.Live,ClosingSoon = true,NumberOfEmployerReviewedApplications=numberOfEmployerReviewedApplications, StatusCount = closingSoon, NoOfNewApplications = numberOfNewApplications}
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
                new VacancyStatusDashboard { Status = VacancyStatus.Live,ClosingSoon = true, StatusCount = closingSoon}
            };

            feature.Setup(x => x.IsFeatureEnabled(FeatureNames.MongoMigration)).Returns(false);
            vacanciesSummaryProvider.Setup(x => x.GetProviderOwnedVacancyDashboardByUkprnAsync(ukprn, false)).ReturnsAsync(new VacancyDashboard
            {
                VacancyApplicationsDashboard = vacancyApplicationsDashboard,
                VacancyStatusDashboard = vacancyDashboards,
                VacanciesClosingSoonWithNoApplications = closingSoonNoApplications
            });
            
            var actual = await vacancyClient.GetDashboardSummary(ukprn);

            actual.Closed.Should().Be(closedCount);
            actual.Draft.Should().Be(draftCount);
            actual.Review.Should().Be(reviewCount);
            actual.Referred.Should().Be(referredCount + rejectedCount);
            actual.Live.Should().Be(liveCount + closingSoon);
            actual.NumberOfNewApplications.Should().Be(numberOfNewApplications*4);
            actual.NumberOfEmployerReviewedApplications.Should().Be(numberOfEmployerReviewedApplications * 4);
            actual.NumberOfUnsuccessfulApplications.Should().Be(numberOfUnsuccessfulApplications*2 + closedUnsuccessfulApplications*2);
            actual.NumberOfSuccessfulApplications.Should().Be(numberOfSuccessfulApplications*2 + closedSuccessfulApplications*2);
            actual.NumberClosingSoon.Should().Be(closingSoon);
            actual.NumberClosingSoonWithNoApplications.Should().Be(closingSoonNoApplications);

            actual.HasVacancies.Should().BeTrue();
            actual.HasOneVacancy.Should().BeFalse();
            actual.HasApplications.Should().BeTrue();
            actual.NumberOfVacancies.Should().Be(closedCount + draftCount + reviewCount + referredCount + rejectedCount + submittedCount + liveCount + closingSoon);
        }

        [Test, MoqAutoData]
        public async Task Then_Model_Values_Set_For_No_Vacancies(
            long ukprn,
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            [Frozen] Mock<IFeature> feature,
            VacancyClient vacancyClient)
        {
            feature.Setup(x => x.IsFeatureEnabled(FeatureNames.MongoMigration)).Returns(false);
            vacanciesSummaryProvider.Setup(x => x.GetProviderOwnedVacancyDashboardByUkprnAsync(ukprn, false)).ReturnsAsync(new VacancyDashboard
            {
                VacancyApplicationsDashboard = new List<VacancyApplicationsDashboard>(),
                VacancyStatusDashboard = new List<VacancyStatusDashboard>()
            });
            
            var actual = await vacancyClient.GetDashboardSummary(ukprn);
            
            actual.HasVacancies.Should().BeFalse();
            actual.HasOneVacancy.Should().BeFalse();
            actual.HasApplications.Should().BeFalse();
            actual.NumberOfVacancies.Should().Be(0);
        }
        
        
        [Test, MoqAutoData]
        public async Task Then_Model_Values_Set_For_One_Vacancies(
            long ukprn,
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            [Frozen] Mock<IFeature> feature,
            VacancyClient vacancyClient)
        {
            feature.Setup(x => x.IsFeatureEnabled(FeatureNames.MongoMigration)).Returns(false);
            vacanciesSummaryProvider.Setup(x => x.GetProviderOwnedVacancyDashboardByUkprnAsync(ukprn, false)).ReturnsAsync(new VacancyDashboard
            {
                VacancyApplicationsDashboard = new List<VacancyApplicationsDashboard>(),
                VacancyStatusDashboard = new List<VacancyStatusDashboard>()
            });
            
            var actual = await vacancyClient.GetDashboardSummary(ukprn);
            
            actual.HasVacancies.Should().BeFalse();
            actual.HasOneVacancy.Should().BeFalse();
            actual.HasApplications.Should().BeFalse();
            actual.NumberOfVacancies.Should().Be(0);
        }
        
        [Test, MoqAutoData]
        public async Task Then_Gets_DashboardSummary_With_TransferredVacancies(
            List<TransferInfo> transferredVacancies,
            long ukprn,
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            VacancyClient vacancyClient)
        {
            vacanciesSummaryProvider.Setup(x => x.GetTransferredFromProviderAsync(ukprn)).ReturnsAsync(transferredVacancies);
            
            var actual = await vacancyClient.GetDashboardSummary(ukprn);

            actual.TransferredVacancies.Should().BeEquivalentTo(transferredVacancies.Select(t => 
                new ProviderDashboardTransferredVacancy
                {
                    LegalEntityName = t.LegalEntityName,
                    TransferredDate = t.TransferredDate,
                    Reason = t.Reason
                }));
        }

        [Test, MoqAutoData]
        public async Task Then_Feature_Flag_True_Maps_To_ProviderDashboardSummary(
            int closedCount,
            int draftCount,
            int reviewCount,
            int referredCount,
            int liveCount,
            int submittedCount,
            int numberOfNewApplications,
            int numberOfEmployerReviewedApplications,
            int numberOfUnsuccessfulApplications,
            int numberOfSuccessfulApplications,
            int closedSuccessfulApplications,
            int closedUnsuccessfulApplications,
            int closingSoon,
            int closingSoonNoApplications,
            int rejectedCount,
            long ukprn,
            GetDashboardCountApiResponse getDashboardCountApiResponse,
            [Frozen] Mock<IFeature> feature,
            [Frozen] Mock<IVacancySummariesProvider> vacanciesSummaryProvider,
            [Frozen] Mock<ITrainingProviderService> trainingProviderService,
            VacancyClient vacancyClient)
        {
            getDashboardCountApiResponse.EmployerReviewedApplicationsCount = reviewCount;
            getDashboardCountApiResponse.NewApplicationsCount = numberOfNewApplications;
            var vacancyApplicationsDashboard = new List<VacancyApplicationsDashboard>
            {
                new() { Status = VacancyStatus.Closed, StatusCount = closedCount, NoOfNewApplications = numberOfNewApplications,NumberOfEmployerReviewedApplications=numberOfEmployerReviewedApplications, NoOfSuccessfulApplications = closedSuccessfulApplications, NoOfUnsuccessfulApplications = closedUnsuccessfulApplications},
                new() { Status = VacancyStatus.Closed, StatusCount = closedCount, NoOfSuccessfulApplications = closedSuccessfulApplications, NoOfUnsuccessfulApplications = closedUnsuccessfulApplications},
                new() { Status = VacancyStatus.Live,ClosingSoon = false,NumberOfEmployerReviewedApplications=numberOfEmployerReviewedApplications, StatusCount = liveCount, NoOfNewApplications = numberOfNewApplications, NoOfSuccessfulApplications = numberOfSuccessfulApplications,NoOfUnsuccessfulApplications = numberOfUnsuccessfulApplications},
                new() { Status = VacancyStatus.Live,ClosingSoon = false,NumberOfEmployerReviewedApplications=numberOfEmployerReviewedApplications, StatusCount = liveCount, NoOfNewApplications = numberOfNewApplications, NoOfSuccessfulApplications = numberOfSuccessfulApplications,NoOfUnsuccessfulApplications = numberOfUnsuccessfulApplications},
                new() { Status = VacancyStatus.Live,ClosingSoon = true,NumberOfEmployerReviewedApplications=numberOfEmployerReviewedApplications, StatusCount = closingSoon, NoOfNewApplications = numberOfNewApplications}
            };

            var vacancyDashboards = new List<VacancyStatusDashboard>
            {
                new() { Status = VacancyStatus.Closed, StatusCount = closedCount},
                new() { Status = VacancyStatus.Closed, StatusCount = closedCount},
                new() { Status = VacancyStatus.Draft, StatusCount = draftCount },
                new() { Status = VacancyStatus.Review, StatusCount = reviewCount },
                new() { Status = VacancyStatus.Referred, StatusCount = referredCount },
                new() { Status = VacancyStatus.Rejected, StatusCount = rejectedCount },
                new() { Status = VacancyStatus.Submitted, StatusCount = submittedCount },
                new() { Status = VacancyStatus.Live,ClosingSoon = false, StatusCount = liveCount},
                new() { Status = VacancyStatus.Live,ClosingSoon = true, StatusCount = closingSoon}
            };


            vacanciesSummaryProvider.Setup(x => x.GetProviderOwnedVacancyDashboardByUkprnAsync(ukprn, true)).ReturnsAsync(new VacancyDashboard
            {
                VacancyApplicationsDashboard = vacancyApplicationsDashboard,
                VacancyStatusDashboard = vacancyDashboards,
                VacanciesClosingSoonWithNoApplications = closingSoonNoApplications
            });

            feature.Setup(x => x.IsFeatureEnabled(FeatureNames.MongoMigration)).Returns(true);
            trainingProviderService.Setup(x => x.GetProviderDashboardStats(ukprn)).ReturnsAsync(getDashboardCountApiResponse);

            var actual = await vacancyClient.GetDashboardSummary(ukprn);

            actual.Closed.Should().Be(closedCount);
            actual.Draft.Should().Be(draftCount);
            actual.Review.Should().Be(reviewCount);
            actual.Referred.Should().Be(referredCount + rejectedCount);
            actual.Live.Should().Be(liveCount + closingSoon);
            actual.NumberOfNewApplications.Should().Be(numberOfNewApplications);
            actual.NumberOfEmployerReviewedApplications.Should().Be(reviewCount);
            actual.NumberOfUnsuccessfulApplications.Should().Be(numberOfUnsuccessfulApplications * 2 + closedUnsuccessfulApplications * 2);
            actual.NumberOfSuccessfulApplications.Should().Be(numberOfSuccessfulApplications * 2 + closedSuccessfulApplications * 2);
            actual.NumberClosingSoon.Should().Be(closingSoon);
            actual.NumberClosingSoonWithNoApplications.Should().Be(closingSoonNoApplications);

            actual.HasVacancies.Should().BeTrue();
            actual.HasOneVacancy.Should().BeFalse();
            actual.HasApplications.Should().BeTrue();
            actual.NumberOfVacancies.Should().Be(closedCount + draftCount + reviewCount + referredCount + rejectedCount + submittedCount + liveCount + closingSoon);
        }
    }
}