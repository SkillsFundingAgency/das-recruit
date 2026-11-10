using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;

internal interface IRuleSet<in TSubject>
{
    Task<RuleSetOutcome> EvaluateAsync(TSubject subject, RuleSetOptions options = null);
}