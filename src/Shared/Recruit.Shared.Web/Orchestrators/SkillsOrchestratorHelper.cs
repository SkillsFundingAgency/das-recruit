using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.ViewModels.Skills;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.Orchestrators
{
    public class SkillsOrchestratorHelper
    {
        private const int ColumnOneCutOffIndex = 9;
        private const char SortPrefixSeparator = '-';

        private readonly Lazy<List<string>> _lazyCandidateSkills;
        
        private List<string> CandidateSkills => _lazyCandidateSkills.Value;
        private IEnumerable<string> Column1BuiltInSkills => CandidateSkills.Take(ColumnOneCutOffIndex);
        private IEnumerable<string> Column2BuiltInSkills => CandidateSkills.Skip(ColumnOneCutOffIndex);

        public SkillsOrchestratorHelper(Func<List<string>> getCandidateSkillsAsync)
        {
            _lazyCandidateSkills = new Lazy<List<string>>(getCandidateSkillsAsync);
        }

        public void SetViewModelSkillsFromVacancy(ISkillsViewModel vm, Vacancy vacancy)
        {
            var orderedCustomSkills = GetCustomSkills(vacancy.Skills).ToArray();
            var baseSkills = GetBaseSkills(vacancy.Skills).ToArray();

            SetViewModelSkills(vm, baseSkills, orderedCustomSkills);
        }

        public void SetViewModelSkillsFromDraftSkills(ISkillsViewModel vm, IList<string> draftSkills)
        {
            var orderedCustomSkills = ExtractAndSort(GetCustomSkills(draftSkills).ToArray());
            var baseSkills = GetBaseSkills(draftSkills).ToArray();

            SetViewModelSkills(vm, baseSkills, orderedCustomSkills);
        }

        public void SetVacancyFromEditModel(Vacancy vacancy, SkillsEditModelBase m)
        {
            if (m.Skills == null)
            {
                m.Skills = new List<string>();
            }

            var baseSkills = GetBaseSkills(m.Skills).ToList();
            var customSkills = GetCustomSkills(m.Skills);
            var sortedCustomSkills = ExtractAndSort(customSkills.ToArray()).ToList();

            HandleCustomSkillChange(m, baseSkills, sortedCustomSkills);

            vacancy.Skills = baseSkills.Union(sortedCustomSkills).ToList();

            // Adding in the ordering of the custom skill entries
            m.Skills = baseSkills.Union(AddOrdering(sortedCustomSkills)).ToList();
        }

        private IEnumerable<string> GetCustomSkills(IEnumerable<string> selected)
        {
            return selected == null ? new List<string>() : selected.Except(CandidateSkills);
        }

        private IEnumerable<string> GetBaseSkills(IEnumerable<string> selected)
        {
            return selected == null ? new List<string>() : selected.Intersect(CandidateSkills);
        }

        private void SetViewModelSkills(ISkillsViewModel vm, IList<string> baseSkills, IEnumerable<string> orderedCustomSkills)
        {
            var col1Skills = GetSkillsColumnViewModel(Column1BuiltInSkills, baseSkills);
            var col2Skills = GetSkillsColumnViewModel(Column2BuiltInSkills, baseSkills);

            var allCustomSkillViewModels = GetDraftSkillsColumnViewModel(orderedCustomSkills).ToList();
            var col1CustomSkillViewModels = allCustomSkillViewModels.Where((_, index) => index % 2 == 1);
            var col2CustomSkillViewModels = allCustomSkillViewModels.Where((_, index) => index % 2 == 0);

            vm.Column1Checkboxes = col1Skills.Union(col1CustomSkillViewModels).ToList();
            vm.Column2Checkboxes = col2Skills.Union(col2CustomSkillViewModels).ToList();
        }

        private IEnumerable<SkillViewModel> GetSkillsColumnViewModel(IEnumerable<string> skills, IEnumerable<string> selectedSkills)
        {
            return skills.Select(c => new SkillViewModel
            {
                Name = c,
                Selected = selectedSkills != null && selectedSkills.Any(s => s == c),
                Value = c
            });
        }

        private IEnumerable<SkillViewModel> GetDraftSkillsColumnViewModel(IEnumerable<string> draftSkills)
        {
            return draftSkills.Select((c, index) => new SkillViewModel
            {
                Name = c,
                Selected = true, // Always selected
                Value = $"{index + 1}-{c}"
            });
        }

        //
        // Custom skills have an order value pre-fixed to their name in order to retain their order.
        // This method extracts the name part and orders the returned list by the pre-fix
        //
        private static IEnumerable<string> ExtractAndSort(string[] skills)
        {
            if (skills == null || skills.Length == 0)
            {
                return skills ?? new string[0];
            }

            var skillNames = new SortedList<int, string>();

            foreach (var skillValue in skills)
            {
                var separatorIndex = skillValue.IndexOf(SortPrefixSeparator);
                var skillIndex = int.Parse(skillValue.Substring(0, separatorIndex));
                var skillName = skillValue.Substring(separatorIndex + 1);

                skillNames.Add(skillIndex, skillName);
            }

            return skillNames.Select(x => x.Value);
        }

        private void HandleCustomSkillChange(SkillsEditModelBase m, IList<string> baseSkillList, IList<string> customSkillList)
        {
            if (m.IsAddingCustomSkill)
            {
                // If the user has tried to add a custom skill that exists in the default skills list.
                if (CandidateSkills.Contains(m.AddCustomSkillName) && !baseSkillList.Contains(m.AddCustomSkillName))
                {
                    baseSkillList.Add(m.AddCustomSkillName);
                }
                else if (!CandidateSkills.Contains(m.AddCustomSkillName) && !customSkillList.Contains(m.AddCustomSkillName))
                {
                    customSkillList.Add(m.AddCustomSkillName);
                }
            }
        }

        private IEnumerable<string> AddOrdering(List<string> extractedCustomSkills)
        {
            return extractedCustomSkills.Select((c, index) => $"{index + 1}-{c}");
        }

    }
}
