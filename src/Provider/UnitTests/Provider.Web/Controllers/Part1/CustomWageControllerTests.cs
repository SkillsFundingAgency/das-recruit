﻿using System.Security.Claims;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Controllers.Part1;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.CustomWage;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers.Part1
{
    public class CustomWageControllerTests
    {
        private Mock<ICustomWageOrchestrator> _orchestrator;
        private CustomWageController _controller;
        private Fixture _fixture;
        private long _ukprn;

        [SetUp]
        public void Setup()
        {
            _orchestrator = new Mock<ICustomWageOrchestrator>();
            _fixture = new Fixture();
            _ukprn = 10000234;

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier,_ukprn.ToString())
            }));

            _controller = new CustomWageController(_orchestrator.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test, MoqAutoData]
        public async Task GET_CustomWage_ReturnsViewModel()
        {
            var rm = _fixture.Create<VacancyRouteModel>();
            var vm = _fixture.Create<CustomWageViewModel>();
            _orchestrator.Setup(orchestrator => orchestrator.GetCustomWageViewModelAsync(It.Is<VacancyRouteModel>(x => x == rm)))
                .ReturnsAsync(vm);

            var result = await _controller.CustomWage(rm) as ViewResult;

            result.Should().NotBeNull();
            result!.Model.Should().Be(vm);
        }

        [Test]
        public async Task POST_CustomWage_RedirectsToAddExtraInformation()
        {
            var rm = _fixture.Create<VacancyRouteModel>();
            var vm = _fixture.Create<CustomWageViewModel>();
            var editModel = _fixture.Create<CustomWageEditModel>();

            _orchestrator.Setup(orchestrator => orchestrator.GetCustomWageViewModelAsync(It.Is<VacancyRouteModel>(x => x == rm)))
                .ReturnsAsync(vm);

            _orchestrator.Setup(x => x.PostCustomWageEditModelAsync(It.Is<CustomWageEditModel>(x => x == editModel), It.IsAny<VacancyUser>()))
                .ReturnsAsync(new OrchestratorResponse(true));

            var redirectResult = await _controller.CustomWage(editModel, false) as RedirectToRouteResult;

            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.AddExtraInformation_Get, Is.EqualTo(redirectResult.RouteName));
            Assert.That(editModel.VacancyId, Is.EqualTo(redirectResult.RouteValues["VacancyId"]));
            Assert.That(editModel.Ukprn, Is.EqualTo(redirectResult.RouteValues["Ukprn"]));
        }
    }
}
