using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.QA.Core.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
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
                ProfanityCheckAsync(() => subject.ShortDescription),
                ProfanityCheckAsync(() => subject.EmployerLocation.AddressLine1),
                ProfanityCheckAsync(() => subject.EmployerLocation.AddressLine2),
                ProfanityCheckAsync(() => subject.EmployerLocation.AddressLine3),
                ProfanityCheckAsync(() => subject.EmployerLocation.AddressLine4),
                ProfanityCheckAsync(() => subject.Wage.WorkingWeekDescription),
                ProfanityCheckAsync(() => subject.Wage.WageAdditionalInformation),
                ProfanityCheckAsync(() => subject.Description),
                ProfanityCheckAsync(() => subject.TrainingDescription),
                ProfanityCheckAsync(() => subject.OutcomeDescription),
                ProfanityCheckAsync(() => subject.ThingsToConsider),
                ProfanityCheckAsync(() => subject.Skills.ToDelimitedString(","), "Skills"),
                ProfanityCheckAsync(() => subject.Qualifications.SelectMany(q => $"{q.Grade}, {q.Subject}").ToDelimitedString(","), "Qualifications"),
                ProfanityCheckAsync(() => subject.EmployerDescription),
                ProfanityCheckAsync(() => subject.EmployerContactName),
                ProfanityCheckAsync(() => subject.ApplicationInstructions)
            };

            await Task.WhenAll(tasks);

            var outcomes = tasks.SelectMany(t => t.Result);

            var outcome = outcomeBuilder.Add(outcomes)
                .ComputeSum();

            return outcome;
        }
    }
}
