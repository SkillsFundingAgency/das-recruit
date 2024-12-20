using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Orchestrators.Part2;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels.Qualifications;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.Part2
{
    public class QualificationsOrchestratorTests
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [TestCase(0,"GCSE",null)]
        [TestCase(1,"A Level",null)]
        [TestCase(2,"BTEC",null)]
        [TestCase(3,"Other","AS Level or equivalent")]
        [TestCase(4,"Other","NVQ")]
        public async Task WhenCallingGetQualificationViewModelForEditAsync_ThenMappedForV2(int index, string expectedQualificationName, string? expectedOtherQualificationName)
        {
            var fixture = new QualificationsOrchestratorTestsFixture();

            fixture
                .WithQualfication("Mathematics", "A", "GCSE or equivalent", QualificationWeighting.Desired)
                .WithQualfication("Mathematics", "A", "A Level or equivalent", QualificationWeighting.Desired)
                .WithQualfication("Mathematics", "A", "BTEC or equivalent", QualificationWeighting.Desired)
                .WithQualfication("Mathematics", "A", "AS Level or equivalent", QualificationWeighting.Desired)
                .WithQualfication("Mathematics", "A", "NVQ", QualificationWeighting.Desired)
                .Setup();

            var result = await fixture.GetQualificationForEdit(new VacancyRouteModel
                {
                    EmployerAccountId = fixture.Vacancy.EmployerAccountId,
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

                Sut = new QualificationsOrchestrator(MockRecruitVacancyClient.Object, Mock.Of<ILogger<QualificationsOrchestrator>>(), Mock.Of<IReviewSummaryService>(), utility);
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
