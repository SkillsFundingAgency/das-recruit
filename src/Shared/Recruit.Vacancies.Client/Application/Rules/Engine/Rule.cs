using System;
using Esfa.Recruit.Vacancies.Client.Application.Rules.BaseRules;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.Engine
{
    public abstract class Rule
    {
        private readonly decimal _weighting;

        protected Rule(string ruleId, decimal weighting = 1.0m)
        {
            if (string.IsNullOrWhiteSpace(ruleId))
                throw new ArgumentException("Null or empty rule ID passed to Rule", nameof(ruleId));

            RuleId = ruleId;
            _weighting = Math.Min(weighting, 100);
        }

        public string RuleId { get; }

        protected RuleOutcome CreateOutcome(int score, string narrative, ProfanityData data, string target = RuleOutcome.NoSpecificTarget)
        {
            return new RuleOutcome(RuleId, (int) (score * _weighting), narrative, target, null, data);
        }
    }
}
