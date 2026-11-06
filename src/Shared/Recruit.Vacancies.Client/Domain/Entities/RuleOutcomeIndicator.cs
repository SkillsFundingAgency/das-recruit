using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public class RuleOutcomeIndicator
{
    public Guid RuleOutcomeId { get; set; }

    public bool IsReferred { get; set; }
}