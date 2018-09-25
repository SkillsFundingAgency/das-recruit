using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.QA.Core.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Rules.BaseRules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules

{
    public sealed class VacancyProfanityChecksRule : BaseProfanityChecksRule, IRule<Vacancy>
    {
        public VacancyProfanityChecksRule(IProfanityListProvider profanityListProvider, ConsolidationOption consolidationOption = ConsolidationOption.NoConsolidation, decimal weighting = 100.0m)
            : base("ProfanityChecks", profanityListProvider, consolidationOption, weighting) { }

        public async Task<RuleOutcome> EvaluateAsync(Vacancy subject)
        {
            var outcomeBuilder = RuleOutcomeDetailsBuilder.Create(RuleId);

            var tasks = new List<Task<IEnumerable<RuleOutcome>>>
            {
                ProfanityCheckAsync(() => subject.Title),
                ProfanityCheckAsync(() => subject.Description),
                ProfanityCheckAsync(() => subject.Skills.ToDelimitedString(","), "Skills")
            };

            await Task.WhenAll(tasks);

            var outcomes = tasks.SelectMany(t => t.Result);

            var outcome = outcomeBuilder.Add(outcomes)
                .ComputeSum();

            return outcome;
        }
    }
}
