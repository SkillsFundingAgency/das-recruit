using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers.Part1;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Moq;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SFA.DAS.Testing.AutoFixture;
using Esfa.Recruit.Employer.Web.RouteModel;
using AutoFixture;
using Esfa.Recruit.Employer.Web.RouteModel.Wage;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers.Part1
{
    public class WageControllerTests
    {
        private Mock<IWageOrchestrator> _orchestrator;
        private WageController _controller;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _orchestrator = new Mock<IWageOrchestrator>();
            _fixture = new Fixture();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier, Guid.NewGuid().ToString()),
            }));

            _controller = new WageController(_orchestrator.Object, null);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task WageType_FixedWage_RedirectsToCustomWage()
        {
            _orchestrator.Setup(orchestrator => orchestrator.GetWageViewModelAsync(It.IsAny<WageEditModel>()))
                .ReturnsAsync(new WageViewModel());

            var wageEditModel = new WageEditModel { WageType = WageType.FixedWage };

            var redirectResult = await _controller.Wage(wageEditModel, false) as RedirectToRouteResult;
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.CustomWage_Get, redirectResult.RouteName);
        }

        [Test]
        public async Task WageType_CompetitiveSalary_RedirectsToSetCompetitivePayRate()
        {
            _orchestrator.Setup(orchestrator => orchestrator.GetWageViewModelAsync(It.IsAny<WageEditModel>()))
                .ReturnsAsync(new WageViewModel());

            var wageEditModel = new WageEditModel { WageType = WageType.CompetitiveSalary };

            var redirectResult = await _controller.Wage(wageEditModel, false) as RedirectToRouteResult;
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.SetCompetitivePayRate_Get, redirectResult.RouteName);
        }

        [Test]
        public async Task WageType_NationalMinimumWage_RedirectsToAddExtraInformation()
        {
            var orchestratorResponse = new OrchestratorResponse(true);

            _orchestrator.Setup(orchestrator => orchestrator.PostWageEditModelAsync(It.IsAny<WageEditModel>(), It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);

            var wageEditModel = new WageEditModel { WageType = WageType.NationalMinimumWage };

            var redirectResult = await _controller.Wage(wageEditModel, false) as RedirectToRouteResult;
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.AddExtraInformation_Get, redirectResult.RouteName);
        }

        [Test]
        public async Task WageType_NationalMinimumWageForApprentices_RedirectsToAddExtraInformation()
        {
            var orchestratorResponse = new OrchestratorResponse(true);

            _orchestrator.Setup(orchestrator => orchestrator.PostWageEditModelAsync(It.IsAny<WageEditModel>(), It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);

            var wageEditModel = new WageEditModel { WageType = WageType.NationalMinimumWageForApprentices };

            var redirectResult = await _controller.Wage(wageEditModel, false) as RedirectToRouteResult;
            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.AddExtraInformation_Get, redirectResult.RouteName);
        }

        [Test, MoqAutoData]
        public async Task WageType_Invalid_ReturnsView(WageViewModel viewModel)
        {
            _orchestrator.Setup(orchestrator => orchestrator.GetWageViewModelAsync(It.IsAny<WageEditModel>()))
                .ReturnsAsync(viewModel);
            _controller.ModelState.AddModelError("PropertyName", "Error Message");
            var wageEditModel = new WageEditModel();

            var result = await _controller.Wage(wageEditModel, false) as ViewResult;

            result.Should().NotBeNull();
            result!.Model.Should().Be(viewModel);
        }

        [Test, MoqAutoData]
        public async Task Errors_ReturnsView(WageViewModel viewModel)
        {
            var orchestratorResponse = new OrchestratorResponse(false);
            orchestratorResponse.Errors.Errors.Add(new EntityValidationError(123, "Test.PropertyName", "Test.ErrorMessage", "Test.ErrorCode"));

            _orchestrator.Setup(orchestrator => orchestrator.GetWageViewModelAsync(It.IsAny<WageEditModel>()))
                        .ReturnsAsync(viewModel);

            _orchestrator.Setup(orchestrator => orchestrator.PostWageEditModelAsync(It.IsAny<WageEditModel>(), It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);

            var wageEditModel = new WageEditModel { WageType = WageType.NationalMinimumWageForApprentices };

            var result = await _controller.Wage(wageEditModel, false) as ViewResult;

            result.Should().NotBeNull();
            result!.Model.Should().Be(viewModel);
        }

        [Test, MoqAutoData]
        public async Task GET_CompetitiveSalary_ReturnsViewModel()
        {
            var rm = _fixture.Create<VacancyRouteModel>();
            var vm = _fixture.Create<CompetitiveWageViewModel>();
            _orchestrator.Setup(orchestrator => orchestrator.GetCompetitiveWageViewModelAsync(It.Is<VacancyRouteModel>(x => x == rm)))
                .ReturnsAsync(vm);

            var result = await _controller.CompetitiveSalary(rm) as ViewResult;

            result.Should().NotBeNull();
            result!.Model.Should().Be(vm);
        }

        [Test, MoqAutoData]
        public void POST_CompetitiveSalary_RedirectsToAddExtraInformation()
        {
            var editModel = _fixture.Create<CompetitiveWageEditModel>();

            var redirectResult = _controller.CompetitiveSalary(editModel) as RedirectToRouteResult;

            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.AddExtraInformation_Get, redirectResult.RouteName);
            Assert.AreEqual(editModel.VacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(editModel.EmployerAccountId, redirectResult.RouteValues["EmployerAccountId"]);
            Assert.AreEqual(editModel.WageType, redirectResult.RouteValues["WageType"]);
            Assert.AreEqual(editModel.CompetitiveSalaryType, redirectResult.RouteValues["CompetitiveSalaryType"]);
        }
    }
}