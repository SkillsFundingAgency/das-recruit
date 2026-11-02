using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules;

public class VacancyRuleSet : RuleSet<Vacancy>
{
    public VacancyRuleSet(
        IProfanityListProvider profanityListProvider,
        IBannedPhrasesProvider bannedPhrasesProvider) : base(nameof(VacancyRuleSet))
    {
        AddRule(new VacancyProfanityChecksRule(profanityListProvider));
        AddRule(new VacancyBannedPhraseChecksRule(bannedPhrasesProvider));
        AddRule(new VacancyAnonymousCheckRule());
    }
}