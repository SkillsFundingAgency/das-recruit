using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Controllers.Part2;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ProviderContactDetails;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Controllers
{
    public class ProviderContactDetailsControllerTests
    {
        [Test, MoqAutoData]
        public async Task Then_If_No_Contact_Details_Selected_Then_Values_Are_Cleared(
            string userName,
            ProviderContactDetailsEditModel editModel,
            [Frozen] Mock<IRecruitVacancyClient> client,
            ProviderContactDetailsOrchestrator orchestrator)
        {
            client.Setup(x => x.Validate(It.IsAny<Vacancy>(), VacancyRuleSet.ProviderContactDetails)).Returns(new EntityValidationResult
            {
                Errors = new List<EntityValidationError>()
            });
            var controller = new ProviderContactDetailsController(orchestrator);
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
            editModel.AddContactDetails = false;

            await controller.ProviderContactDetails(editModel);
            
            client.Verify(x=>x.UpdateDraftVacancyAsync(
                It.Is<Vacancy>(c=>
                    string.IsNullOrEmpty(c.ProviderContact.Email) &&
                    string.IsNullOrEmpty(c.ProviderContact.Phone) &&
                    string.IsNullOrEmpty(c.ProviderContact.Name)
                ), It.IsAny<VacancyUser>()), Times.Once);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Yes_Contact_Details_Selected_Then_Values_Are_Updated(
            string userName,
            ProviderContactDetailsEditModel editModel,
            [Frozen] Mock<IRecruitVacancyClient> client,
            ProviderContactDetailsOrchestrator orchestrator)
        {
            client.Setup(x => x.Validate(It.IsAny<Vacancy>(), VacancyRuleSet.ProviderContactDetails)).Returns(new EntityValidationResult
            {
                Errors = new List<EntityValidationError>()
            });
            var controller = new ProviderContactDetailsController(orchestrator);
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
            editModel.AddContactDetails = true;

            await controller.ProviderContactDetails(editModel);
            
            client.Verify(x=>x.UpdateDraftVacancyAsync(
                It.Is<Vacancy>(c=>
                    c.ProviderContact.Email.Equals(editModel.ProviderContactEmail) &&
                    c.ProviderContact.Phone.Equals(editModel.ProviderContactPhone) &&
                    c.ProviderContact.Name.Equals(editModel.ProviderContactName)
                ), It.IsAny<VacancyUser>()), Times.Once);
        }
    }
}