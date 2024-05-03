using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.Qualifications;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels.Qualifications;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part2
{
    public class QualificationsOrchestratorTests
    {
        public QualificationsOrchestratorTests()
        {
        }

        [Fact]
        public async Task WhenQualificationIsAdded_ShouldAddQualificationToDraftVacancy()
        {
            var fixture = new QualificationsOrchestratorTestsFixture();
            fixture
                .WithQualfication("Mathematics", "A", "GCSE", QualificationWeighting.Desired)
                .Setup();

            var qualification = new Qualification
            {
                Subject = "English",
                Grade = "A",
                QualificationType = "GCSE",
                Weighting = QualificationWeighting.Essential
            };

            await fixture.PostQualificationEditModelForAddAsync(
                new VacancyRouteModel
                {
                    Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                    VacancyId = fixture.Vacancy.Id
                }, 
                FromQualification(qualification));

            fixture.VerifyAddQualificationToDraftVacancy(2, qualification);
        }

        [Fact]
        public async Task WhenQualificationIsAdded_ShouldCallUpdateDraftVacancyAsync()
        {
            var fixture = new QualificationsOrchestratorTestsFixture();
            fixture
                .WithQualfication("Mathematics", "A", "GCSE", QualificationWeighting.Desired)
                .Setup();

            var qualification = new Qualification
            {
                Subject = "English",
                Grade = "A",
                QualificationType = "GCSE",
                Weighting = QualificationWeighting.Essential
            };

            await fixture.PostQualificationEditModelForAddAsync(
                new VacancyRouteModel
                {
                    Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                    VacancyId = fixture.Vacancy.Id
                },
                FromQualification(qualification));

            fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
        }

        [Fact]
        public async Task WhenQualificationIsAdded_ShouldFlagQualificationsFieldIndicator()
        {
            var fixture = new QualificationsOrchestratorTestsFixture();
            fixture
                .WithQualfication("Mathematics", "A", "GCSE", QualificationWeighting.Desired)
                .Setup();

            var qualification = new Qualification
            {
                Subject = "English",
                Grade = "A",
                QualificationType = "GCSE",
                Weighting = QualificationWeighting.Essential
            };

            await fixture.PostQualificationEditModelForAddAsync(
                new VacancyRouteModel
                {
                    Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                    VacancyId = fixture.Vacancy.Id
                },
                FromQualification(qualification));

            fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.Qualifications, true);
        }

        [Fact]
        public async Task WhenQualificationIsUpdated_ShouldUpdateQualificationForDraftVacancy()
        {
            var fixture = new QualificationsOrchestratorTestsFixture();
            fixture
                .WithQualfication("Mathematics", "A", "GCSE", QualificationWeighting.Desired)
                .WithQualfication("English", "B", "GCSE", QualificationWeighting.Essential)
                .WithQualfication("Science", "C", "GCSE", QualificationWeighting.Essential)
                .Setup();

            var qualification = new Qualification
            {
                Subject = "English",
                Grade = "C",
                QualificationType = "GCSE",
                Weighting = QualificationWeighting.Desired
            };

            await fixture.PostQualificationEditModelForEditAsync(
                new VacancyRouteModel
                {
                    Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                    VacancyId = fixture.Vacancy.Id
                },
                FromQualification(qualification),
                index: 1);

            fixture.VerifyUpdateQualificationForDraftVacancy(qualification);
        }

        [Fact]
        public async Task WhenQualificationIsUpdated_ShouldCallUpdateDraftVacancyAsync()
        {
            var fixture = new QualificationsOrchestratorTestsFixture();
            fixture
                .WithQualfication("Mathematics", "A", "GCSE", QualificationWeighting.Desired)
                .WithQualfication("English", "B", "GCSE", QualificationWeighting.Essential)
                .WithQualfication("Science", "C", "GCSE", QualificationWeighting.Essential)
                .Setup();

            var qualification = new Qualification
            {
                Subject = "English",
                Grade = "C",
                QualificationType = "GCSE",
                Weighting = QualificationWeighting.Desired
            };

            await fixture.PostQualificationEditModelForEditAsync(
                new VacancyRouteModel
                {
                    Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                    VacancyId = fixture.Vacancy.Id
                },
                FromQualification(qualification),
                index: 1);

            fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
        }

        [Fact]
        public async Task WhenQualificationIsUpdated_ShouldFlagQualificationsFieldIndicator()
        {
            var fixture = new QualificationsOrchestratorTestsFixture();
            fixture
                .WithQualfication("Mathematics", "A", "GCSE", QualificationWeighting.Desired)
                .WithQualfication("English", "B", "GCSE", QualificationWeighting.Essential)
                .WithQualfication("Science", "C", "GCSE", QualificationWeighting.Essential)
                .Setup();

            var qualification = new Qualification
            {
                Subject = "English",
                Grade = "C",
                QualificationType = "GCSE",
                Weighting = QualificationWeighting.Desired
            };

            await fixture.PostQualificationEditModelForEditAsync(
                new VacancyRouteModel
                {
                    Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                    VacancyId = fixture.Vacancy.Id
                },
                FromQualification(qualification),
                index: 1);

            fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.Qualifications, true);
        }

        [Fact]
        public async Task WhenQualificationIsDeleted_ShouldRemoveQualificationFromDraftVacancy()
        {
            var fixture = new QualificationsOrchestratorTestsFixture();
            fixture
                .WithQualfication("Mathematics", "A", "GCSE", QualificationWeighting.Desired)
                .WithQualfication("English", "B", "GCSE", QualificationWeighting.Essential)
                .WithQualfication("Science", "C", "GCSE", QualificationWeighting.Essential)
                .Setup();

            var qualification = new Qualification
            {
                Subject = "English",
                Grade = "C",
                QualificationType = "GCSE",
                Weighting = QualificationWeighting.Desired
            };

            await fixture.DeleteQualificationAsync(
                new VacancyRouteModel
                {
                    Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                    VacancyId = fixture.Vacancy.Id
                },
                index: 1);

            fixture.VerifyRemoveQualificationFromDraftVacancy(qualification);
        }

        [Fact]
        public async Task WhenQualificationIsDeleted_ShouldCallUpdateDraftVacancyAsync()
        {
            var fixture = new QualificationsOrchestratorTestsFixture();
            fixture
                .WithQualfication("Mathematics", "A", "GCSE", QualificationWeighting.Desired)
                .WithQualfication("English", "B", "GCSE", QualificationWeighting.Essential)
                .WithQualfication("Science", "C", "GCSE", QualificationWeighting.Essential)
                .Setup();

            await fixture.DeleteQualificationAsync(
                new VacancyRouteModel
                {
                    Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                    VacancyId = fixture.Vacancy.Id
                },
                index: 1);

            fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
        }

        [Fact]
        public async Task WhenQualificationIsDeleted_ShouldFlagQualificationsFieldIndicator()
        {
            var fixture = new QualificationsOrchestratorTestsFixture();
            fixture
                .WithQualfication("Mathematics", "A", "GCSE", QualificationWeighting.Desired)
                .WithQualfication("English", "B", "GCSE", QualificationWeighting.Essential)
                .WithQualfication("Science", "C", "GCSE", QualificationWeighting.Essential)
                .Setup();

            await fixture.DeleteQualificationAsync(
                new VacancyRouteModel
                {
                    Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                    VacancyId = fixture.Vacancy.Id
                },
                index: 1);

            fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.Qualifications, true);
        }
        [Theory]
        [InlineData(0,"GCSE",null)]
        [InlineData(1,"A Level",null)]
        [InlineData(2,"BTEC",null)]
        [InlineData(3,"Other","AS Level or equivalent")]
        [InlineData(4,"Other","NVQ")]
        public async Task WhenCallingGetQualificationViewModelForEditAsync_ThenMappedForV2(int index, string expectedQualificationName, string? expectedOtherQualificationName)
        {
            var fixture = new QualificationsOrchestratorTestsFixture();

            fixture
                .WithQualfication("Mathematics", "A", "GCSE or equivalent", QualificationWeighting.Desired)
                .WithQualfication("Mathematics", "A", "A Level or equivalent", QualificationWeighting.Desired)
                .WithQualfication("Mathematics", "A", "BTEC or equivalent", QualificationWeighting.Desired)
                .WithQualfication("Mathematics", "A", "AS Level or equivalent", QualificationWeighting.Desired)
                .WithQualfication("Mathematics", "A", "NVQ", QualificationWeighting.Desired)
                .IsFaaV2()
                .Setup();

            var result = await fixture.GetQualificationForEdit(new VacancyRouteModel
                {
                    Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                    VacancyId = fixture.Vacancy.Id
                },
                index: index);

            result.QualificationType.Should().Be(expectedQualificationName);
            result.OtherQualificationName.Should().Be(expectedOtherQualificationName);
        }


        private QualificationEditModel FromQualification(Qualification qualification)
        {
            return  new QualificationEditModel
            {
                Subject = qualification.Subject,
                Grade = qualification.Grade,
                QualificationType = qualification.QualificationType,
                Weighting = qualification.Weighting
            };
        }

        public class QualificationsOrchestratorTestsFixture
        {
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; private set; }
            public QualificationsOrchestrator Sut {get; private set;}

            public QualificationsOrchestratorTestsFixture()
            {
                MockClient = new Mock<IProviderVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();
                FeatureMock = new Mock<IFeature>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public QualificationsOrchestratorTestsFixture WithQualfication(string subject, string grade, string qualificationType, QualificationWeighting weighting)
            {
                if ((Vacancy.Qualifications == null))
                    Vacancy.Qualifications = new List<Qualification>();

                Vacancy.Qualifications.Add(new Qualification
                {
                    Subject = subject,
                    Grade = grade,
                    QualificationType = qualificationType,
                    Weighting = weighting 
                });
                
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.GetCandidateQualificationsAsync()).ReturnsAsync(new List<string>() { "GCSE", "A-LEVEL" });
                MockRecruitVacancyClient.Setup(x => x.ValidateQualification(It.IsAny<Qualification>())).Returns(new EntityValidationResult());

                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User))
                    .Callback<Vacancy, VacancyUser>((vacancy, user) => { Vacancy = vacancy; })
                    .Returns(Task.FromResult(0));

                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));

                Sut = new QualificationsOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<QualificationsOrchestrator>>(), Mock.Of<IReviewSummaryService>(), new Utility(MockRecruitVacancyClient.Object), FeatureMock.Object);
            }

            public async Task PostQualificationEditModelForAddAsync(VacancyRouteModel vacancyRouteModel, QualificationEditModel model)
            {
                await Sut.PostQualificationEditModelForAddAsync(vacancyRouteModel, model, User);
            }

            public async Task PostQualificationEditModelForEditAsync(VacancyRouteModel vacancyRouteModel, QualificationEditModel model, int index)
            {
                await Sut.PostQualificationEditModelForEditAsync(vacancyRouteModel, model, User, index);
            }

            public async Task DeleteQualificationAsync(VacancyRouteModel vacancyRouteModel, int index)
            {
                await Sut.DeleteQualificationAsync(vacancyRouteModel, index, User);
            }
            
            public async Task<QualificationViewModel> GetQualificationForEdit(VacancyRouteModel vacancyRouteModel, int index)
            {
                return await Sut.GetQualificationViewModelForEditAsync(vacancyRouteModel, index);
            }

            public void VerifyAddQualificationToDraftVacancy(int count, Qualification qualification)
            {
                Vacancy.Qualifications.Should().HaveCount(count);
                ContainEquivalentOf(Vacancy.Qualifications, qualification).Should().BeTrue();
            }

            public void VerifyUpdateQualificationForDraftVacancy(Qualification qualification)
            {
                ContainEquivalentOf(Vacancy.Qualifications, qualification).Should().BeTrue();
            }

            public void VerifyRemoveQualificationFromDraftVacancy(Qualification qualification)
            {
                ContainEquivalentOf(Vacancy.Qualifications, qualification).Should().BeFalse();
            }

            public void VerifyProviderReviewFieldIndicators(string fieldIdentifier, bool value)
            {
                Vacancy.ProviderReviewFieldIndicators
                    .Where(p => p.FieldIdentifier == fieldIdentifier).FirstOrDefault()
                    .Should().NotBeNull().And
                    .Match<ProviderReviewFieldIndicator>((x) => x.IsChangeRequested == value);
            }

            public void VerifyUpdateDraftVacancyAsyncIsCalled()
            {
                MockRecruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(Vacancy, User), Times.Once);
            }
            public QualificationsOrchestratorTestsFixture IsFaaV2()
            {
                FeatureMock.Setup(x => x.IsFeatureEnabled(FeatureNames.FaaV2Improvements)).Returns(true);
                return this;
            }

            private bool ContainEquivalentOf<T>(List<T> collection, T expected)
            {
                // the FluentAssertions ContainEquivalentOf does not have a negated equivalent
                return collection.Any(p => JsonConvert.SerializeObject(p).Equals(JsonConvert.SerializeObject(expected)));
            }

            public Mock<IProviderVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
            public Mock<IFeature> FeatureMock { get; set; }
        }
    }
}
