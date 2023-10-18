using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Controllers.Part1;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Moq;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Shared.Web.Orchestrators;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SFA.DAS.Testing.AutoFixture;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application.Validation;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers.Part1
{
    public class WageControllerTests
    {
        private Mock<IWageOrchestrator> _orchestrator;
        private WageController _controller;
        private Fixture _fixture;
        private long _ukprn;

        [SetUp]
        public void Setup()
        {
            _orchestrator = new Mock<IWageOrchestrator>();
            _fixture = new Fixture();
            _ukprn = 10000234;

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier,_ukprn.ToString())
            }));

            _controller = new WageController(_orchestrator.Object, null);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test, MoqAutoData]
        public async Task AdditionalInformation_ValidModel_SuccessfulRedirect(WageExtraInformationViewModel viewModel)
        {
            var orchestratorResponse = new OrchestratorResponse(true);

            _orchestrator.Setup(orchestrator => orchestrator.PostExtraInformationEditModelAsync(It.IsAny<WageExtraInformationViewModel>(), It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);

            var redirectResult = await _controller.AdditionalInformation(viewModel, true) as RedirectToRouteResult;

            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.NumberOfPositions_Get, redirectResult.RouteName);
        }

        [Test, MoqAutoData]
        public async Task AdditionalInformation_Invalid_ReturnsView(WageExtraInformationViewModel viewModel)
        {
            _orchestrator.Setup(orchestrator => orchestrator.GetExtraInformationViewModelAsync(It.IsAny<VacancyRouteModel>()))
                .ReturnsAsync(viewModel);

            _controller.ModelState.AddModelError("PropertyName", "Error Message");

            var result = await _controller.AdditionalInformation(viewModel, true) as ViewResult;

            result.Should().NotBeNull();
            result!.Model.Should().Be(viewModel);
        }

        [Test, MoqAutoData]
        public async Task Errors_ReturnsView(WageExtraInformationViewModel viewModel)
        {
            var orchestratorResponse = new OrchestratorResponse(false);
            orchestratorResponse.Errors.Errors.Add(new EntityValidationError(123, "Test.PropertyName", "Test.ErrorMessage", "Test.ErrorCode"));

            _orchestrator.Setup(orchestrator => orchestrator.GetExtraInformationViewModelAsync(It.IsAny<VacancyRouteModel>()))
                .ReturnsAsync(viewModel);

            _orchestrator.Setup(orchestrator => orchestrator.PostExtraInformationEditModelAsync(It.IsAny<WageExtraInformationViewModel>(), It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);

            var result = await _controller.AdditionalInformation(viewModel, true) as ViewResult;

            result.Should().NotBeNull();
            result!.Model.Should().Be(viewModel);
        }

        [Test, MoqAutoData]
        public async Task AdditionalInformation_Get_ReturnsViewModel(WageExtraInformationViewModel viewModel, VacancyRouteModel vacancyRouteModel)
        {
            _orchestrator.Setup(orchestrator => orchestrator.GetExtraInformationViewModelAsync(It.IsAny<VacancyRouteModel>()))
                        .ReturnsAsync(viewModel);

            var result = await _controller.AdditionalInformation(vacancyRouteModel, "true");

            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewModel, viewResult.Model);
        }
    }
}
