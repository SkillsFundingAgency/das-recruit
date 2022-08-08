using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part2
{
    public class FutureProspectsOrchestratorTests
    {
        [Test, MoqAutoData]
        public async Task When_Calling_GetViewModel_And_Not_Referred_Then_Returns_ViewModel(
            VacancyRouteModel vacancyRouteModel,
            Vacancy vacancy,
            bool isTaskListCompleted,
            [Frozen] Mock<IUtility> mockUtility,
            //[Frozen] Mock<IReviewSummaryService> mockReviewSummaryService,
            FutureProspectsOrchestrator orchestrator)
        {
            vacancy.Status = VacancyStatus.Draft;
            mockUtility
                .Setup(utility => utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.FutureProspects_Get))
                .ReturnsAsync(vacancy);
            mockUtility
                .Setup(utility => utility.IsTaskListCompleted(vacancy))
                .Returns(isTaskListCompleted);

            var response = await orchestrator.GetViewModel(vacancyRouteModel);

            response.VacancyId.Should().Be(vacancy.Id);
            response.Title.Should().Be(vacancy.Title);
            response.FutureProspects.Should().Be(vacancy.OutcomeDescription);
            response.IsTaskListCompleted.Should().Be(isTaskListCompleted);
            response.Review.Should().BeEquivalentTo(new ReviewSummaryViewModel());
        }
        
        [Test, MoqAutoData]
        public async Task When_Calling_GetViewModel_And_Is_Referred_Then_Returns_ViewModel_And_Review(
            VacancyRouteModel vacancyRouteModel,
            Vacancy vacancy,
            bool isTaskListCompleted,
            ReviewSummaryViewModel reviewSummaryViewModel,
            [Frozen] Mock<IUtility> mockUtility,
            [Frozen] Mock<IReviewSummaryService> mockReviewSummaryService,
            FutureProspectsOrchestrator orchestrator)
        {
            vacancy.Status = VacancyStatus.Referred;
            mockUtility
                .Setup(utility => utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.FutureProspects_Get))
                .ReturnsAsync(vacancy);
            mockUtility
                .Setup(utility => utility.IsTaskListCompleted(vacancy))
                .Returns(isTaskListCompleted);
            mockReviewSummaryService
                .Setup(service => service.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetFutureProspectsFieldIndicators()))
                .ReturnsAsync(reviewSummaryViewModel);

            var response = await orchestrator.GetViewModel(vacancyRouteModel);

            response.VacancyId.Should().Be(vacancy.Id);
            response.Title.Should().Be(vacancy.Title);
            response.FutureProspects.Should().Be(vacancy.OutcomeDescription);
            response.IsTaskListCompleted.Should().Be(isTaskListCompleted);
            response.Review.Should().Be(reviewSummaryViewModel);
        }
    }
}