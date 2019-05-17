using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules
{
    public class VacancyRuleSet : RuleSet<Vacancy>
    {
        public VacancyRuleSet(QaRulesConfiguration qaRulesConfig,
                    IApprenticeshipProgrammeProvider apprenticeshipProgrammeProvider,
                    IProfanityListProvider profanityListProvider,
                    IBannedPhrasesProvider bannedPhrasesProvider,
                    IGetTitlePopularity popularityService) : base(nameof(VacancyRuleSet))
        {
            AddRule(new VacancyProfanityChecksRule(profanityListProvider));
            AddRule(new VacancyBannedPhraseChecksRule(bannedPhrasesProvider));
            AddRule(new VacancyTitlePopularityCheckRule(apprenticeshipProgrammeProvider, popularityService, qaRulesConfig));
            AddRule(new VacancyAnonymousCheckRule());
        }
    }
}