﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules.BaseRules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules
{
    public sealed class VacancyBannedPhraseChecksRule : BaseBannedPhraseChecksRule, IRule<Vacancy>
    {
        private readonly IBannedPhrasesProvider _bannedPhrasesProvider;

        public VacancyBannedPhraseChecksRule(
            IBannedPhrasesProvider bannedPhrasesProvider, 
            ConsolidationOption consolidationOption = ConsolidationOption.NoConsolidation, 
            decimal weighting = 100.0m) 
            : base(RuleId.BannedPhraseChecks, consolidationOption, weighting)
        {
            _bannedPhrasesProvider = bannedPhrasesProvider;
        }

        public async Task<RuleOutcome> EvaluateAsync(Vacancy subject)
        {
            var outcomeBuilder = RuleOutcomeDetailsBuilder.Create(RuleId);

            BannedPhrases = await _bannedPhrasesProvider.GetBannedPhrasesAsync();

            var outcomes = new List<RuleOutcome>();
            outcomes.AddRange(BannedPhraseCheck(() => subject.Title));
            outcomes.AddRange(BannedPhraseCheck(() => subject.ShortDescription));
            outcomes.AddRange(BannedPhraseCheck(() => subject.EmployerLocation.AddressLine1));
            outcomes.AddRange(BannedPhraseCheck(() => subject.EmployerLocation.AddressLine2));
            outcomes.AddRange(BannedPhraseCheck(() => subject.EmployerLocation.AddressLine3));
            outcomes.AddRange(BannedPhraseCheck(() => subject.EmployerLocation.AddressLine4));
            outcomes.AddRange(BannedPhraseCheck(() => subject.Wage.WorkingWeekDescription));
            outcomes.AddRange(BannedPhraseCheck(() => subject.Wage.WageAdditionalInformation));
            outcomes.AddRange(BannedPhraseCheck(() => subject.Description));
            outcomes.AddRange(BannedPhraseCheck(() => subject.TrainingDescription));
            outcomes.AddRange(BannedPhraseCheck(() => subject.OutcomeDescription));
            outcomes.AddRange(BannedPhraseCheck(() => subject.ThingsToConsider));
            outcomes.AddRange(BannedPhraseCheck(() => subject.Skills.ToDelimitedString(","), "Skills"));
            outcomes.AddRange(BannedPhraseCheck(() => 
                subject.Qualifications.SelectMany(q => new [] {q.Grade, q.Subject}).ToDelimitedString(",")
                , "Qualifications"));
            outcomes.AddRange(BannedPhraseCheck(() => subject.EmployerDescription));
            outcomes.AddRange(BannedPhraseCheck(() => subject.EmployerContact.Name));
            outcomes.AddRange(BannedPhraseCheck(() => subject.ApplicationInstructions));

            var outcome = outcomeBuilder.Add(outcomes)
                .ComputeSum();

            return outcome;
        }
    }
}
