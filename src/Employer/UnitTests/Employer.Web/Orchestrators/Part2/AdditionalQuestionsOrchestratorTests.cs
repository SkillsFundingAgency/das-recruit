﻿using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.AdditionalQuestions;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part2;

public class AdditionalQuestionsOrchestratorTests
{
    [Test, MoqAutoData]
        public async Task When_Calling_GetViewModel_And_Not_Referred_Then_Returns_ViewModel(
            VacancyRouteModel vacancyRouteModel,
            Vacancy vacancy,
            bool isTaskListCompleted,
            [Frozen] Mock<IUtility> mockUtility,
            [Frozen] Mock<IOptions<ExternalLinksConfiguration>> mockConfig,
            AdditionalQuestionsOrchestrator orchestrator)
        {
            vacancy.Status = VacancyStatus.Draft;
            mockUtility
                .Setup(utility => utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.AdditionalQuestions_Get))
                .ReturnsAsync(vacancy);
            mockUtility
                .Setup(utility => utility.IsTaskListCompleted(vacancy))
                .Returns(isTaskListCompleted);

            var response = await orchestrator.GetViewModel(vacancyRouteModel);

            response.VacancyId.Should().Be(vacancy.Id);
            response.EmployerAccountId.Should().Be(vacancy.EmployerAccountId.ToUpper());
            response.AdditionalQuestion1.Should().Be(vacancy.AdditionalQuestion1);
            response.AdditionalQuestion2.Should().Be(vacancy.AdditionalQuestion2);
            response.FindAnApprenticeshipUrl.Should().Be(mockConfig.Object.Value.FindAnApprenticeshipUrl);
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
            AdditionalQuestionsOrchestrator orchestrator)
        {
            vacancy.Status = VacancyStatus.Referred;
            mockUtility
                .Setup(utility => utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.AdditionalQuestions_Get))
                .ReturnsAsync(vacancy);
            mockUtility
                .Setup(utility => utility.IsTaskListCompleted(vacancy))
                .Returns(isTaskListCompleted);
            mockReviewSummaryService
                .Setup(service => service.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    It.IsAny<ReviewFieldMappingLookupsForPage>()))
                .ReturnsAsync(reviewSummaryViewModel);

            var response = await orchestrator.GetViewModel(vacancyRouteModel);

            response.VacancyId.Should().Be(vacancy.Id);
            response.EmployerAccountId.Should().Be(vacancy.EmployerAccountId.ToUpper());
            response.AdditionalQuestion1.Should().Be(vacancy.AdditionalQuestion1);
            response.AdditionalQuestion2.Should().Be(vacancy.AdditionalQuestion2);
            response.IsTaskListCompleted.Should().Be(isTaskListCompleted);
            response.Review.Should().Be(reviewSummaryViewModel);
        }
        
        [Test, MoqAutoData]
        public async Task When_Calling_PostViewModel_And_No_Validation_Errors_Then_Response_Is_Success_And_Updates_Entity(
            AdditionalQuestionsEditModel editModel,
            VacancyUser vacancyUser,
            Vacancy vacancy,
            EntityValidationResult validationResult,
            AdditionalQuestionsViewModel viewModel,
            [Frozen] Mock<IUtility> mockUtility,
            [Frozen] Mock<IRecruitVacancyClient> mockRecruitVacancyClient,
            AdditionalQuestionsOrchestrator orchestrator)
        {
            vacancy.Status = VacancyStatus.Draft;
            validationResult.Errors = new List<EntityValidationError>();
            editModel.AdditionalQuestion1 = "How did you find the vacancy?";
            editModel.AdditionalQuestion2 = "Do you have a valid UK driving licence?";
            mockUtility
                .Setup(utility => utility.GetAuthorisedVacancyForEditAsync(editModel, RouteNames.AdditionalQuestions_Post))
                .ReturnsAsync(vacancy);
            mockRecruitVacancyClient
                .Setup(client => client.Validate(vacancy, VacancyRuleSet.AdditionalQuestion1 | VacancyRuleSet.AdditionalQuestion2))
                .Returns(validationResult);

            var response = await orchestrator.PostEditModel(editModel, vacancyUser);

            response.Success.Should().BeTrue();
            mockRecruitVacancyClient.Verify(client => client.Validate(vacancy, VacancyRuleSet.AdditionalQuestion1 | VacancyRuleSet.AdditionalQuestion2), Times.Once);
            response.Errors.Errors
                .Should()
                .NotContain(e => e.PropertyName == "AdditionalQuestion1");
            response.Errors.Errors
                .Should()
                .NotContain(e => e.PropertyName == "AdditionalQuestion2");
        mockRecruitVacancyClient.Verify(client => client.UpdateDraftVacancyAsync(vacancy, vacancyUser), Times.Once);
        }
        
        [Test, MoqAutoData]
        public async Task When_Calling_PostViewModel_And_Validation_Errors_Then_Response_Not_Success(
            AdditionalQuestionsEditModel editModel,
            VacancyUser vacancyUser,
            Vacancy vacancy,
            EntityValidationResult validationResultWithErrors,
            AdditionalQuestionsViewModel viewModel,
            [Frozen] Mock<IUtility> mockUtility,
            [Frozen] Mock<IRecruitVacancyClient> mockRecruitVacancyClient,
            AdditionalQuestionsOrchestrator orchestrator)
        {
            vacancy.Status = VacancyStatus.Draft;
            mockUtility
                .Setup(utility => utility.GetAuthorisedVacancyForEditAsync(editModel, RouteNames.AdditionalQuestions_Post))
                .ReturnsAsync(vacancy);
            mockRecruitVacancyClient
                .Setup(client => client.Validate(vacancy, VacancyRuleSet.AdditionalQuestion1 | VacancyRuleSet.AdditionalQuestion2))
                .Returns(validationResultWithErrors);

            var response = await orchestrator.PostEditModel(editModel, vacancyUser);

            response.Success.Should().BeFalse();
            mockRecruitVacancyClient.Verify(client => client.Validate(vacancy, VacancyRuleSet.AdditionalQuestion1 | VacancyRuleSet.AdditionalQuestion2), Times.Once);
            mockRecruitVacancyClient.Verify(client => client.UpdateDraftVacancyAsync(vacancy, vacancyUser), Times.Never);
        }
}