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
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;

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

            _controller = new WageController(_orchestrator.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test, MoqAutoData]
        public async Task POST_AdditionalInformation_ValidModel_SuccessfulRedirect(WageExtraInformationViewModel viewModel)
        {
            var orchestratorResponse = new OrchestratorResponse(true);

            _orchestrator.Setup(orchestrator => orchestrator.PostExtraInformationEditModelAsync(It.IsAny<WageExtraInformationViewModel>(), It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);

            var redirectResult = await _controller.AdditionalInformation(viewModel, true) as RedirectToRouteResult;

            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.NumberOfPositions_Get, Is.EqualTo(redirectResult.RouteName));
        }

        [Test, MoqAutoData]
        public async Task POST_AdditionalInformation_Invalid_ReturnsView(WageExtraInformationViewModel viewModel)
        {
            _orchestrator.Setup(orchestrator => orchestrator.GetExtraInformationViewModelAsync(It.IsAny<VacancyRouteModel>()))
                .ReturnsAsync(viewModel);

            _controller.ModelState.AddModelError("PropertyName", "Error Message");

            var result = await _controller.AdditionalInformation(viewModel, true) as ViewResult;

            result.Should().NotBeNull();
            result!.Model.Should().Be(viewModel);
        }

        [Test, MoqAutoData]
        public async Task POST_AdditionalInformation_Errors_ReturnsView(WageExtraInformationViewModel viewModel)
        {
            var orchestratorResponse = new OrchestratorResponse(false);
            orchestratorResponse.Errors.Errors.Add(new EntityValidationError(123, "Test.PropertyName",
                "Test.ErrorMessage", "Test.ErrorCode"));

            _orchestrator.Setup(orchestrator =>
                    orchestrator.GetExtraInformationViewModelAsync(It.IsAny<VacancyRouteModel>()))
                .ReturnsAsync(viewModel);

            _orchestrator.Setup(orchestrator =>
                    orchestrator.PostExtraInformationEditModelAsync(It.IsAny<WageExtraInformationViewModel>(),
                        It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);

            var result = await _controller.AdditionalInformation(viewModel, true) as ViewResult;

            result.Should().NotBeNull();
            result!.Model.Should().Be(viewModel);
        } 
         
        public async Task GET_CompetitiveSalary_ReturnsViewModel()
        {
            var rm = _fixture.Create<VacancyRouteModel>();
            var vm = _fixture.Create<CompetitiveWageViewModel>();
            _orchestrator.Setup(orchestrator => orchestrator.GetCompetitiveWageViewModelAsync(It.Is<VacancyRouteModel>(x => x == rm)))
                .ReturnsAsync(vm);

            var result = await _controller.CompetitiveSalary(rm, true) as ViewResult;

            result.Should().NotBeNull();
            result!.Model.Should().Be(vm);
        }

        [Test]
        public async Task POST_CompetitiveSalary_RedirectsToAddExtraInformation()
        {
            var editModel = _fixture.Create<CompetitiveWageEditModel>();
            var vm = _fixture.Create<CompetitiveWageViewModel>();

            vm.WageType = WageType.FixedWage;
            editModel.WageType = WageType.CompetitiveSalary;

            _orchestrator.Setup(o => o.GetCompetitiveWageViewModelAsync(It.IsAny<CompetitiveWageEditModel>()))
                .ReturnsAsync(vm);

            _orchestrator.Setup(x => x.PostCompetitiveWageEditModelAsync(It.Is<CompetitiveWageEditModel>(x => x == editModel), It.IsAny<VacancyUser>()))
                .ReturnsAsync(new OrchestratorResponse(true));

            var redirectResult = await _controller.CompetitiveSalary(editModel, false) as RedirectToRouteResult;

            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.AddExtraInformation_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(editModel.VacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(editModel.Ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
        }

        [Test, MoqAutoData]
        public async Task POST_Wage_FixedWage_RedirectsToCustomWage(WageViewModel viewModel)
        {
            _orchestrator.Setup(orchestrator => orchestrator.GetWageViewModelAsync(It.IsAny<VacancyRouteModel>()))
                .ReturnsAsync(viewModel);
            var wageEditModel = new WageEditModel { WageType = WageType.FixedWage };

            var redirectResult = await _controller.Wage(wageEditModel, true) as RedirectToRouteResult;

            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.CustomWage_Get, Is.EqualTo(redirectResult.RouteName));
        }

        [Test, MoqAutoData]
        public async Task POST_Wage_CompetitiveSalary_RedirectsToSetCompetitivePayRate(WageViewModel viewModel)
        {
            _orchestrator.Setup(orchestrator => orchestrator.GetWageViewModelAsync(It.IsAny<VacancyRouteModel>()))
                .ReturnsAsync(viewModel);
            var wageEditModel = new WageEditModel { WageType = WageType.CompetitiveSalary };

            var redirectResult = await _controller.Wage(wageEditModel, true) as RedirectToRouteResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.SetCompetitivePayRate_Get, Is.EqualTo(redirectResult.RouteName));
        }

        [Test, MoqAutoData]
        public async Task POST_Wage_NationalMinimumWage_RedirectsToAddExtraInformation(WageViewModel viewModel)
        {
            var orchestratorResponse = new OrchestratorResponse(true);
            _orchestrator.Setup(orchestrator => orchestrator.GetWageViewModelAsync(It.IsAny<VacancyRouteModel>()))
                .ReturnsAsync(viewModel);
            _orchestrator.Setup(orchestrator => orchestrator.PostWageEditModelAsync(It.IsAny<WageEditModel>(), It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);
            var wageEditModel = new WageEditModel { WageType = WageType.NationalMinimumWage };

            var redirectResult = await _controller.Wage(wageEditModel, true) as RedirectToRouteResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.AddExtraInformation_Get, Is.EqualTo(redirectResult.RouteName));
        }

        [Test, MoqAutoData]
        public async Task POST_Wage_NationalMinimumWageForApprentices_RedirectsToAddExtraInformation(WageViewModel viewModel)
        {
            _orchestrator.Setup(orchestrator => orchestrator.GetWageViewModelAsync(It.IsAny<VacancyRouteModel>()))
                .ReturnsAsync(viewModel);
            var orchestratorResponse = new OrchestratorResponse(true);
            _orchestrator.Setup(orchestrator => orchestrator.PostWageEditModelAsync(It.IsAny<WageEditModel>(), It.IsAny<VacancyUser>()))
                .ReturnsAsync(orchestratorResponse);

            var wageEditModel = new WageEditModel { WageType = WageType.NationalMinimumWageForApprentices };

            var redirectResult = await _controller.Wage(wageEditModel, true) as RedirectToRouteResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.AddExtraInformation_Get, Is.EqualTo(redirectResult.RouteName));
        }

        [Test, MoqAutoData]
        public async Task POST_Wage_InvalidWageType_ReturnsView(WageViewModel viewModel)
        {
            _orchestrator.Setup(orchestrator => orchestrator.GetWageViewModelAsync(It.IsAny<VacancyRouteModel>()))
                .ReturnsAsync(viewModel);
            _controller.ModelState.AddModelError("PropertyName", "Error Message");
            var wageEditModel = new WageEditModel();

            var result = await _controller.Wage(wageEditModel, false) as ViewResult;

            result.Should().NotBeNull();
            result!.Model.Should().Be(viewModel);
        }

        [Test, MoqAutoData]
        public async Task GET_AdditionalInformation_ReturnsViewModel(WageExtraInformationViewModel viewModel, VacancyRouteModel vacancyRouteModel)
        {
            _orchestrator.Setup(orchestrator => orchestrator.GetExtraInformationViewModelAsync(It.IsAny<VacancyRouteModel>()))
                        .ReturnsAsync(viewModel);

            var result = await _controller.AdditionalInformation(vacancyRouteModel, "true");

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = (ViewResult)result;
            Assert.That(viewModel, Is.EqualTo(viewResult.Model));
        }
    }
}