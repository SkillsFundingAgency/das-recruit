using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Moq;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using FluentAssertions;
using SFA.DAS.Testing.AutoFixture;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.Controllers.Part1;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers.Part1
{
    public class WageControllerTests
    {
        private Mock<IWageOrchestrator> _orchestrator;
        private Mock<IFeature> _feature;
        private WageController _controller;

        [SetUp]
        public void Setup()
        {
            _orchestrator = new Mock<IWageOrchestrator>();
            _feature = new Mock<IFeature>();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier, "10000001"),
            }));

            _controller = new WageController(_orchestrator.Object, _feature.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test, MoqAutoData]
        public async Task WageType_FixedWage_RedirectsToCustomWage(WageViewModel viewModel)
        {
            _feature.Setup(feat => feat.IsFeatureEnabled("ProviderTaskList"))
                 .Returns(true);
            _orchestrator.Setup(orchestrator => orchestrator.GetWageViewModelAsync(It.IsAny<VacancyRouteModel>()))
                .ReturnsAsync(viewModel);
            var wageEditModel = new WageEditModel { WageType = WageType.FixedWage };

            var redirectResult = await _controller.Wage(wageEditModel, true) as RedirectToRouteResult;

            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.CustomWage_Get, redirectResult.RouteName);
        }

        [Test, MoqAutoData]
        public async Task WageType_CompetitiveSalary_RedirectsToSetCompetitivePayRate(WageViewModel viewModel)
        {
            _feature.Setup(feat => feat.IsFeatureEnabled("ProviderTaskList"))
                .Returns(true);
            _orchestrator.Setup(orchestrator => orchestrator.GetWageViewModelAsync(It.IsAny<VacancyRouteModel>()))
                .ReturnsAsync(viewModel);
            var wageEditModel = new WageEditModel { WageType = WageType.CompetitiveSalary };

            var redirectResult = await _controller.Wage(wageEditModel, true) as RedirectToRouteResult;
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.SetCompetitivePayRate_Get, redirectResult.RouteName);
        }

        [Test, MoqAutoData]
        public async Task WageType_NationalMinimumWage_RedirectsToAddExtraInformation(WageViewModel viewModel)
        {
            var orchestratorResponse = new OrchestratorResponse(true);
            _orchestrator.Setup(orchestrator => orchestrator.GetWageViewModelAsync(It.IsAny<VacancyRouteModel>()))
                .ReturnsAsync(viewModel);
            _feature.Setup(feat => feat.IsFeatureEnabled("ProviderTaskList"))
                .Returns(true);
            _orchestrator.Setup(orchestrator => orchestrator.PostWageEditModelAsync(It.IsAny<WageEditModel>(), It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);
            var wageEditModel = new WageEditModel { WageType = WageType.NationalMinimumWage };

            var redirectResult = await _controller.Wage(wageEditModel, true) as RedirectToRouteResult;
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.AddExtraInformation_Get, redirectResult.RouteName);
        }

        [Test, MoqAutoData]
        public async Task WageType_NationalMinimumWageForApprentices_RedirectsToAddExtraInformation(WageViewModel viewModel)
        {
            _orchestrator.Setup(orchestrator => orchestrator.GetWageViewModelAsync(It.IsAny<VacancyRouteModel>()))
                .ReturnsAsync(viewModel);
            _feature.Setup(feat => feat.IsFeatureEnabled("ProviderTaskList"))
                .Returns(true);
            var orchestratorResponse = new OrchestratorResponse(true);
            _orchestrator.Setup(orchestrator => orchestrator.PostWageEditModelAsync(It.IsAny<WageEditModel>(), It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);

            var wageEditModel = new WageEditModel { WageType = WageType.NationalMinimumWageForApprentices };

            var redirectResult = await _controller.Wage(wageEditModel, true) as RedirectToRouteResult;
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.AddExtraInformation_Get, redirectResult.RouteName);
        }

        [Test, MoqAutoData]
        public async Task WageType_InvalidWageType_ReturnsView(WageViewModel viewModel)
        {
            _orchestrator.Setup(orchestrator => orchestrator.GetWageViewModelAsync(It.IsAny<VacancyRouteModel>()))
                .ReturnsAsync(viewModel);
            _controller.ModelState.AddModelError("PropertyName", "Error Message");
            var wageEditModel = new WageEditModel();

            var result = await _controller.Wage(wageEditModel, false) as ViewResult;

            result.Should().NotBeNull();
            result!.Model.Should().Be(viewModel);
        }
    }
}