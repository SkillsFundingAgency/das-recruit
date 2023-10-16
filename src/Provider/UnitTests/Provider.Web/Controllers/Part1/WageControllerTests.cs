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

            Assert.NotNull(redirectResult);
            Assert.AreEqual(RouteNames.AddExtraInformation_Get, redirectResult.RouteName);
            Assert.AreEqual(editModel.VacancyId, redirectResult.RouteValues["VacancyId"]);
            Assert.AreEqual(editModel.Ukprn, redirectResult.RouteValues["Ukprn"]);
        }
    }
}
