using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
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

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part2
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
                    EmployerAccountId = fixture.Vacancy.EmployerAccountId,
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
                    EmployerAccountId = fixture.Vacancy.EmployerAccountId,
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
                    EmployerAccountId = fixture.Vacancy.EmployerAccountId,
                    VacancyId = fixture.Vacancy.Id
                },
                FromQualification(qualification));

            fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.Qualifications, true);
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
                    EmployerAccountId = fixture.Vacancy.EmployerAccountId,
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
                    EmployerAccountId = fixture.Vacancy.EmployerAccountId,
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
                    EmployerAccountId = fixture.Vacancy.EmployerAccountId,
                    VacancyId = fixture.Vacancy.Id
                },
                FromQualification(qualification),
                index: 1);

            fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.Qualifications, true);
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
                    EmployerAccountId = fixture.Vacancy.EmployerAccountId,
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
                    EmployerAccountId = fixture.Vacancy.EmployerAccountId,
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
                    EmployerAccountId = fixture.Vacancy.EmployerAccountId,
                    VacancyId = fixture.Vacancy.Id
                },
                index: 1);

            fixture.VerifyEmployerReviewFieldIndicators(FieldIdentifiers.Qualifications, true);
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
                MockClient = new Mock<IEmployerVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

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
                var utility = new Utility(MockRecruitVacancyClient.Object);

                Sut = new QualificationsOrchestrator(MockClient.Object, MockRecruitVacancyClient.Object, Mock.Of<ILogger<QualificationsOrchestrator>>(), Mock.Of<IReviewSummaryService>(), utility, Mock.Of<IFeature>());
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

            public void VerifyEmployerReviewFieldIndicators(string fieldIdentifier, bool value)
            {
                Vacancy.EmployerReviewFieldIndicators
                    .Where(p => p.FieldIdentifier == fieldIdentifier).FirstOrDefault()
                    .Should().NotBeNull().And
                    .Match<EmployerReviewFieldIndicator>((x) => x.IsChangeRequested == value);
            }

            public void VerifyUpdateDraftVacancyAsyncIsCalled()
            {
                MockRecruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(Vacancy, User), Times.Once);
            }

            private bool ContainEquivalentOf<T>(List<T> collection, T expected)
            {
                // the FluentAssertions ContainEquivalentOf does not have a negated equivalent
                return collection.Any(p => JsonConvert.SerializeObject(p).Equals(JsonConvert.SerializeObject(expected)));
            }

            public Mock<IEmployerVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
        }
    }
}
