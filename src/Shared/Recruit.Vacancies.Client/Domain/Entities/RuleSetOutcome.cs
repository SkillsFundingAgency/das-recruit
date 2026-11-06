using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public class RuleSetOutcome
{
    public List<RuleOutcome> RuleOutcomes { get; set; } = new List<RuleOutcome>();
    public RuleSetDecision Decision { get; set; }
    public int TotalScore => RuleOutcomes.Sum(o => o.Score);
}