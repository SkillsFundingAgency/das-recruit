using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels.Skills;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;
using System.Linq;
using Esfa.Recruit.Shared.Web.Orchestrators;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Orchestrators
{
    public class SkillsOrchestratorHelperTests
    {
        
        public SkillsOrchestratorHelperTests()
        {}

        [Fact]
        public void SetViewModelSkillsFromDraftSkills_ShouldOrderVacancySkills()
        {
            var helper = new SkillsOrchestratorHelper(GetBaseSkills);

            var vm = new TestSkillsViewModel();
            var draftSkills = new List<string>
            {
                "1-Custom Skill 2",
                "Initiative",
                "Problem solving skills",
                "2-Custom Skill 1",
                "Administrative skills",
                "Communication skills"
            };

            helper.SetViewModelSkillsFromDraftSkills(vm, draftSkills);

            vm.Column1Checkboxes.Count.Should().Be(10);
            vm.Column1Checkboxes.Count(c => c.Selected == false).Should().Be(6);

            vm.Column2Checkboxes.Count.Should().Be(9);
            vm.Column2Checkboxes.Count(c => c.Selected == false).Should().Be(7);
            
            vm.Column1Checkboxes[0].Value.Should().Be("Communication skills");
            vm.Column1Checkboxes[0].Selected.Should().BeTrue();

            vm.Column1Checkboxes[5].Value.Should().Be("Problem solving skills");
            vm.Column1Checkboxes[5].Selected.Should().BeTrue();

            vm.Column1Checkboxes[7].Value.Should().Be("Administrative skills");
            vm.Column1Checkboxes[7].Selected.Should().BeTrue();

            vm.Column1Checkboxes[9].Value.Should().Be("2-Custom Skill 1");
            vm.Column1Checkboxes[9].Selected.Should().BeTrue();

            vm.Column2Checkboxes[4].Value.Should().Be("Initiative");
            vm.Column2Checkboxes[4].Selected.Should().BeTrue();

            vm.Column2Checkboxes[8].Value.Should().Be("1-Custom Skill 2");
            vm.Column2Checkboxes[8].Selected.Should().BeTrue();
        }

        [Fact]
        public void SetViewModelSkillsFromVacancy_ShouldOrderVacancySkills()
        {
            var helper = new SkillsOrchestratorHelper(GetBaseSkills);

            var vm = new TestSkillsViewModel();
            var vacancy = new Vacancy
            {
                Skills = new List<string>
                {
                    "Custom Skill 2",
                    "Initiative",
                    "Problem solving skills",
                    "Custom Skill 1",
                    "Administrative skills",
                    "Communication skills"
                }
            };
            helper.SetViewModelSkillsFromVacancy(vm, vacancy);

            vm.Column1Checkboxes.Count.Should().Be(10);
            vm.Column1Checkboxes.Count(c => c.Selected == false).Should().Be(6);

            vm.Column2Checkboxes.Count.Should().Be(9);
            vm.Column2Checkboxes.Count(c => c.Selected == false).Should().Be(7);

            vm.Column1Checkboxes[0].Value.Should().Be("Communication skills");
            vm.Column1Checkboxes[0].Selected.Should().BeTrue();

            vm.Column1Checkboxes[5].Value.Should().Be("Problem solving skills");
            vm.Column1Checkboxes[5].Selected.Should().BeTrue();

            vm.Column1Checkboxes[7].Value.Should().Be("Administrative skills");
            vm.Column1Checkboxes[7].Selected.Should().BeTrue();

            vm.Column1Checkboxes[9].Value.Should().Be("2-Custom Skill 1");
            vm.Column1Checkboxes[9].Selected.Should().BeTrue();

            vm.Column2Checkboxes[4].Value.Should().Be("Initiative");
            vm.Column2Checkboxes[4].Selected.Should().BeTrue();

            vm.Column2Checkboxes[8].Value.Should().Be("1-Custom Skill 2");
            vm.Column2Checkboxes[8].Selected.Should().BeTrue();
        }

        [Fact]
        public void SetVacancyFromEditModel_ShouldOrderVacancySkills()
        {
            var helper = new SkillsOrchestratorHelper(GetBaseSkills);

            var vacancy = new Vacancy
            {
                Skills = new List<string>
                {
                    "Team worker"
                }
            };

            var m = new SkillsEditModelBase
            {
                Skills = new List<string>
                {
                    "1-Custom Skill 2",
                    "Initiative",
                    "Problem solving skills",
                    "2-Custom Skill 1",
                    "Administrative skills",
                    "Communication skills"
                },
                AddCustomSkillAction = "add custom skill",
                AddCustomSkillName = "Added Custom Skill"
            };
        
            helper.SetVacancyFromEditModel(vacancy, m);

            m.Skills.Count.Should().Be(7);
            m.Skills[0].Should().Be("Initiative");
            m.Skills[1].Should().Be("Problem solving skills");
            m.Skills[2].Should().Be("Administrative skills");
            m.Skills[3].Should().Be("Communication skills");
            m.Skills[4].Should().Be("1-Custom Skill 2");
            m.Skills[5].Should().Be("2-Custom Skill 1");
            m.Skills[6].Should().Be("3-Added Custom Skill");

            vacancy.Skills.Count.Should().Be(7);
            vacancy.Skills[0].Should().Be("Initiative");
            vacancy.Skills[1].Should().Be("Problem solving skills");
            vacancy.Skills[2].Should().Be("Administrative skills");
            vacancy.Skills[3].Should().Be("Communication skills");
            vacancy.Skills[4].Should().Be("Custom Skill 2");
            vacancy.Skills[5].Should().Be("Custom Skill 1");
            vacancy.Skills[6].Should().Be("Added Custom Skill");
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

    public class TestSkillsViewModel : ISkillsViewModel
    {
        public List<SkillViewModel> Column1Checkboxes { get; set; }
        public List<SkillViewModel> Column2Checkboxes { get; set; }
    }
}