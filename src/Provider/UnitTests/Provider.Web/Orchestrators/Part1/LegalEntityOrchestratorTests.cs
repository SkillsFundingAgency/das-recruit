using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntity;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;
using Xunit;

namespace Esfa.Recruit.UnitTests.Provider.Web.Orchestrators.Part1
{
    public class LegalEntityOrchestratorTests
    {
        private const long TestUkprn = 12345678;
        private readonly Mock<ILogger<LegalEntityOrchestrator>> _mockLogger;
        private readonly Mock<IProviderVacancyClient> _mockClient;
        private readonly Mock<IRecruitVacancyClient> _mockVacancyClient;
        private readonly LegalEntityOrchestrator _orchestrator;
        private readonly Vacancy _testVacancy;
        private readonly VacancyRouteModel _testRouteModel = new VacancyRouteModel { Ukprn = TestUkprn, VacancyId = Guid.NewGuid() };
        private readonly string AccountLegalEntityPublicHashedId = "ABCEFG";
        public LegalEntityOrchestratorTests()
        {
            _mockLogger = new Mock<ILogger<LegalEntityOrchestrator>>();
            _mockClient = new Mock<IProviderVacancyClient>();
            _mockVacancyClient = new Mock<IRecruitVacancyClient>();
            _testVacancy = GetTestVacancy();
            _mockVacancyClient.Setup(x => x.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(_testVacancy);
            _orchestrator = new LegalEntityOrchestrator(_mockClient.Object, Mock.Of<IRecruitVacancyClient>(), _mockLogger.Object, new Utility(_mockVacancyClient.Object,Mock.Of<IFeature>()));
        }

        [Fact]
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
        
        [Test, MoqAutoData]
        public async Task Then_The_Vacancy_Is_Updated_With_The_AccountLegalEntityPublicHashedId(
            VacancyRouteModel vacancyRouteModel,
            LegalEntityEditModel legalEntityEditModel,
            VacancyUser vacancyUser,
            Vacancy vacancy,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
            LegalEntityOrchestrator orchestrator)
        {
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.Employer_Get))
                .ReturnsAsync(vacancy);
            vacancyClient.Setup(x => x.Validate(vacancy, VacancyRuleSet.None))
                .Returns(new EntityValidationResult { Errors = null });
            
            await orchestrator.SetAccountLegalEntityPublicId(vacancyRouteModel, legalEntityEditModel, vacancyUser);
            
            vacancyClient.Verify(x=>x.UpdateDraftVacancyAsync(It.Is<Vacancy>(c=>c.AccountLegalEntityPublicHashedId.Equals(legalEntityEditModel.SelectedOrganisationId)), vacancyUser ), Times.Once);
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
}
