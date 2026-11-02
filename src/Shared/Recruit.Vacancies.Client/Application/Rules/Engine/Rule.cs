using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;

public abstract class Rule
{
    private readonly decimal _weighting;

    protected Rule(RuleId ruleId, decimal weighting = 1.0m)
    {
        RuleId = ruleId;
        _weighting = Math.Min(weighting, 100);
    }

    public RuleId RuleId { get; }

    protected RuleOutcome CreateOutcome(int score, string narrative, string data, string target = RuleOutcome.NoSpecificTarget)
    {
        return new RuleOutcome(RuleId, (int) (score * _weighting), narrative, target, null, data);
    }
}