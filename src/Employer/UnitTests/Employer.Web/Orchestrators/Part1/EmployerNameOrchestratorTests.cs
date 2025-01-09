using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.EmployerName;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Address = Esfa.Recruit.Vacancies.Client.Domain.Entities.Address;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part1;

public class EmployerNameOrchestratorTests
{
    [Test, MoqAutoData]
    public async Task When_Employer_Name_Is_Set_To_Anonymous_Then_The_Vacancy_Fields_Are_Set_Correctly(
        EmployerNameEditModel model,
        VacancyUser user,
        Vacancy vacancy,
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<IUtility> utility,
        [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
        EmployerNameOrchestrator sut)
    {
        // arrange
        feature.Enable(FeatureNames.MultipleLocations);
        model.SelectedEmployerIdentityOption = EmployerIdentityOption.Anonymous;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, RouteNames.EmployerName_Post)).ReturnsAsync(vacancy);
        recruitVacancyClient.Setup(x => x.Validate(vacancy, VacancyRuleSet.EmployerNameOption)).Returns(new EntityValidationResult { Errors = null });
        
        // act
        var result = await sut.PostEmployerNameEditModelAsync(model, user);
        
        // assert
        result.Success.Should().BeTrue();
        vacancy.EmployerName.Should().Be(model.AnonymousName);
        vacancy.AnonymousReason.Should().Be(model.AnonymousReason);
        recruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(vacancy, user), Times.Once);
        utility.Verify(x => x.UpdateEmployerProfile(It.IsAny<VacancyEmployerInfoModel>(), It.IsAny<EmployerProfile>(), It.IsAny<Address>(), It.IsAny<VacancyUser>()), Times.Never);
    }
    
    [Test, MoqAutoData]
    public async Task When_Employer_Name_Is_Set_To_ExistingTradingName_Then_The_Vacancy_Fields_Are_Set_Correctly(
        EmployerNameEditModel model,
        VacancyUser user,
        Vacancy vacancy,
        EmployerProfile employerProfile,
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<IUtility> utility,
        [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
        EmployerNameOrchestrator sut)
    {
        // arrange
        feature.Enable(FeatureNames.MultipleLocations);
        model.SelectedEmployerIdentityOption = EmployerIdentityOption.ExistingTradingName;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, RouteNames.EmployerName_Post)).ReturnsAsync(vacancy);
        recruitVacancyClient.Setup(x => x.Validate(vacancy, VacancyRuleSet.EmployerNameOption)).Returns(new EntityValidationResult { Errors = null });
        recruitVacancyClient.Setup(x => x.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId)).ReturnsAsync(employerProfile);
        
        // act
        var result = await sut.PostEmployerNameEditModelAsync(model, user);
        
        // assert
        result.Success.Should().BeTrue();
        vacancy.EmployerName.Should().Be(employerProfile.TradingName);
        vacancy.AnonymousReason.Should().Be(null);
        recruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(vacancy, user), Times.Once);
        utility.Verify(x => x.UpdateEmployerProfile(It.IsAny<VacancyEmployerInfoModel>(), It.IsAny<EmployerProfile>(), It.IsAny<Address>(), It.IsAny<VacancyUser>()), Times.Never);
    }
    
    [Test, MoqAutoData]
    public async Task When_Employer_Name_Is_Set_To_NewTradingName_Then_The_Vacancy_Fields_Are_Set_Correctly(
        EmployerNameEditModel model,
        VacancyUser user,
        Vacancy vacancy,
        //LegalEntity legalEntity,
        EmployerProfile employerProfile,
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<IUtility> utility,
        [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
        EmployerNameOrchestrator sut)
    {
        // arrange
        feature.Enable(FeatureNames.MultipleLocations);
        model.SelectedEmployerIdentityOption = EmployerIdentityOption.NewTradingName;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, RouteNames.EmployerName_Post)).ReturnsAsync(vacancy);
        recruitVacancyClient.Setup(x => x.Validate(vacancy, VacancyRuleSet.EmployerNameOption | VacancyRuleSet.TradingName)).Returns(new EntityValidationResult { Errors = null });
        recruitVacancyClient.Setup(x => x.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId)).ReturnsAsync(employerProfile);
        
        // act
        var result = await sut.PostEmployerNameEditModelAsync(model, user);
        
        // assert
        result.Success.Should().BeTrue();
        vacancy.EmployerName.Should().Be(model.NewTradingName);
        vacancy.AnonymousReason.Should().Be(null);
        recruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(vacancy, user), Times.Once);
        utility.Verify(x => x.UpdateEmployerProfile(
            It.Is<VacancyEmployerInfoModel>(m => m.NewTradingName == model.NewTradingName && m.EmployerIdentityOption == EmployerIdentityOption.NewTradingName),
            employerProfile,
            null,
            user), Times.Once
        );
    }
    
    [Test, MoqAutoData]
    public async Task When_Employer_Name_Is_Set_To_RegisteredName_Then_The_Vacancy_Fields_Are_Set_Correctly(
        EmployerNameEditModel model,
        VacancyUser user,
        Vacancy vacancy,
        LegalEntity legalEntity,
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<IUtility> utility,
        [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
        [Frozen] Mock<IEmployerVacancyClient> employerVacancyClient,
        EmployerNameOrchestrator sut)
    {
        // arrange
        feature.Enable(FeatureNames.MultipleLocations);
        model.SelectedEmployerIdentityOption = EmployerIdentityOption.RegisteredName;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, RouteNames.EmployerName_Post)).ReturnsAsync(vacancy);
        recruitVacancyClient.Setup(x => x.Validate(vacancy, VacancyRuleSet.EmployerNameOption)).Returns(new EntityValidationResult { Errors = null });
        employerVacancyClient.Setup(x => x.GetEmployerLegalEntitiesAsync(vacancy.EmployerAccountId)).ReturnsAsync([legalEntity]);
        legalEntity.AccountLegalEntityPublicHashedId = vacancy.AccountLegalEntityPublicHashedId;
        
        // act
        var result = await sut.PostEmployerNameEditModelAsync(model, user);
        
        // assert
        result.Success.Should().BeTrue();
        vacancy.EmployerName.Should().Be(legalEntity.Name);
        vacancy.AnonymousReason.Should().Be(null);
        recruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(vacancy, user), Times.Once);
        utility.Verify(x => x.UpdateEmployerProfile(It.IsAny<VacancyEmployerInfoModel>(), It.IsAny<EmployerProfile>(), It.IsAny<Address>(), It.IsAny<VacancyUser>()), Times.Never);
    }
}