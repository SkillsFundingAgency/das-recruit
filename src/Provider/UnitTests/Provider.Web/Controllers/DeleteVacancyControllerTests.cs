using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Controllers;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.ViewModels.DeleteVacancy;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers
{
    public class DeleteVacancyControllerTests
    {
        [Test, MoqAutoData]
        public async Task Then_Returns_TempData_With_VacancyReference_Title(
                string userName,
                DeleteEditModel model,
                Vacancy vacancy,
                [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
                DeleteVacancyOrchestrator orchestrator)
        {

            vacancy.ClosingDate = DateTime.UtcNow.AddMonths(-1);
            vacancy.Status = VacancyStatus.Submitted;
            vacancy.IsDeleted = false;

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var controller = new DeleteVacancyController(orchestrator)
            {
                TempData = tempData
            };

            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[]
                {
                    new Claim(ProviderRecruitClaims.IdamsUserNameClaimTypeIdentifier,userName),
                    new Claim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier,"10000001")
                }
            ));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            vacancyClient.Setup(x => x.GetVacancyAsync(model.VacancyId.GetValueOrDefault()))
                .ReturnsAsync(vacancy);

            await controller.Delete(model);

            Assert.IsTrue(controller.TempData.ContainsKey(TempDataKeys.VacanciesInfoMessage));
            Assert.AreEqual(string.Format(InfoMessages.VacancyDeleted, vacancy.VacancyReference, vacancy.Title), controller.TempData[TempDataKeys.VacanciesInfoMessage]);
        }
    }
}
