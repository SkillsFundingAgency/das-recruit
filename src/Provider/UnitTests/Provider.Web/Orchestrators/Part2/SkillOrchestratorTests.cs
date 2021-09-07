using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ShortDescription;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.Skills;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part2
{
    public class SkillOrchestratorTests
    {
        [Fact]
        public async Task WhenNoSkillsSaved_ShouldReturnListOfAllBasicSkillsUnchecked()
        {
            var fixture = new SkillsOrchestratorTestsFixture();
            fixture
                .Setup();

            var vacancyRouteModel = new VacancyRouteModel
            {
                Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = fixture.Vacancy.Id
            };

            var skillsViewModel = await fixture.GetSkillsViewModelAsync(vacancyRouteModel);

            fixture.VerifyAllBasicSkillsAreUnchecked(skillsViewModel);
        }

        [Theory]
        [InlineData(new string[] { "Logical" }, 1)]
        [InlineData(new string[] { "Logical", "Patience" }, 2)]
        public async Task WhenBaseSkillSaved_ShouldReturnListOfAllBasicSkillsWithSkillsSelected(string[] selectedSkills, int selectedCount)
        {
            var fixture = new SkillsOrchestratorTestsFixture();
            fixture
                .WithSelectedSkills(selectedSkills)
                .Setup();

            var vacancyRouteModel = new VacancyRouteModel
            {
                Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = fixture.Vacancy.Id
            };

            var skillsViewModel = await fixture.GetSkillsViewModelAsync(vacancyRouteModel);

            fixture.VerifySelectedSkillsCount(skillsViewModel, selectedCount);
        }

        [Fact]
        public async Task WhenCustomSkillHasBeenSaved_ShouldReturnCustomSkillsSelectedInLastItemInSecondColumn()
        {
            var fixture = new SkillsOrchestratorTestsFixture();
         
            var customSkill = "Custom1";
            fixture
                .WithSelectedSkills(new string[] { customSkill })
                .Setup();

            var vacancyRouteModel = new VacancyRouteModel
            {
                Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = fixture.Vacancy.Id
            };

            var skillsViewModel = await fixture.GetSkillsViewModelAsync(vacancyRouteModel);

            fixture.VerifyColumn2CheckboxesItemSelected(skillsViewModel, customSkill, 8);
        }

        [Fact]
        public async Task WhenMultipleCustomSkillsSaved_ShouldReturnTheCustomSkillsInAlternateColumnsStaringWithColumn2()
        {
            var fixture = new SkillsOrchestratorTestsFixture();

            var customSkill1 = "Custom1";
            var customSkill2 = "Custom2";
            var customSkill3 = "Custom3";

            fixture
                .WithSelectedSkills(new string[] { customSkill1, customSkill2, customSkill3 })
                .Setup();

            var vacancyRouteModel = new VacancyRouteModel
            {
                Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = fixture.Vacancy.Id
            };

            var skillsViewModel = await fixture.GetSkillsViewModelAsync(vacancyRouteModel);

            fixture.VerifyColumn2CheckboxesItemSelected(skillsViewModel, customSkill1, 8);
            fixture.VerifyColumn1CheckboxesItemSelected(skillsViewModel, customSkill2, 9);
            fixture.VerifyColumn2CheckboxesItemSelected(skillsViewModel, customSkill3, 9);
        }

        [Fact]
        public async Task WhenCustomDraftSkillHasBeenAdded_ShouldBeAddedToColumn2()
        {
            var fixture = new SkillsOrchestratorTestsFixture();

            var draftSkill = "Draft1";
            fixture
                .WithSelectedSkills(new string[] { })
                .Setup();

            var vacancyRouteModel = new VacancyRouteModel
            {
                Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = fixture.Vacancy.Id
            };

            var skillsViewModel = await fixture.GetSkillsViewModelAsync(vacancyRouteModel, new string[] { "1-" + draftSkill});

            fixture.VerifyColumn2CheckboxesItemSelected(skillsViewModel, draftSkill, 8);
        }

        [Fact]
        public async Task WhenCustomeDraftSkillsAdded_ShouldBeAddedToAlternateColumnsStartingWithColumn2()
        {
            var fixture = new SkillsOrchestratorTestsFixture();

            var draftSkill1 = "Draft1";
            var draftSkill2 = "Draft2";
            var draftSkill3 = "Draft3";

            fixture
                .WithSelectedSkills(new string[] { })
                .Setup();

            var vacancyRouteModel = new VacancyRouteModel
            {
                Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = fixture.Vacancy.Id
            };

            var skillsViewModel = await fixture.GetSkillsViewModelAsync(vacancyRouteModel, 
                new string[] { "1-" + draftSkill1, "2-" + draftSkill2, "3-" + draftSkill3 });

            fixture.VerifyColumn2CheckboxesItemSelected(skillsViewModel, draftSkill1, 8);
            fixture.VerifyColumn1CheckboxesItemSelected(skillsViewModel, draftSkill2, 9);
            fixture.VerifyColumn2CheckboxesItemSelected(skillsViewModel, draftSkill3, 9);
        }

        [Fact]
        public async Task WhenCustomDraftSkillsAddedAndBaseSkillSelected_ShouldBeAddedToAlternateColumnsStartingWithColumn2()
        {
            var fixture = new SkillsOrchestratorTestsFixture();

            var draftSkill1 = "Draft1";
            var draftSkill2 = "Draft2";
            var draftSkill3 = "Draft3";

            fixture
                .WithSelectedSkills(new string[] { })
                .Setup();

            var vacancyRouteModel = new VacancyRouteModel
            {
                Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = fixture.Vacancy.Id
            };

            var skillsViewModel = await fixture.GetSkillsViewModelAsync(vacancyRouteModel,
                new string[] { "1-" + draftSkill1, "Patience", "2-" + draftSkill2, "3-" + draftSkill3 });

            fixture.VerifyColumn2CheckboxesItemSelected(skillsViewModel, draftSkill1, 8);
            fixture.VerifyColumn1CheckboxesItemSelected(skillsViewModel, draftSkill2, 9);
            fixture.VerifyColumn2CheckboxesItemSelected(skillsViewModel, draftSkill3, 9);
        }
        
        [Fact]
        public async Task WhenCustomDraftSkillsHaveBeenAdded_ShouldBeOrderedByTheirPrefix()
        {
            var fixture = new SkillsOrchestratorTestsFixture();

            var draftSkill1 = "Draft1";
            var draftSkill2 = "Draft2";
            var draftSkill3 = "Draft3";

            fixture
                .WithSelectedSkills(new string[] { })
                .Setup();

            var vacancyRouteModel = new VacancyRouteModel
            {
                Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = fixture.Vacancy.Id
            };

            var skillsViewModel = await fixture.GetSkillsViewModelAsync(vacancyRouteModel,
                new string[] { "2-" + draftSkill2, "3-" + draftSkill3, "1-" + draftSkill1 });

            fixture.VerifyColumn2CheckboxesItemSelected(skillsViewModel, draftSkill1, 8);
            fixture.VerifyColumn1CheckboxesItemSelected(skillsViewModel, draftSkill2, 9);
            fixture.VerifyColumn2CheckboxesItemSelected(skillsViewModel, draftSkill3, 9);
        }

        [Theory]
        [InlineData(new string[] { }, new string[] { }, false)]
        [InlineData(new string[] { }, new string[] { "Organisation skills" }, true)]
        [InlineData(new string[] { "Organisation skills" }, new string[] { "Organisation skills" }, false)]
        [InlineData(new string[] { "Organisation skills" }, new string[] { }, true)]
        public async Task WhenSkillsAreUpdated_ShouldFlagSkillsFieldIndicator(string[] currentlySelectedSkills, string[] newSelectedSkills, bool fieldIndicatorSet)
        {
            var fixture = new SkillsOrchestratorTestsFixture();
            fixture
                .WithSelectedSkills(currentlySelectedSkills)
                .Setup();

            var vacancyRouteModel = new VacancyRouteModel
            {
                Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = fixture.Vacancy.Id
            };

            var skillsEditModel = new SkillsEditModel
            {
                Skills = newSelectedSkills.ToList(),
                AddCustomSkillAction = null,
                AddCustomSkillName = null
            };

            await fixture.PostSkillsEditModelAsync(vacancyRouteModel, skillsEditModel);

            fixture.VerifyProviderReviewFieldIndicators(FieldIdentifiers.Skills, fieldIndicatorSet);
        }

        [Theory]
        [InlineData(new string[] { }, new string[] { })]
        [InlineData(new string[] { }, new string[] { "Organisation skills" })]
        [InlineData(new string[] { "Organisation skills" }, new string[] { "Organisation skills" })]
        [InlineData(new string[] { "Organisation skills" }, new string[] { })]
        public async Task WhenSkillsAreUpdated_ShouldCallUpdateDraftVacancyAsync(string[] currentlySelectedSkills, string[] newSelectedSkills)
        {
            var fixture = new SkillsOrchestratorTestsFixture();
            fixture
                .WithSelectedSkills(currentlySelectedSkills)
                .Setup();

            var vacancyRouteModel = new VacancyRouteModel
            {
                Ukprn = fixture.Vacancy.TrainingProvider.Ukprn.Value,
                VacancyId = fixture.Vacancy.Id
            };

            var skillsEditModel = new SkillsEditModel
            {
                Skills = newSelectedSkills.ToList(),
                AddCustomSkillAction = null,
                AddCustomSkillName = null
            };

            await fixture.PostSkillsEditModelAsync(vacancyRouteModel, skillsEditModel);

            fixture.VerifyUpdateDraftVacancyAsyncIsCalled();
        }

        public class SkillsOrchestratorTestsFixture
        {
            private const VacancyRuleSet ValidationRules = VacancyRuleSet.Skills;
            public VacancyUser User { get; }
            public Vacancy Vacancy { get; }
            public SkillsOrchestrator Sut { get; private set; }

            public SkillsOrchestratorTestsFixture()
            {
                MockClient = new Mock<IProviderVacancyClient>();
                MockRecruitVacancyClient = new Mock<IRecruitVacancyClient>();

                User = VacancyOrchestratorTestData.GetVacancyUser();
                Vacancy = VacancyOrchestratorTestData.GetPart1CompleteVacancy();
            }

            public SkillsOrchestratorTestsFixture WithSelectedSkills(string[] selectedSkills)
            {
                Vacancy.Skills = selectedSkills.ToList();
                return this;
            }

            public void Setup()
            {
                MockRecruitVacancyClient.Setup(x => x.GetVacancyAsync(Vacancy.Id)).ReturnsAsync(Vacancy);
                MockRecruitVacancyClient.Setup(x => x.GetCandidateSkillsAsync()).ReturnsAsync(GetBasicSkills());
                MockRecruitVacancyClient.Setup(x => x.Validate(Vacancy, ValidationRules)).Returns(new EntityValidationResult());
                MockRecruitVacancyClient.Setup(x => x.UpdateDraftVacancyAsync(It.IsAny<Vacancy>(), User));
                MockRecruitVacancyClient.Setup(x => x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), User));
                
                Sut = new SkillsOrchestrator(MockClient.Object, MockRecruitVacancyClient.Object, Mock.Of<ILogger<SkillsOrchestrator>>(), Mock.Of<IReviewSummaryService>());
            }

            public async Task<SkillsViewModel> GetSkillsViewModelAsync(VacancyRouteModel vacancyRouteModel, string[] draftSkills = null)
            {
                return await Sut.GetSkillsViewModelAsync(vacancyRouteModel, draftSkills);
            }

            public async Task PostSkillsEditModelAsync(VacancyRouteModel vacancyRouteModel, SkillsEditModel model)
            {
                await Sut.PostSkillsEditModelAsync(vacancyRouteModel, model, User);
            }

            public void VerifyAllBasicSkillsAreUnchecked(SkillsViewModel skillsViewModel)
            {
                var allCheckboxes = skillsViewModel.Column1Checkboxes.Union(skillsViewModel.Column2Checkboxes);
                allCheckboxes.Where(x => x.Selected == false).Should().HaveCount(GetBasicSkills().Count);
            }

            internal void VerifySelectedSkillsCount(SkillsViewModel skillsViewModel, int count)
            {
                var allCheckboxes = skillsViewModel.Column1Checkboxes.Union(skillsViewModel.Column2Checkboxes);
                allCheckboxes.Where(x => x.Selected).Should().HaveCount(count);
            }

            public SkillsOrchestratorTestsFixture VerifyColumn2CheckboxesItemSelected(SkillsViewModel skillsViewModel, string customSkill, int index)
            {
                skillsViewModel.Column2Checkboxes.FindIndex(x => x.Name == customSkill && x.Selected).Should().Be(index);
                return this;
            }

            public SkillsOrchestratorTestsFixture VerifyColumn1CheckboxesItemSelected(SkillsViewModel skillsViewModel, string customSkill, int index)
            {
                skillsViewModel.Column1Checkboxes.FindIndex(x => x.Name == customSkill && x.Selected).Should().Be(index);
                return this;
            }

            public void VerifyProviderReviewFieldIndicators(string fieldIdentifier, bool value)
            {
                Vacancy.ProviderReviewFieldIndicators
                    .Where(p => p.FieldIdentifier == fieldIdentifier).Single()
                    .Should().NotBeNull().And
                    .Match<ProviderReviewFieldIndicator>((x) => x.IsChangeRequested == value);
            }

            public void VerifyUpdateDraftVacancyAsyncIsCalled()
            {
                MockRecruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(Vacancy, User), Times.Once);
            }

            private static List<string> GetBasicSkills()
            {
                return new List<string>
                {
                    "Communication skills",
                    "IT skills",
                    "Attention to detail",
                    "Organisation skills",
                    "Customer care skills",
                    "Problem solving skills",
                    "Presentation skills",
                    "Administrative skills",
                    "Number skills",
                    "Analytical skills",
                    "Logical",
                    "Team working",
                    "Creative",
                    "Initiative",
                    "Non judgemental",
                    "Patience",
                    "Physical fitness"
                };
            }

            public Mock<IProviderVacancyClient> MockClient { get; set; }
            public Mock<IRecruitVacancyClient> MockRecruitVacancyClient { get; set; }
        }
    }
}
