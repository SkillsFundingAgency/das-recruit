using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.Extensions;

public static class RuleOutcomeExtensions
{
    public static RuleSetDecision CalculateOutcome(this List<RuleOutcome> ruleOutcomes, RuleSetOptions options)
    {
        var totalScore = ruleOutcomes.Sum(o => o.Score);

        if (totalScore > options.ReferralThreshold)
            return RuleSetDecision.Refer;
        if (totalScore < options.ApprovalThreshold)
            return RuleSetDecision.Approve;

        return RuleSetDecision.Indeterminate;
    }

}