using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;

public interface IRule<in TSubject>
{
    Task<RuleOutcome> EvaluateAsync(TSubject subject);
}