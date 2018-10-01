using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.QA.Core.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules.BaseRules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules
{
    public sealed class VacancyBannedPhraseChecksRule : BaseBannedPhraseChecksRule, IRule<Vacancy>
    {
        public VacancyBannedPhraseChecksRule(
            IBannedPhrasesProvider bannedPhrasesProvider, 
            BaseProfanityChecksRule.ConsolidationOption consolidationOption = BaseProfanityChecksRule.ConsolidationOption.NoConsolidation, 
            decimal weighting = 100.0m) 
            : base("BannedPhraseChecks", bannedPhrasesProvider, consolidationOption, weighting)
        { }

        public async Task<RuleOutcome> EvaluateAsync(Vacancy subject)
        {
            var outcomeBuilder = RuleOutcomeDetailsBuilder.Create(RuleId);

            var tasks = new List<Task<IEnumerable<RuleOutcome>>>
            {
                BannedPhraseCheckAsync(() => subject.Title),
                BannedPhraseCheckAsync(() => subject.ShortDescription),
                BannedPhraseCheckAsync(() => subject.EmployerLocation.AddressLine1),
                BannedPhraseCheckAsync(() => subject.EmployerLocation.AddressLine2),
                BannedPhraseCheckAsync(() => subject.EmployerLocation.AddressLine3),
                BannedPhraseCheckAsync(() => subject.EmployerLocation.AddressLine4),
                BannedPhraseCheckAsync(() => subject.Wage.WorkingWeekDescription),
                BannedPhraseCheckAsync(() => subject.Wage.WageAdditionalInformation),
                BannedPhraseCheckAsync(() => subject.Description),
                BannedPhraseCheckAsync(() => subject.TrainingDescription),
                BannedPhraseCheckAsync(() => subject.OutcomeDescription),
                BannedPhraseCheckAsync(() => subject.ThingsToConsider),
                BannedPhraseCheckAsync(() => subject.Skills.ToDelimitedString(","), "Skills"),
                BannedPhraseCheckAsync(() => subject.Qualifications.SelectMany(q => $"{q.Grade}, {q.Subject}").ToDelimitedString(","), "Qualifications"),
                BannedPhraseCheckAsync(() => subject.EmployerDescription),
                BannedPhraseCheckAsync(() => subject.EmployerContactName),
                BannedPhraseCheckAsync(() => subject.ApplicationInstructions)
            };


            await Task.WhenAll(tasks);

            var outcomes = tasks.SelectMany(t => t.Result);

            var outcome = outcomeBuilder.Add(outcomes)
                .ComputeSum();

            return outcome;
        }
    }
}
