using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;

/// <summary>
/// used to build a single consolidated outcome from multiple rule executions 
/// (eg. when checking the content of multiple text fields).
/// total reported score for the rule is then either the mean or sum of all the rule outcomes added
/// </summary>
public class RuleOutcomeDetailsBuilder
{
    private readonly RuleId _ruleId;
    private int _totalScore;
    private int _outcomeCount;
    private readonly StringBuilder _narrative;
    private readonly List<RuleOutcome> _details;
    private readonly string _target;

    public int Count => _details.Count;

    public static RuleOutcomeDetailsBuilder Create(RuleId ruleId, string target = RuleOutcome.NoSpecificTarget)
    {
        return new RuleOutcomeDetailsBuilder(ruleId, target);
    }

    private RuleOutcomeDetailsBuilder(RuleId ruleId, string target)
    {
        _ruleId = ruleId;
        _totalScore = 0;
        _outcomeCount = 0;
        _narrative = new StringBuilder();
        _details = new List<RuleOutcome>();
        _target = target;
    }

    public RuleOutcomeDetailsBuilder Add(IEnumerable<RuleOutcome> outcomes)
    {
        foreach (var outcome in outcomes)
        {
            if (outcome.RuleId != _ruleId)
                throw new ArgumentException($"Invalid rule ID specified '{outcome.RuleId}' (does not match existing rule ID '{_ruleId}' in the outcome)", nameof(outcome.RuleId));

            _totalScore += outcome.Score;
            _outcomeCount++;
            _narrative.AppendLine(outcome.Narrative);
            _details.Add(outcome);
        }

        return this;
    }

    public RuleOutcomeDetailsBuilder Add(RuleOutcome outcome)
    {
        return Add(new[] {outcome});
    }

    public RuleOutcome ComputeSum(string narrative = null)
    {
        EnsureThereAreOutcomes();

        return new RuleOutcome(_ruleId, _totalScore, narrative ?? _narrative.ToString(), _target, _details);
    }

    public RuleOutcome ComputeMean(string narrative = null)
    {
        EnsureThereAreOutcomes();

        var meanScore = _totalScore / _outcomeCount;

        return new RuleOutcome(_ruleId, meanScore, narrative ?? _narrative.ToString(), _target, _details);
    }

    public RuleOutcome ComputeMax(string narrative = null)
    {
        EnsureThereAreOutcomes();

        var maxScore = _details.Max(d => d.Score);

        return new RuleOutcome(_ruleId, maxScore, narrative ?? _narrative.ToString(), _target, _details);
    }

    public RuleOutcome CreateOutcome(int score, string narrative = null)
    {
        return new RuleOutcome(_ruleId, score, narrative ?? _narrative.ToString(), _target, _details);
    }

    private void EnsureThereAreOutcomes()
    {
        if (_outcomeCount == 0)
            throw new InvalidOperationException("Cannot compute score from outcome details because there are no details");
    }
}