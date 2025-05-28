﻿using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntity;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1;

public class LegalEntityOrchestratorUnitTests
{
    [Test, MoqAutoData]
    public async Task Then_The_Vacancy_Is_Updated_With_The_AccountLegalEntityPublicHashedId(
        VacancyRouteModel vacancyRouteModel,
        LegalEntityEditModel legalEntityEditModel,
        VacancyUser vacancyUser,
        Vacancy vacancy,
        LegalEntity legalEntity,
        [Frozen] Mock<IUtility> utility,
        [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
        [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
        LegalEntityOrchestrator orchestrator)
    {
        legalEntity.AccountLegalEntityPublicHashedId = legalEntityEditModel.SelectedOrganisationId;
        utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.LegalEntity_Get))
            .ReturnsAsync(vacancy);
        vacancyClient.Setup(x => x.Validate(vacancy, VacancyRuleSet.None))
            .Returns(new EntityValidationResult { Errors = null });
        providerVacancyClient
            .Setup(x => x.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, vacancy.EmployerAccountId))
            .ReturnsAsync(new EmployerInfo
            {
                LegalEntities = new List<LegalEntity> {legalEntity}
            });
            
        await orchestrator.SetAccountLegalEntityPublicId(vacancyRouteModel, legalEntityEditModel, vacancyUser);
            
        vacancyClient.Verify(x=>x.UpdateDraftVacancyAsync(It.Is<Vacancy>(c=>
            c.AccountLegalEntityPublicHashedId.Equals(legalEntityEditModel.SelectedOrganisationId)
            && c.LegalEntityName.Equals(legalEntity.Name)
        ), vacancyUser ), Times.Once);
    }
}
public class LegalEntityOrchestratorTests
{
    private const long TestUkprn = 12345678;
    private Mock<ILogger<LegalEntityOrchestrator>> _mockLogger;
    private Mock<IProviderVacancyClient> _mockClient;
    private Mock<IRecruitVacancyClient> _mockVacancyClient;
    private LegalEntityOrchestrator _orchestrator;
    private Vacancy _testVacancy;
    private VacancyRouteModel _testRouteModel = new VacancyRouteModel { Ukprn = TestUkprn, VacancyId = Guid.NewGuid() };
    private string AccountLegalEntityPublicHashedId = "ABCEFG";
    
    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<LegalEntityOrchestrator>>();
        _mockClient = new Mock<IProviderVacancyClient>();
        _mockVacancyClient = new Mock<IRecruitVacancyClient>();
        _testVacancy = GetTestVacancy();
        _mockVacancyClient.Setup(x => x.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(_testVacancy);
        _orchestrator = new LegalEntityOrchestrator(_mockClient.Object, Mock.Of<IRecruitVacancyClient>(), _mockLogger.Object, new Utility(_mockVacancyClient.Object, Mock.Of<ITaskListValidator>()));
    }

    [Test]
    public async Task WhenNoLegalEntities_ShouldReturnEmptyList()
    {
        const string TestEmployerAccountId = "XXXXXX";

        var dummyEmployerInfo = new EmployerInfo()
        {
            EmployerAccountId = TestEmployerAccountId,
            LegalEntities = new List<LegalEntity>()
        };

        _mockClient.Setup(x => x.GetProviderEmployerVacancyDataAsync(TestUkprn, TestEmployerAccountId))
            .ReturnsAsync(dummyEmployerInfo);

            

        var result = await _orchestrator.GetLegalEntityViewModelAsync(_testRouteModel, TestUkprn, "", 1, AccountLegalEntityPublicHashedId);

        result.Organisations.Count().Should().Be(0);
    }
        

    private Vacancy GetTestVacancy()
    {
        return new Vacancy 
        {
            TrainingProvider = new TrainingProvider { Ukprn = TestUkprn },
            Title = "Test Title",
            NumberOfPositions = 1,
            ShortDescription = "Test Short Description",
            LegalEntityName = "legal name",
            EmployerNameOption = EmployerNameOption.RegisteredName,
            EmployerLocation = new Recruit.Vacancies.Client.Domain.Entities.Address
            {
                Postcode = "AB1 2XZ"
            },
            ProgrammeId = "2",
            Wage = new Wage
            {
                WageType = WageType.NationalMinimumWage
            },
            OwnerType = OwnerType.Provider
        };
    }
}