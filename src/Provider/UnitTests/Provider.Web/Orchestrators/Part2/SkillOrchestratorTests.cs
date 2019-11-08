using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Services;
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
        private const long TestUkprn = 12345678;
        private readonly Mock<IRecruitVacancyClient> _mockVacancyClient;
        private readonly SkillsOrchestrator _orchestrator;
        private readonly Vacancy _testVacancy;
        private readonly VacancyRouteModel _testRouteModel = new VacancyRouteModel { Ukprn = TestUkprn, VacancyId = Guid.NewGuid() };

        public SkillOrchestratorTests()
        {
            var mockLogger = new Mock<ILogger<SkillsOrchestrator>>();
            var candidateSkills = GetBaseSkills();
            var mockClient = new Mock<IProviderVacancyClient>();
            _mockVacancyClient = new Mock<IRecruitVacancyClient>();
            _orchestrator = new SkillsOrchestrator(mockClient.Object, _mockVacancyClient.Object, mockLogger.Object, Mock.Of<IReviewSummaryService>());
            _testVacancy = GetTestVacancy();

            _mockVacancyClient.Setup(x => x.GetCandidateSkillsAsync()).ReturnsAsync(candidateSkills);
        }

        [Fact]
        public async Task WhenNoSkillsSaved_ShouldReturnListOfAllBasicSkillsUnchecked()
        {
            _mockVacancyClient.Setup(x => x.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(_testVacancy);

            var result = await _orchestrator.GetSkillsViewModelAsync(_testRouteModel);

            // Count that all 18 base skills are there
            result.Column1Checkboxes.Count(x => x.Selected == false).Should().Be(9);
            result.Column2Checkboxes.Count(x => x.Selected == false).Should().Be(8);
        }

        [Fact]
        public async Task WhenSingleBaseSkillSaved_ShouldReturnListOfAllBasicSkillsWithSkillSelected()
        {
            _testVacancy.Skills = new List<string> { "Logical" }; // Set the saved skill

            _mockVacancyClient.Setup(x => x.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(_testVacancy);

            var result = await _orchestrator.GetSkillsViewModelAsync(_testRouteModel);

            var allCheckboxes = result.Column1Checkboxes.Union(result.Column2Checkboxes);
            allCheckboxes.Where(x => x.Selected).Should().HaveCount(1);
        }

        [Fact]
        public async Task WhenMultipleBaseSkillsSaved_ShouldReturnListOfAllBasicSkillsWithSkillsSelected()
        {
            _testVacancy.Skills = new List<string> { "Logical", "Patience" }; // Set the saved skill

            _mockVacancyClient.Setup(x => x.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(_testVacancy);

            var result = await _orchestrator.GetSkillsViewModelAsync(_testRouteModel);

            var allCheckboxes = result.Column1Checkboxes.Union(result.Column2Checkboxes);
            allCheckboxes.Where(x => x.Selected).Should().HaveCount(2);
        }

        [Fact]
        public async Task WhenCustomSkillHasBeenSaved_ShouldReturnCustomSkillsSelectedInLastItemInSecondColumn()
        {
            _testVacancy.Skills = new List<string> { "Custom1" }; // Set the saved skill

            _mockVacancyClient.Setup(x => x.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(_testVacancy);

            var result = await _orchestrator.GetSkillsViewModelAsync(_testRouteModel);

            var lastCol2Item = result.Column2Checkboxes.Last();
            lastCol2Item.Name.Should().Be("Custom1");
            lastCol2Item.Selected.Should().BeTrue();
        }

        [Fact]
        public async Task WhenMultipleCustomSkillsSaved_ShouldReturnTheCustomSkillsInAlternateColumnsStaringWithColumn2()
        {
            _testVacancy.Skills = new List<string> { "Custom1", "Custom2", "Custom3" }; // Set the saved skill

            _mockVacancyClient.Setup(x => x.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(_testVacancy);

            var result = await _orchestrator.GetSkillsViewModelAsync(_testRouteModel);

            result.Column2Checkboxes.FindIndex(x => x.Name == "Custom1").Should().Be(8); // 9th item in list
            result.Column1Checkboxes.FindIndex(x => x.Name == "Custom2").Should().Be(9); // 10th item in list
            result.Column2Checkboxes.FindIndex(x => x.Name == "Custom3").Should().Be(9); // 10th item in list
        }

        [Fact]
        public async Task WhenCustomDraftSkillHasBeenAdded_ShouldBeAddedToColumn2()
        {
            _testVacancy.Skills = new List<string>(); // No selected skills already persisted

            var draftSkills = new[] { "1-Draft1" };

            _mockVacancyClient.Setup(x => x.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(_testVacancy);

            var result = await _orchestrator.GetSkillsViewModelAsync(_testRouteModel, draftSkills);

            result.Column2Checkboxes.FindIndex(x => x.Name == "Draft1").Should().Be(8); // 9th item in list
        }

        [Fact]
        public async Task WhenCustomeDraftSkillsAdded_ShouldBeAddedToAlternateColumnsStartingWithColumn2()
        {
            _testVacancy.Skills = new List<string>(); // No selected skills already persisted

            var draftSkills = new[] { "1-Draft1", "2-Draft2", "3-Draft3" };

            _mockVacancyClient.Setup(x => x.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(_testVacancy);

            var result = await _orchestrator.GetSkillsViewModelAsync(_testRouteModel, draftSkills);

            result.Column2Checkboxes.FindIndex(x => x.Name == "Draft1").Should().Be(8); // 9th item in list
            result.Column1Checkboxes.FindIndex(x => x.Name == "Draft2").Should().Be(9); // 10th item in list
            result.Column2Checkboxes.FindIndex(x => x.Name == "Draft3").Should().Be(9); // 10th item in list
        }

        [Fact]
        public async Task WhenCustomDraftSkillsAddedAndBaseSkillSelected_ShouldBeAddedToAlternateColumnsStartingWithColumn2()
        {
            _testVacancy.Skills = new List<string>(); // No selected skills already persisted

            var draftSkills = new[] { "1-Draft1", "Patience", "2-Draft2", "3-Draft3" };

            _mockVacancyClient.Setup(x => x.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(_testVacancy);

            var result = await _orchestrator.GetSkillsViewModelAsync(_testRouteModel, draftSkills);

            result.Column2Checkboxes.FindIndex(x => x.Name == "Draft1").Should().Be(8); // 9th item in list
            result.Column1Checkboxes.FindIndex(x => x.Name == "Draft2").Should().Be(9); // 10th item in list
            result.Column2Checkboxes.FindIndex(x => x.Name == "Draft3").Should().Be(9); // 10th item in list
        }

        [Fact]
        public async Task WhenCustomDraftSkillsAdded_ShouldIncludeIndicatorOfItsOrder()
        {
            _testVacancy.Skills = new List<string>(); // No selected skills already persisted

            var draftSkills = new[] { "2-Draft2", "3-Draft3", "1-Draft1" };

            _mockVacancyClient.Setup(x => x.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(_testVacancy);

            var result = await _orchestrator.GetSkillsViewModelAsync(_testRouteModel, draftSkills);

            result.Column2Checkboxes.Single(x => x.Name == "Draft1").Value.Should().Be("1-Draft1");
            result.Column1Checkboxes.Single(x => x.Name == "Draft2").Value.Should().Be("2-Draft2");
            result.Column2Checkboxes.Single(x => x.Name == "Draft3").Value.Should().Be("3-Draft3");
        }

        [Fact]
        public async Task WhenCustomDraftSkillsHaveBeenAdded_ShouldBeOrderedByTheirPrefix()
        {
            _testVacancy.Skills = new List<string>(); // No selected skills already persisted

            var draftSkills = new[] { "2-Draft2", "3-Draft3", "1-Draft1" };

            _mockVacancyClient.Setup(x => x.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(_testVacancy);

            var result = await _orchestrator.GetSkillsViewModelAsync(_testRouteModel, draftSkills);

            result.Column2Checkboxes.FindIndex(x => x.Name == "Draft1").Should().Be(8); // 9th item in list
            result.Column1Checkboxes.FindIndex(x => x.Name == "Draft2").Should().Be(9); // 10th item in list
            result.Column2Checkboxes.FindIndex(x => x.Name == "Draft3").Should().Be(9); // 10th item in list
        }

        private static Vacancy GetTestVacancy()
        {
            return new Vacancy 
            {
                TrainingProvider = new TrainingProvider { Ukprn = TestUkprn },
                Title = "Test Title",
                NumberOfPositions = 1,
                ShortDescription = "Test Short Description",
                LegalEntityName = "legal name",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                EmployerLocation = new Address {
                    Postcode = "AB1 2XZ"
                },
                ProgrammeId = "2",
                Wage = new Wage {
                    Duration = 1,
                    WageType = WageType.NationalMinimumWage
                },
                StartDate = DateTime.Now,
                OwnerType = OwnerType.Provider
            };
        }

        private static List<string> GetBaseSkills()
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
    }
}
