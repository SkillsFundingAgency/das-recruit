using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.VacancyWorkDescription;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part2;

public class VacancyWorkDescriptionOrchestratorTests
{
    [Test, MoqAutoData]
        public async Task When_Calling_GetViewModel_And_Not_Referred_Then_Returns_ViewModel(
            VacancyRouteModel vacancyRouteModel,
            Vacancy vacancy,
            bool isTaskListCompleted,
            [Frozen] Mock<IUtility> mockUtility,
            VacancyWorkDescriptionOrchestrator orchestrator)
        {
            vacancy.Status = VacancyStatus.Draft;
            mockUtility
                .Setup(utility => utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.VacancyWorkDescription_Index_Get))
                .ReturnsAsync(vacancy);
            mockUtility
                .Setup(utility => utility.IsTaskListCompleted(vacancy))
                .Returns(isTaskListCompleted);

            var response = await orchestrator.GetVacancyDescriptionViewModelAsync(vacancyRouteModel);

            response.VacancyId.Should().Be(vacancy.Id);
            response.EmployerAccountId.Should().Be(vacancy.EmployerAccountId.ToUpper());
            response.Title.Should().Be(vacancy.Title);
            response.VacancyDescription.Should().Be(vacancy.Description);
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
            VacancyWorkDescriptionOrchestrator orchestrator)
        {
            vacancy.Status = VacancyStatus.Referred;
            mockUtility
                .Setup(utility => utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.VacancyWorkDescription_Index_Get))
                .ReturnsAsync(vacancy);
            mockUtility
                .Setup(utility => utility.IsTaskListCompleted(vacancy))
                .Returns(isTaskListCompleted);
            mockReviewSummaryService
                .Setup(service => service.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    It.IsAny<ReviewFieldMappingLookupsForPage>()))
                .ReturnsAsync(reviewSummaryViewModel);

            var response = await orchestrator.GetVacancyDescriptionViewModelAsync(vacancyRouteModel);

            response.VacancyId.Should().Be(vacancy.Id);
            response.EmployerAccountId.Should().Be(vacancy.EmployerAccountId.ToUpper());
            response.Title.Should().Be(vacancy.Title);
            response.VacancyDescription.Should().Be(vacancy.Description);
            response.IsTaskListCompleted.Should().Be(isTaskListCompleted);
            response.Review.Should().Be(reviewSummaryViewModel);
        }
        
        [Test, MoqAutoData]
        public async Task When_Calling_PostViewModel_And_No_Validation_Errors_Then_Response_Is_Success_And_Updates_Entity(
            VacancyWorkDescriptionEditModel editModel,
            VacancyUser vacancyUser,
            Vacancy vacancy,
            EntityValidationResult validationResult,
            VacancyWorkDescriptionModel viewModel,
            [Frozen] Mock<IUtility> mockUtility,
            [Frozen] Mock<IRecruitVacancyClient> mockRecruitVacancyClient,
            VacancyWorkDescriptionOrchestrator orchestrator)
        {
            vacancy.Status = VacancyStatus.Draft;
            validationResult.Errors = new List<EntityValidationError>();
            mockUtility
                .Setup(utility => utility.GetAuthorisedVacancyForEditAsync(editModel, RouteNames.VacancyWorkDescription_Index_Post))
                .ReturnsAsync(vacancy);
            mockRecruitVacancyClient
                .Setup(client => client.Validate(vacancy, VacancyRuleSet.Description))
                .Returns(validationResult);

            var response = await orchestrator.PostVacancyDescriptionEditModelAsync(editModel, vacancyUser);

            response.Success.Should().BeTrue();
            mockRecruitVacancyClient.Verify(client => client.Validate(vacancy, VacancyRuleSet.Description), Times.Once);
            mockRecruitVacancyClient.Verify(client => client.UpdateDraftVacancyAsync(vacancy, vacancyUser), Times.Once);
        }
        
        [Test, MoqAutoData]
        public async Task When_Calling_PostViewModel_And_Validation_Errors_Then_Response_Not_Success(
            VacancyWorkDescriptionEditModel editModel,
            VacancyUser vacancyUser,
            Vacancy vacancy,
            EntityValidationResult validationResult,
            VacancyWorkDescriptionModel viewModel,
            [Frozen] Mock<IUtility> mockUtility,
            [Frozen] Mock<IRecruitVacancyClient> mockRecruitVacancyClient,
            VacancyWorkDescriptionOrchestrator orchestrator)
        {
            vacancy.Status = VacancyStatus.Draft;
            mockUtility
                .Setup(utility => utility.GetAuthorisedVacancyForEditAsync(editModel, RouteNames.VacancyWorkDescription_Index_Post))
                .ReturnsAsync(vacancy);
            mockRecruitVacancyClient
                .Setup(client => client.Validate(vacancy, VacancyRuleSet.Description))
                .Returns(validationResult);

            var response = await orchestrator.PostVacancyDescriptionEditModelAsync(editModel, vacancyUser);

            response.Success.Should().BeFalse();
            mockRecruitVacancyClient.Verify(client => client.Validate(vacancy, VacancyRuleSet.Description), Times.Once);
            mockRecruitVacancyClient.Verify(client => client.UpdateDraftVacancyAsync(vacancy, vacancyUser), Times.Never);
        }
}