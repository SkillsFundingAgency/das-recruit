using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Controllers.Part2;
using Esfa.Recruit.Provider.Web.Interfaces;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.AdditionalQuestions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers.Part2
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
        public async Task When_Calling_Post_And_Not_Success_Then_Returns_AdditionalQuestions_View_With_ModelState(
            AdditionalQuestionsEditModel editModel,
            OrchestratorResponse orchestratorResponse,
            AdditionalQuestionsViewModel viewModel,
            [Frozen] Mock<IAdditionalQuestionsOrchestrator> mockOrchestrator,
            [Greedy] AdditionalQuestionsController controller)
        {
            ArrangeControllerContext(controller, editModel);
            mockOrchestrator
                .Setup(orchestrator => orchestrator.PostEditModel(editModel, It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);
            mockOrchestrator
                .Setup(orchestrator => orchestrator.GetViewModel(editModel))
                .ReturnsAsync(viewModel);
            
            var response = await controller.AdditionalQuestions(editModel) as ViewResult;

            response.Should().NotBeNull();
            response!.Model.Should().Be(viewModel);
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
            ArrangeControllerContext(controller, editModel);
            mockOrchestrator
                .Setup(orchestrator => orchestrator.PostEditModel(editModel, It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);
            mockOrchestrator
                .Setup(orchestrator => orchestrator.GetViewModel(editModel))
                .ReturnsAsync(viewModel);

            var response = await controller.AdditionalQuestions(editModel) as RedirectToRouteResult;

            response.Should().NotBeNull();
            response!.RouteName.Should().Be(RouteNames.ProviderCheckYourAnswersGet);
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
            ArrangeControllerContext(controller, editModel);
            mockOrchestrator
                .Setup(orchestrator => orchestrator.PostEditModel(editModel, It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);
            mockOrchestrator
                .Setup(orchestrator => orchestrator.GetViewModel(editModel))
                .ReturnsAsync(viewModel);

            var response = await controller.AdditionalQuestions(editModel) as RedirectToRouteResult;

            response.Should().NotBeNull();
            response!.RouteName.Should().Be(RouteNames.ProviderTaskListGet);
        }

        private void ArrangeControllerContext(Controller controller, VacancyRouteModel vacancyRouteModel)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new []
                {
                    new Claim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier,vacancyRouteModel.Ukprn.ToString())
                }
            ));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext {User = user}
            };
        }
    }
}