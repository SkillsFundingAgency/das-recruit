using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Controllers.Part1;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers
{
    public class LegalEntityAndEmployerControllerTests
    {
        [Test, MoqAutoData]
        public async Task Then_If_Not_Selected_Then_Shows_Validation_Error(
            ConfirmLegalEntityAndEmployerEditModel editModel,
            VacancyRouteModel vacancyRouteModel,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            var controller = new LegalEntityAndEmployerController(orchestrator, Mock.Of<IWebHostEnvironment>(),
                new ServiceParameters("Apprenticeship"));
            controller.ModelState.AddModelError("HasConfirmedEmployer","Error");
            
            editModel.HasConfirmedEmployer = null;

            var actual = await controller.ConfirmEmployerLegalEntitySelection(editModel);
            var actualResult = actual as ViewResult;
            
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actualResult);
            var actualModel = actualResult.Model as ConfirmLegalEntityAndEmployerViewModel;
            Assert.IsNotNull(actualModel);
            actualModel.EmployerName.Should().Be(editModel.EmployerName);
            actualModel.EmployerAccountId.Should().Be(editModel.EmployerAccountId);
            actualModel.AccountLegalEntityName.Should().Be(editModel.AccountLegalEntityName);
            actualModel.AccountLegalEntityPublicHashedId.Should().Be(editModel.AccountLegalEntityPublicHashedId);
            actualModel.Ukprn.Should().Be(editModel.Ukprn);
        }
        
        [Test, MoqAutoData]
        public async Task Then_Chooses_No_For_Employer_Then_Redirected_To_EmployerLegalEntity_View_For_New_Vacancy(
            ConfirmLegalEntityAndEmployerEditModel editModel,
            VacancyRouteModel vacancyRouteModel,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            editModel.VacancyId = null;
            var controller = new LegalEntityAndEmployerController(orchestrator, Mock.Of<IWebHostEnvironment>(),
                new ServiceParameters("Apprenticeship"));
            editModel.HasConfirmedEmployer = false;

            var actual = await controller.ConfirmEmployerLegalEntitySelection(editModel);

            actual.Should().NotBeNull();
            var result = actual as RedirectToRouteResult;
            result?.RouteName.Should().Be(RouteNames.LegalEntityEmployer_Get);
            result?.RouteValues["ukprn"].Should().Be(editModel.Ukprn);
        }
        [Test, MoqAutoData]
        public async Task Then_Chooses_No_For_Employer_Then_Redirected_To_EmployerLegalEntity_View_For_Existing_Vacancy(
            ConfirmLegalEntityAndEmployerEditModel editModel,
            VacancyRouteModel vacancyRouteModel,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            var controller = new LegalEntityAndEmployerController(orchestrator, Mock.Of<IWebHostEnvironment>(),
                new ServiceParameters("Apprenticeship"));
            editModel.HasConfirmedEmployer = false;

            var actual = await controller.ConfirmEmployerLegalEntitySelection(editModel);

            actual.Should().NotBeNull();
            var result = actual as RedirectToRouteResult;
            result?.RouteName.Should().Be(RouteNames.LegalEntityEmployerChange_Get);
            result?.RouteValues["ukprn"].Should().Be(editModel.Ukprn);
            result?.RouteValues["vacancyId"].Should().Be(editModel.VacancyId);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Has_Confirmed_Employer_Then_Redirected_To_TaskList(
            ConfirmLegalEntityAndEmployerEditModel editModel,
            VacancyRouteModel vacancyRouteModel,
            Vacancy vacancy,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier,vacancyRouteModel.Ukprn.ToString()),
            }));
            utility.Setup(x =>
                    x.GetAuthorisedVacancyForEditAsync(It.Is<VacancyRouteModel>(c=>c.Ukprn.Equals(editModel.Ukprn) && c.VacancyId.Equals(editModel.VacancyId)), RouteNames.ConfirmLegalEntityEmployer_Get))
                .ReturnsAsync(vacancy);
            utility.Setup(x => x.IsTaskListCompleted(vacancy)).Returns(false);
            var controller = new LegalEntityAndEmployerController(orchestrator, Mock.Of<IWebHostEnvironment>(),
                new ServiceParameters("Apprenticeship"))
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = user
                    }
                }
                
            };
            editModel.HasConfirmedEmployer = true;

            var actual = await controller.ConfirmEmployerLegalEntitySelection(editModel);

            actual.Should().NotBeNull();
            var result = actual as RedirectToRouteResult;
            result?.RouteName.Should().Be(RouteNames.ProviderTaskListGet);
            result?.RouteValues["ukprn"].Should().Be(editModel.Ukprn);
            result?.RouteValues["vacancyId"].Should().Be(vacancy.Id);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Has_Confirmed_Employer_And_Completed_TaskList_Then_Redirected_To_CheckYourAnswers(
            ConfirmLegalEntityAndEmployerEditModel editModel,
            VacancyRouteModel vacancyRouteModel,
            Vacancy vacancy,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier,vacancyRouteModel.Ukprn.ToString()),
            }));
            utility.Setup(x =>
                    x.GetAuthorisedVacancyForEditAsync(It.Is<VacancyRouteModel>(c=>c.Ukprn.Equals(editModel.Ukprn) && c.VacancyId.Equals(editModel.VacancyId)), RouteNames.ConfirmLegalEntityEmployer_Get))
                .ReturnsAsync(vacancy);
            utility.Setup(x => x.IsTaskListCompleted(vacancy)).Returns(true);
            var controller = new LegalEntityAndEmployerController(orchestrator, Mock.Of<IWebHostEnvironment>(),
                new ServiceParameters("Apprenticeship"))
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = user
                    }
                }
                
            };
            editModel.HasConfirmedEmployer = true;

            var actual = await controller.ConfirmEmployerLegalEntitySelection(editModel);

            actual.Should().NotBeNull();
            var result = actual as RedirectToRouteResult;
            result?.RouteName.Should().Be(RouteNames.ProviderCheckYourAnswersGet);
            result?.RouteValues["ukprn"].Should().Be(editModel.Ukprn);
            result?.RouteValues["vacancyId"].Should().Be(vacancy.Id);
        }
    }
}