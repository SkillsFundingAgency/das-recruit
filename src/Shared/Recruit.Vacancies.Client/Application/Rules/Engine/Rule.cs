using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;

public abstract class Rule(RuleId ruleId, decimal weighting = 1.0m)
{
    private readonly decimal _weighting = Math.Min(weighting, 100);

    public RuleId RuleId { get; } = ruleId;

    protected RuleOutcome CreateOutcome(int score, string narrative, string data, string target = RuleOutcome.NoSpecificTarget)
    {
        return new RuleOutcome(RuleId, (int) (score * _weighting), narrative, target, null, data);
    }
}