using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Controllers.Part1;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Title;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers.Part1;

public class TitleControllerTests
{
    [Test, MoqAutoData]
    public async Task When_Calling_Post_Then_And_Apprenticeship_Vacancy_Goes_To_Title_Select(
        string userName,
        VacancyRouteModel vacancyRouteModel,
        TitleEditModel titleEditModel,
        Mock<TitleOrchestrator> orchestrator)
    {
        orchestrator
            .Setup(o => o.PostTitleEditModelAsync(vacancyRouteModel, titleEditModel, It.IsAny<VacancyUser>(), vacancyRouteModel.Ukprn))
            .ReturnsAsync(new OrchestratorResponse<Guid>(Guid.NewGuid()){Success = true});
        var controller = new TitleController(orchestrator.Object, Mock.Of<IProviderVacancyClient>(), new ServiceParameters("Apprenticeship"));
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new []
            {
                new Claim(ProviderRecruitClaims.IdamsUserNameClaimTypeIdentifier,userName),
                new Claim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier,"10000001")
            }
        ));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext {User = user}
        };
        
        var response = await controller.Title(vacancyRouteModel, titleEditModel, true) as RedirectToRouteResult;

        response.Should().NotBeNull();
        response!.RouteName.Should().Be(RouteNames.Training_Get);
    }
    [Test, MoqAutoData]
    public async Task When_Calling_Post_Then_And_Traineeship_Vacancy_Goes_To_Sector_Select(
        string userName,
        VacancyRouteModel vacancyRouteModel,
        TitleEditModel titleEditModel,
        Mock<TitleOrchestrator> orchestrator)
    {
        orchestrator
            .Setup(o => o.PostTitleEditModelAsync(vacancyRouteModel, titleEditModel, It.IsAny<VacancyUser>(), vacancyRouteModel.Ukprn))
            .ReturnsAsync(new OrchestratorResponse<Guid>(Guid.NewGuid()){Success = true});
        var controller = new TitleController(orchestrator.Object, Mock.Of<IProviderVacancyClient>(), new ServiceParameters("Traineeship"));
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new []
            {
                new Claim(ProviderRecruitClaims.IdamsUserNameClaimTypeIdentifier,userName),
                new Claim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier,"10000001")
            }
        ));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext {User = user}
        };
        
        var response = await controller.Title(vacancyRouteModel, titleEditModel, true) as RedirectToRouteResult;

        response.Should().NotBeNull();
        response!.RouteName.Should().Be(RouteNames.TraineeSector_Get);
    }
}