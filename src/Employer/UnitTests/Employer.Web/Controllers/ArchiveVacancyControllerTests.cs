using System.Security.Claims;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Controllers;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.ArchiveVacancy;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Controllers;

public class ArchiveVacancyControllerTests
{
    [Test, MoqAutoData]
    public async Task Then_Redirects_To_VacancyList_With_Info_Message(
        string userName,
        string userId,
        string email,
        ArchiveEditModel model,
        Vacancy vacancy,
        [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
        ArchiveVacancyOrchestrator orchestrator)
    {
        vacancy.ClosingDate = DateTime.UtcNow.AddMonths(-1);
        vacancy.Status = VacancyStatus.Closed;
        vacancy.IsDeleted = false;

        var controller = new ArchiveVacancyController(orchestrator)
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

        var actual = await controller.Archive(model) as RedirectToRouteResult;

        Assert.That(actual, Is.Not.Null);
        actual.RouteName.Should().Be(RouteNames.VacanciesGetAll);
        Assert.That(controller.TempData.ContainsKey(TempDataKeys.DashboardInfoMessage), Is.True);
        Assert.That(string.Format(InfoMessages.AdvertArchived, vacancy.Title, vacancy.VacancyReference), Is.EqualTo(controller.TempData[TempDataKeys.DashboardInfoMessage]));
    }
}