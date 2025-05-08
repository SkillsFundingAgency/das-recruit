using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Training;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part1
{
    public class TrainingOrchestratorTests
    {
        private TrainingOrchestratorTestsFixture _fixture;

        public TrainingOrchestratorTests()
        {
            _fixture = new TrainingOrchestratorTestsFixture();
        }

        [TestCase("this is a value", false)]
        [TestCase("this is a new value", true)]
        public async Task WhenUpdated_ShouldFlagFieldIndicators(string programmeId, bool fieldIndicatorSet)
        {
            // arrange
            _fixture
                .WithProgrammeId("this is a value")
                .Setup();

            var confirmTrainingEditModel = new ConfirmTrainingEditModel
            {
                EmployerAccountId = _fixture.Vacancy.EmployerAccountId,
                VacancyId = _fixture.Vacancy.Id,
                ProgrammeId = programmeId
            };
            
            var programme = new ApprenticeshipProgramme()
            {
                Id = programmeId,
                ApprenticeshipType = TrainingType.Standard
            };

            // act
            await _fixture.PostConfirmTrainingEditModelAsync(confirmTrainingEditModel, programme);

            // assert
            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.TrainingLevel, fieldIndicatorSet);
            _fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.Training, fieldIndicatorSet);
        }

        [Test, MoqAutoData]
        public async Task When_Updated_ApprenticeshipType_Is_Set_To_Foundation(
            string programmeId,
            ConfirmTrainingEditModel model,
            VacancyUser user,
            Vacancy vacancy,
            [Frozen] Mock<IUtility> utility,
            [Greedy] TrainingOrchestrator sut)
        {
            // arrange
            vacancy.ApprenticeshipType = null;
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(model, It.IsAny<string>())).ReturnsAsync(vacancy);
            var programme = new ApprenticeshipProgramme()
            {
                Id = programmeId,
                ApprenticeshipType = TrainingType.Foundation
            };

            // act
            await sut.PostConfirmTrainingEditModelAsync(model, programme, user);

            // assert
            vacancy.ApprenticeshipType.Should().Be(ApprenticeshipTypes.Foundation);
        }

        public class TrainingOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.TrainingProgramme;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public TrainingOrchestrator Sut {get; private set;}

            public Mock<IEmployerVacancyClient> MockEmployerVacancyClient { get ; }

            public TrainingOrchestratorTestsFixture()
            {
                MockClient = new Mock<IEmployerVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();
                MockEmployerVacancyClient = new Mock<IEmployerVacancyClient>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public TrainingOrchestratorTestsFixture WithProgrammeId(string programmeId)
            {
                Vacancy.ProgrammeId = programmeId;
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));
                MockEmployerVacancyClient.Setup(x => x.GetEditVacancyInfoAsync(Vacancy.EmployerAccountId))
                    .ReturnsAsync(new EmployerEditVacancyInfo
                    {
                        LegalEntities = new List<LegalEntity>
                        {
                            new LegalEntity(),
                            new LegalEntity()
                        }
                    });
                var utility = new Utility(MockRecruitVacancyClient.Object);
                
                Sut = new TrainingOrchestrator(MockClient.Object, MockRecruitVacancyClient.Object, Mock.Of<ILogger<TrainingOrchestrator>>(), 
                    Mock.Of<IReviewSummaryService>(), utility, MockEmployerVacancyClient.Object);
            }

            public async Task PostConfirmTrainingEditModelAsync(ConfirmTrainingEditModel model, ApprenticeshipProgramme programme)
            {
                await Sut.PostConfirmTrainingEditModelAsync(model, programme, User);
            }

            public void VerifyEmployerReviewFieldIndicators(string fieldIdentifier, bool value)
            {
                Vacancy.EmployerReviewFieldIndicators.Single(p => p.FieldIdentifier == fieldIdentifier)
                    .Should().NotBeNull().And
                    .Match<EmployerReviewFieldIndicator>((x) => x.IsChangeRequested == value);
            }

            public Mock<IEmployerVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
        }
    }
}
