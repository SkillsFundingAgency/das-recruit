using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1.LegalEntityAndEmployer;

public class LegalEntityAndEmployerOrchestratorPostConfirmAccountLegalEntityModel
{
    [Test, MoqAutoData]
    public async Task Then_The_Vacancy_Is_Created_If_No_VacancyId_Present(
        Guid vacancyId,
        Vacancy vacancy,
        VacancyUser user,
        ConfirmLegalEntityAndEmployerEditModel model,
        VacancyRouteModel vacancyRouteModel,
        [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
        [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
        LegalEntityAndEmployerOrchestrator orchestrator)
    {
        vacancyRouteModel.VacancyId = null;
        
        recruitVacancyClient.Setup(x =>
            x.Validate(It.Is<Vacancy>(c => 
                    c.EmployerAccountId.Equals(model.EmployerAccountId)
                    && c.TrainingProvider.Ukprn.Equals(user.Ukprn)
                    && c.LegalEntityName.Equals(model.AccountLegalEntityName)
                    && c.AccountLegalEntityPublicHashedId.Equals(model.AccountLegalEntityPublicHashedId)),
                Esfa.Recruit.Vacancies.Client.Application.Validation.VacancyRuleSet.None)).Returns(new EntityValidationResult());
        providerVacancyClient.Setup(x => x.CreateVacancyAsync(model.EmployerAccountId, user.Ukprn.Value, null, user,
            model.AccountLegalEntityPublicHashedId, model.AccountLegalEntityName)).ReturnsAsync(vacancyId);
        
        var actual = await orchestrator.PostConfirmAccountLegalEntityModel(vacancyRouteModel, model, user);

        actual.Data.Should().Be(vacancyId);
        
    }

    [Test, MoqAutoData]
    public async Task Then_If_VacancyId_Present_Then_Vacancy_Updated(
        Vacancy vacancy,
        VacancyUser user,
        ConfirmLegalEntityAndEmployerEditModel model,
        VacancyRouteModel vacancyRouteModel,
        [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
        [Frozen] Mock<IUtility> utility,
        LegalEntityAndEmployerOrchestrator orchestrator)
    {
        utility.Setup(x =>
                x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.ConfirmLegalEntityEmployer_Get))
            .ReturnsAsync(vacancy);
        recruitVacancyClient.Setup(x =>
            x.Validate(It.Is<Vacancy>(c => 
                    c.EmployerAccountId.Equals(model.EmployerAccountId)
                    && c.LegalEntityName.Equals(model.AccountLegalEntityName)
                    && c.AccountLegalEntityPublicHashedId.Equals(model.AccountLegalEntityPublicHashedId)),
                Esfa.Recruit.Vacancies.Client.Application.Validation.VacancyRuleSet.None)).Returns(new EntityValidationResult());
        
        var actual = await orchestrator.PostConfirmAccountLegalEntityModel(vacancyRouteModel, model, user);

        actual.Data.Should().Be(vacancy.Id);
        recruitVacancyClient.Verify(x =>
            x.UpdateDraftVacancyAsync(It.Is<Vacancy>(c => 
                c.EmployerAccountId.Equals(model.EmployerAccountId)
                && c.AccountLegalEntityPublicHashedId.Equals(model.AccountLegalEntityPublicHashedId)
                && c.LegalEntityName.Equals(model.AccountLegalEntityName)
                ), user), Times.Once);
    }
        
}