namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

/// <summary>
/// overall decision for a rule set evaluation
/// </summary>
public enum RuleSetDecision
{
    Unknown = 0,
    Refer,
    Approve,
    Indeterminate
}