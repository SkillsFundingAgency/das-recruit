using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Employer.Web.Configuration;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part1
{
    public class TrainingProviderOrchestratorTests
    {        
        [Theory]
        [InlineData("This search won't match a single provider")]
        [InlineData("88888")] // will match multiple providers
        public async Task PostSelectTrainingProviderAsync_TrainingProviderSearch_WhenNoSingleProviderFoundShouldNotReturnFoundProvider(string trainingProviderSearch)
        {
            var fixture = new TrainingProviderOrchestratorTestsFixture();
            fixture
                .WithVacacny(
                    new Vacancy
                    {
                        Id = fixture.VacancyId,
                        EmployerAccountId = TrainingProviderOrchestratorTestsFixture.EmployerAccountId,
                        TrainingProvider = new TrainingProvider(),
                        Title = "specified for route validation",
                        ProgrammeId = "specified for route validation"
                    })
                .Setup();

            var selectTrainingProviderEditModel = new SelectTrainingProviderEditModel
            {
                EmployerAccountId = TrainingProviderOrchestratorTestsFixture.EmployerAccountId,
                VacancyId = fixture.Vacancy.Id,
                SelectionType = TrainingProviderSelectionType.TrainingProviderSearch,
                TrainingProviderSearch = trainingProviderSearch
            };

            var result = await fixture.PostSelectTrainingProviderAsync(selectTrainingProviderEditModel);

            fixture.VerifyNotFoundTrainingProviderUkprn(result);
        }

        [Fact]
        public async Task PostSelectTrainingProviderAsync_TrainingProviderSearch_WhenSingleProviderFoundShouldReturnFoundProvider()
        {
            var fixture = new TrainingProviderOrchestratorTestsFixture();
            fixture
                .WithVacacny(
                    new Vacancy
                    {
                        Id = fixture.VacancyId,
                        EmployerAccountId = TrainingProviderOrchestratorTestsFixture.EmployerAccountId,
                        TrainingProvider = new TrainingProvider(),
                        Title = "specified for route validation",
                        ProgrammeId = "specified for route validation"
                    })
                .Setup();

            var selectTrainingProviderEditModel = new SelectTrainingProviderEditModel
            {
                EmployerAccountId = TrainingProviderOrchestratorTestsFixture.EmployerAccountId,
                VacancyId = fixture.Vacancy.Id,
                SelectionType = TrainingProviderSelectionType.TrainingProviderSearch,
                TrainingProviderSearch = "FIRST TRAINING PROVIDER 88888888",
            };

            var result = await fixture.PostSelectTrainingProviderAsync(selectTrainingProviderEditModel);

            fixture.VerifyFoundTrainingProviderUkprn(result, fixture.TrainingProviderOne.Ukprn.Value);
        }

        [Fact]
        public async Task PostSelectTrainingProviderAsync_UKPRN_WhenSingleProviderFoundShouldReturnFoundProvider()
        {
            var fixture = new TrainingProviderOrchestratorTestsFixture();
            fixture
                .WithVacacny(
                    new Vacancy
                    {
                        Id = fixture.VacancyId,
                        EmployerAccountId = TrainingProviderOrchestratorTestsFixture.EmployerAccountId,
                        TrainingProvider = new TrainingProvider(),
                        Title = "specified for route validation",
                        ProgrammeId = "specified for route validation"
                    })
                .Setup();
            
            var selectTrainingProviderEditModel = new SelectTrainingProviderEditModel
            {
                EmployerAccountId = TrainingProviderOrchestratorTestsFixture.EmployerAccountId,
                VacancyId = fixture.Vacancy.Id,
                SelectionType = TrainingProviderSelectionType.Ukprn,
                Ukprn = fixture.TrainingProviderOne.Ukprn.ToString()
            };

            var result = await fixture.PostSelectTrainingProviderAsync(selectTrainingProviderEditModel);

            fixture.VerifyFoundTrainingProviderUkprn(result, fixture.TrainingProviderOne.Ukprn.Value);
        }

        [Theory]
        [InlineData(88888888)]
        [InlineData(88888889)]
        public async Task PostConfirmEditModelAsync_ShouldFlagProviderFieldIndicator(long ukprn)
        {
            var fixture = new TrainingProviderOrchestratorTestsFixture();
            fixture
                .WithVacacny(
                    new Vacancy
                    {
                        Id = fixture.VacancyId,
                        EmployerAccountId = TrainingProviderOrchestratorTestsFixture.EmployerAccountId,
                        TrainingProvider = new TrainingProvider(),
                        Title = "specified for route validation",
                        ProgrammeId = "specified for route validation"
                    })
                .Setup();

            var confirmTrainingProviderEditModel = new ConfirmTrainingProviderEditModel
            {
                EmployerAccountId = TrainingProviderOrchestratorTestsFixture.EmployerAccountId,
                VacancyId = fixture.Vacancy.Id,
                Ukprn = ukprn.ToString()
            };

            await fixture.PostConfirmEditModelAsync(confirmTrainingProviderEditModel);

            fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.Provider, true);
        }

        [Fact]
        public async Task Then_The_Static_Provider_Is_Returned_For_The_Configured_EmployerAccountId()
        {
            var fixture = new TrainingProviderOrchestratorTestsFixture();
            fixture
                .WithVacacny(
                    new Vacancy
                    {
                        Id = fixture.VacancyId,
                        EmployerAccountId = TrainingProviderOrchestratorTestsFixture.EmployerAccountId,
                        TrainingProvider = new TrainingProvider(),
                        Title = "specified for route validation",
                        ProgrammeId = "specified for route validation"
                    })
                .Setup();
            
            var selectTrainingProviderEditModel = new SelectTrainingProviderEditModel
            {
                EmployerAccountId = TrainingProviderOrchestratorTestsFixture.EmployerAccountId,
                VacancyId = fixture.Vacancy.Id,
                SelectionType = TrainingProviderSelectionType.TrainingProviderSearch,
                TrainingProviderSearch = EsfaTestTrainingProvider.Ukprn.ToString()
            };

            var result = await fixture.PostSelectTrainingProviderAsync(selectTrainingProviderEditModel);

            result.Data.FoundTrainingProviderUkprn.Should().Be(EsfaTestTrainingProvider.Ukprn);
        }

        [Fact]
        public async Task Then_The_Static_Provider_Is_Not_Returned_For_Non_Configured_EmployerAccountId()
        {
            var fixture = new TrainingProviderOrchestratorTestsFixture();
            fixture
                .WithVacacny(
                    new Vacancy
                    {
                        Id = fixture.VacancyId,
                        EmployerAccountId = "ABC123",
                        TrainingProvider = new TrainingProvider(),
                        Title = "specified for route validation",
                        ProgrammeId = "specified for route validation"
                    })
                .Setup();
            
            var selectTrainingProviderEditModel = new SelectTrainingProviderEditModel
            {
                EmployerAccountId = "ABC123",
                VacancyId = fixture.Vacancy.Id,
                SelectionType = TrainingProviderSelectionType.TrainingProviderSearch,
                TrainingProviderSearch = EsfaTestTrainingProvider.Ukprn.ToString()
            };

            var result = await fixture.PostSelectTrainingProviderAsync(selectTrainingProviderEditModel);

            result.Data.FoundTrainingProviderUkprn.Should().BeNull();
        }

        public class TrainingProviderOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.TrainingProvider;
            public TrainingProvider TrainingProviderOne { get; set; }
            public TrainingProvider TrainingProviderTwo { get; set; }
            public TrainingProviderSummary TrainingProviderSummaryOne { get; set; }
            public TrainingProviderSummary TrainingProviderSummaryTwo { get; set; }
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; private set; }
            public TrainingProviderOrchestrator Sut { get; private set; }
            
            public const string EmployerAccountId = "EmployerAccountId";
            public readonly Guid VacancyId = Guid.NewGuid();

            public TrainingProviderOrchestratorTestsFixture()
            {
                MockClient = new Mock<IEmployerVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();
                MockTrainingProviderSummaryProvider = new Mock<ITrainingProviderSummaryProvider>();
                MockTrainingProviderService = new Mock<ITrainingProviderService>();
                
                User = VacancyOrchestratorTestData.GetVacancyUser();
                
                TrainingProviderOne = new TrainingProvider { Ukprn = 88888888 };
                TrainingProviderTwo = new TrainingProvider { Ukprn = 88888889 };
                TrainingProviderSummaryOne = new TrainingProviderSummary { ProviderName = "First Training Provider", Ukprn = TrainingProviderOne.Ukprn.Value };
                TrainingProviderSummaryTwo = new TrainingProviderSummary { ProviderName = "Second Training Provider", Ukprn = TrainingProviderTwo.Ukprn.Value };
            }

            public TrainingProviderOrchestratorTestsFixture WithVacacny(Vacancy vacancy)
            {
                Vacancy = vacancy;
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

                MockTrainingProviderSummaryProvider.Setup(p => p.FindAllAsync()).ReturnsAsync(new List<TrainingProviderSummary>
                {
                    TrainingProviderSummaryOne,
                    TrainingProviderSummaryTwo
                });

                MockTrainingProviderSummaryProvider.Setup(p => p.GetAsync(TrainingProviderSummaryOne.Ukprn))
                    .ReturnsAsync(TrainingProviderSummaryOne);

                MockTrainingProviderSummaryProvider.Setup(p => p.GetAsync(TrainingProviderSummaryTwo.Ukprn))
                    .ReturnsAsync(TrainingProviderSummaryTwo);

                MockTrainingProviderSummaryProvider.Setup(p => p.GetAsync(EsfaTestTrainingProvider.Ukprn))
                    .ReturnsAsync(new TrainingProviderSummary { Ukprn = EsfaTestTrainingProvider.Ukprn, ProviderName = EsfaTestTrainingProvider.Name });
                
                MockTrainingProviderService.Setup(t => t.GetProviderAsync(TrainingProviderOne.Ukprn.Value))
                    .ReturnsAsync(TrainingProviderOne);

                MockTrainingProviderService.Setup(t => t.GetProviderAsync(TrainingProviderTwo.Ukprn.Value))
                    .ReturnsAsync(TrainingProviderTwo);
                
                MockTrainingProviderService.Setup(t => t.GetProviderAsync(EsfaTestTrainingProvider.Ukprn))
                    .ReturnsAsync(new TrainingProvider
                    {
                        Ukprn = EsfaTestTrainingProvider.Ukprn
                    });
                
                var utility = new Utility(MockRecruitVacancyClient.Object, Mock.Of<IFeature>());
                
                Sut = new TrainingProviderOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<TrainingProviderOrchestrator>>(), 
                    Mock.Of<IReviewSummaryService>(), MockTrainingProviderSummaryProvider.Object, MockTrainingProviderService.Object, utility, new RecruitConfiguration(EmployerAccountId));
            }

            public async Task<OrchestratorResponse<PostSelectTrainingProviderResult>> PostSelectTrainingProviderAsync(SelectTrainingProviderEditModel model)
            {
                return await Sut.PostSelectTrainingProviderAsync(model, User);
            }

            public async Task<OrchestratorResponse> PostConfirmEditModelAsync(ConfirmTrainingProviderEditModel model)
            {
                return await Sut.PostConfirmEditModelAsync(model, User);
            }

            public void VerifyFoundTrainingProviderUkprn(OrchestratorResponse<PostSelectTrainingProviderResult> result, long value)
            {
                result.Data.FoundTrainingProviderUkprn.Should().Be(value);
            }

            public void VerifyNotFoundTrainingProviderUkprn(OrchestratorResponse<PostSelectTrainingProviderResult> result)
            {
                result.Data.FoundTrainingProviderUkprn.Should().BeNull();
            }

            public void VerifyTrainingProviderNotSet()
            {
                Vacancy.TrainingProvider.Should().BeNull();
            }

            public void VerifyEmployerReviewFieldIndicators(string fieldIdentifier, bool value)
            {
                Vacancy.EmployerReviewFieldIndicators
                    .Where(p => p.FieldIdentifier == fieldIdentifier).Single()
                    .Should().NotBeNull().And
                    .Match<EmployerReviewFieldIndicator>((x) => x.IsChangeRequested == value);
            }

            public void VerifyUpdateDraftVacancyAsyncIsCalled()
            {
                MockRecruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(Vacancy, User), Times.Once);
            }

            public Mock<IEmployerVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
            public Mock<ITrainingProviderSummaryProvider> MockTrainingProviderSummaryProvider { get; set; }
            public Mock<ITrainingProviderService> MockTrainingProviderService { get; set; }
        }
    }
}
