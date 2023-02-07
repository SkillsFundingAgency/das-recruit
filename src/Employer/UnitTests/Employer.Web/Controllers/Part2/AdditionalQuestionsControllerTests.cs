using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers.Part2;
using Esfa.Recruit.Employer.Web.Interfaces;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.AdditionalQuestions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers.Part2
{
    public class AdditionalQuestionsControllerTests
    {
        [Test, MoqAutoData]
        public async Task When_Calling_Get_Then_Returns_View_With_Model_From_Orchestrator(
            VacancyRouteModel vacancyRouteModel,
            AdditionalQuestionsViewModel viewModel,
            [Frozen] Mock<IAdditionalQuestionsOrchestrator> mockOrchestrator,
            [Greedy] AdditionalQuestionsController controller)
        {
            mockOrchestrator
                .Setup(orchestrator => orchestrator.GetViewModel(vacancyRouteModel))
                .ReturnsAsync(viewModel);
            
            var response = await controller.AdditionalQuestions(vacancyRouteModel) as ViewResult;

            response.Should().NotBeNull();
            response!.Model.Should().Be(viewModel);
        }
        
        [Test, MoqAutoData]
        public async Task When_Calling_Post_And_Not_Success_Then_Returns_AdditionalQuestions_View_With_ModelState_And_Values_Set_From_Form(
            AdditionalQuestionsEditModel editModel,
            OrchestratorResponse orchestratorResponse,
            AdditionalQuestionsViewModel viewModel,
            [Frozen] Mock<IAdditionalQuestionsOrchestrator> mockOrchestrator,
            [Greedy] AdditionalQuestionsController controller)
        {
            ArrangeControllerContext(controller);
            mockOrchestrator
                .Setup(orchestrator => orchestrator.PostEditModel(editModel, It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);
            mockOrchestrator
                .Setup(orchestrator => orchestrator.GetViewModel(editModel))
                .ReturnsAsync(viewModel);
            
            var response = await controller.AdditionalQuestions(editModel) as ViewResult;

            response.Should().NotBeNull();
            response!.Model.Should().Be(viewModel);
            var actualModel = response.Model as AdditionalQuestionsViewModel;
            actualModel!.AdditionalQuestion1.Should().Be(editModel.AdditionalQuestion1);
            actualModel!.AdditionalQuestion2.Should().Be(editModel.AdditionalQuestion2);
        }
        
        [Test, MoqAutoData]
        public async Task When_Calling_Post_And_Is_Success_And_TaskList_Is_Completed_Then_Redirects_To_CheckYourAnswers(
            AdditionalQuestionsEditModel editModel,
            OrchestratorResponse orchestratorResponse,
            AdditionalQuestionsViewModel viewModel,
            [Frozen] Mock<IAdditionalQuestionsOrchestrator> mockOrchestrator,
            [Greedy] AdditionalQuestionsController controller)
        {
            orchestratorResponse.Errors.Errors = new List<EntityValidationError>();
            viewModel.IsTaskListCompleted = true;
            ArrangeControllerContext(controller);
            mockOrchestrator
                .Setup(orchestrator => orchestrator.PostEditModel(editModel, It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);
            mockOrchestrator
                .Setup(orchestrator => orchestrator.GetViewModel(editModel))
                .ReturnsAsync(viewModel);

            var response = await controller.AdditionalQuestions(editModel) as RedirectToRouteResult;

            response.Should().NotBeNull();
            response!.RouteName.Should().Be(RouteNames.EmployerCheckYourAnswersGet);
        }
        
        [Test, MoqAutoData]
        public async Task When_Calling_Post_And_Is_Success_And_TaskList_Not_Completed_Then_Redirects_To_TaskList(
            AdditionalQuestionsEditModel editModel,
            OrchestratorResponse orchestratorResponse,
            AdditionalQuestionsViewModel viewModel,
            [Frozen] Mock<IAdditionalQuestionsOrchestrator> mockOrchestrator,
            [Greedy] AdditionalQuestionsController controller)
        {
            orchestratorResponse.Errors.Errors = new List<EntityValidationError>();
            viewModel.IsTaskListCompleted = false;
            ArrangeControllerContext(controller);
            mockOrchestrator
                .Setup(orchestrator => orchestrator.PostEditModel(editModel, It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);
            mockOrchestrator
                .Setup(orchestrator => orchestrator.GetViewModel(editModel))
                .ReturnsAsync(viewModel);

            var response = await controller.AdditionalQuestions(editModel) as RedirectToRouteResult;

            response.Should().NotBeNull();
            response!.RouteName.Should().Be(RouteNames.EmployerTaskListGet);
        }

        private void ArrangeControllerContext(Controller controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new []
                {
                    new Claim(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier,"userid"),
                    new Claim(EmployerRecruitClaims.IdamsUserDisplayNameClaimTypeIdentifier,"displayname"),
                    new Claim(EmployerRecruitClaims.IdamsUserEmailClaimTypeIdentifier,"email")
                }
            ));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext {User = user}
            };
        }
    }
}