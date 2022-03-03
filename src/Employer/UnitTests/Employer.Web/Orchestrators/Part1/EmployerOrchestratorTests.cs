using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part1
{
    public class EmployerOrchestratorTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_Vacancy_Is_Updated_With_The_AccountLegalEntityPublicHashedId(
            VacancyRouteModel vacancyRouteModel,
            VacancyEmployerInfoModel vacancyEmployerInfoModel,
            VacancyUser vacancyUser,
            Vacancy vacancy,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
            EmployerOrchestrator orchestrator)
        {
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.Employer_Get))
                .ReturnsAsync(vacancy);
            vacancyClient.Setup(x => x.Validate(vacancy, VacancyRuleSet.None))
                .Returns(new EntityValidationResult { Errors = null });
            
            await orchestrator.SetAccountLegalEntityPublicId(vacancyRouteModel, vacancyEmployerInfoModel, vacancyUser);
            
            vacancyClient.Verify(x=>x.UpdateDraftVacancyAsync(It.Is<Vacancy>(c=>c.AccountLegalEntityPublicHashedId.Equals(vacancyEmployerInfoModel.AccountLegalEntityPublicHashedId)), vacancyUser ), Times.Once);
        }
    }
}