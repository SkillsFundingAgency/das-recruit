using System.Security.Claims;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Controllers;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.ViewModels.ArchiveVacancy;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers
{
    public class ArchiveVacancyControllerTests
    {
        [Test, MoqAutoData]
        public async Task Then_Returns_TempData_With_VacancyReference_Title(
                string userName,
                ArchiveEditModel model,
                Vacancy vacancy,
                [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
                ArchiveVacancyOrchestrator orchestrator)
        {
            vacancy.ClosingDate = DateTime.UtcNow.AddMonths(-1);
            vacancy.Status = VacancyStatus.Closed;
            vacancy.IsDeleted = false;

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var controller = new ArchiveVacancyController(orchestrator)
            {
                TempData = tempData
            };

            var user = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ProviderRecruitClaims.IdamsUserNameClaimTypeIdentifier,userName),
                    new Claim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier,"10000001")
                ]
            ));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            vacancyClient.Setup(x => x.GetVacancyAsync(model.VacancyId.GetValueOrDefault()))
                .ReturnsAsync(vacancy);

            await controller.Archive(model);

            Assert.That(controller.TempData.ContainsKey(TempDataKeys.VacanciesInfoMessage), Is.True);
            Assert.That(string.Format(InfoMessages.VacancyArchived, vacancy.Title, vacancy.VacancyReference), Is.EqualTo(controller.TempData[TempDataKeys.VacanciesInfoMessage]));
        }
    }
}