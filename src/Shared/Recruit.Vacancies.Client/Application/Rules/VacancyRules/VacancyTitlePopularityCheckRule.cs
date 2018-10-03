using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.QA.Core.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules.BaseRules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules
{
    public sealed class VacancyTitlePopularityCheckRule : Rule, IRule<Vacancy>
    {
        private readonly IApprenticeshipProgrammeProvider _apprenticeshipProgrammeProvider;
        private readonly IGetTitlePopularity _popularityService;
        private readonly QaRulesConfiguration _qaRulesConfig;

        public VacancyTitlePopularityCheckRule(IApprenticeshipProgrammeProvider apprenticeshipProgrammeProvider, IGetTitlePopularity popularityService, QaRulesConfiguration qaRulesConfig)
                : base(RuleId.TitlePopularity)
        {
            _apprenticeshipProgrammeProvider = apprenticeshipProgrammeProvider;
            _popularityService = popularityService;
            _qaRulesConfig = qaRulesConfig;
        }

        public async Task<RuleOutcome> EvaluateAsync(Vacancy subject)
        {
            RuleOutcome outcomeResult;

            var outcomeBuilder = RuleOutcomeDetailsBuilder.Create(RuleId.TitlePopularity);
            var popularityScore = await _popularityService.GetTitlePopularityAsync(subject.ProgrammeId, subject.Title);

            var trainingProgramme = await _apprenticeshipProgrammeProvider.GetApprenticeshipProgrammeAsync(subject.ProgrammeId);
            var data = new TitlePopularityData
            {
                VacancyTitle = subject.Title,
                TrainingCode = trainingProgramme.Id,
                TrainingTitle = trainingProgramme.Title,
                TrainingType = trainingProgramme.ApprenticeshipType.ToString().ToLower()
            };
            
            if (popularityScore < _qaRulesConfig.TitlePopularityPercentageThreshold)
                outcomeResult = new RuleOutcome(RuleId.TitlePopularity, 100, $"Title '{subject.Title}' is not common for the training specified.", nameof(Vacancy.Title), null, data);
            else
                outcomeResult =  new RuleOutcome(RuleId.TitlePopularity, 0, $"Title '{subject.Title}' is common for the training specified.", nameof(Vacancy.Title), null, data);

            var outcome = outcomeBuilder.Add(new RuleOutcome[]{outcomeResult})
                .ComputeSum();

            return outcome;
        }
    }
}