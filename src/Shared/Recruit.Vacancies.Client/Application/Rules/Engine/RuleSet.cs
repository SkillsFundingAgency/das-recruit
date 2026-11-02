using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;

/// <summary>
/// encapsulates a set of rules that will be applied to a particular entity.
/// the ruleset can be evaluated to produce a single overall outcome
/// </summary>
/// <typeparam name="TSubject"></typeparam>
public abstract class RuleSet<TSubject> : IRuleSet<TSubject>
{
    private readonly RuleSetOptions _defaultOptions;
    private readonly IList<IRule<TSubject>> _rules;

    public string RuleSetId { get; }
    public int RuleCount => _rules.Count;

    protected RuleSet(string ruleSetId)
    {
        RuleSetId = ruleSetId;
        _defaultOptions = RuleSetOptions.ZeroTolerance;
        _rules = new List<IRule<TSubject>>();
    }

    public async Task<RuleSetOutcome> EvaluateAsync(TSubject subject, RuleSetOptions options = null)
    {
        if (_rules.Count == 0)
            throw new InvalidOperationException($"RuleSet '{RuleSetId}' has no rules defined");

        options = options ?? _defaultOptions;

        var outcome = new RuleSetOutcome();

        foreach (var rule in _rules)
        {
            var ruleOutcome = await rule.EvaluateAsync(subject);

            outcome.RuleOutcomes.Add(ruleOutcome);
        }

        outcome.Decision = outcome.RuleOutcomes.CalculateOutcome(options);

        return outcome;
    }

    protected void AddRule(IRule<TSubject> rule)
    {
        _rules.Add(rule);
    }
}