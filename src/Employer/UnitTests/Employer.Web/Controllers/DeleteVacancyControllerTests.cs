using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.DeleteVacancy;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers;

public class DeleteVacancyControllerTests
{
    [Test, MoqAutoData]
    public async Task Then_Redirects_To_Dashboard_With_Info_Message(
        string userName,
        string userId,
        string email,
        DeleteEditModel model,
        Vacancy vacancy,
        [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
        DeleteVacancyOrchestrator orchestrator)
    {
        vacancy.ClosingDate = DateTime.UtcNow.AddMonths(-1);
        vacancy.Status = VacancyStatus.Submitted;
        vacancy.IsDeleted = false;

        var controller = new DeleteVacancyController(orchestrator)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier,userId),
                new Claim(EmployerRecruitClaims.IdamsUserDisplayNameClaimTypeIdentifier,userName),
                new Claim(EmployerRecruitClaims.IdamsUserEmailClaimTypeIdentifier,email)
            }
        ));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
        vacancyClient.Setup(x => x.GetVacancyAsync(model.VacancyId))
            .ReturnsAsync(vacancy);

        var actual = await controller.Delete(model) as RedirectToRouteResult;

        Assert.That(actual, Is.Not.Null);
        actual.RouteName.Should().Be(RouteNames.Vacancies_Get);
        Assert.That(controller.TempData.ContainsKey(TempDataKeys.DashboardInfoMessage), Is.True);
        Assert.That(string.Format(InfoMessages.AdvertDeleted, vacancy.VacancyReference, vacancy.Title), Is.EqualTo(controller.TempData[TempDataKeys.DashboardInfoMessage]));
    }
}