using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.RuleTemplates;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.Mappers
{
    public sealed class ReviewFieldIndicatorMapper
    {
        private readonly IRuleMessageTemplateRunner _ruleTemplateRunner;
        private readonly bool _faaV2FeatureEnabled;

        public ReviewFieldIndicatorMapper(IRuleMessageTemplateRunner ruleTemplateRunner, IFeature feature)
        {
            _ruleTemplateRunner = ruleTemplateRunner;
            _faaV2FeatureEnabled = feature.IsFeatureEnabled("FaaV2Improvements");
        }

        private IDictionary<string, string> ManualQaMessagesForApprenticeship => new Dictionary<string, string> 
        {
            { FieldIdentifiers.Title, "Title requires edit" },
            { FieldIdentifiers.ShortDescription, "Brief overview requires edit" },
            { FieldIdentifiers.ClosingDate, "Closing date requires edit" },
            { FieldIdentifiers.Wage, "Annual wage requires edit" },
            { FieldIdentifiers.WorkingWeek, "Working week requires edit" },
            { FieldIdentifiers.ExpectedDuration, "Expected duration requires edit" },
            { FieldIdentifiers.PossibleStartDate, "Possible start date requires edit" },
            { FieldIdentifiers.TrainingLevel, "Apprenticeship level requires edit" },
            { FieldIdentifiers.NumberOfPositions, "Positions requires edit" },
            { FieldIdentifiers.VacancyDescription, _faaV2FeatureEnabled ?"What will the apprentice do at work requires edit":"What will the apprentice be doing requires edit" },
            { FieldIdentifiers.TrainingDescription, _faaV2FeatureEnabled ?"The apprentice's training schedule requires edit":"What training will the apprentice take and what qualification will the apprentice get at the end requires edit" },
            { FieldIdentifiers.AdditionalTrainingDescription, "Additional training information requires edit" },
            { FieldIdentifiers.OutcomeDescription, "What is the expected career progression after this apprenticeship requires edit" },
            { FieldIdentifiers.Skills, "Skills requires edit" },
            { FieldIdentifiers.Qualifications, "Qualifications requires edit" },
            { FieldIdentifiers.ThingsToConsider, "Things to consider requires edit" },
            { FieldIdentifiers.EmployerDescription, "Employer description requires edit" },
            { FieldIdentifiers.EmployerName, "Employer name requires edit" },
            { FieldIdentifiers.DisabilityConfident, "Disability confident requires edit" },
            { FieldIdentifiers.EmployerWebsiteUrl, "Employer website requires edit" },
            { FieldIdentifiers.EmployerContact, "Contact details requires edit" },
            { FieldIdentifiers.EmployerAddress, "Employer address requires edit" },
            { FieldIdentifiers.Provider, "Training provider requires edit" },
            { FieldIdentifiers.ProviderContact, "Contact details requires edit" },
            { FieldIdentifiers.Training, "Training requires edit" },
            { FieldIdentifiers.ApplicationMethod, "Application method requires edit" },
            { FieldIdentifiers.ApplicationUrl, "Apply now web address requires edit" },
            { FieldIdentifiers.ApplicationInstructions, "Application process requires edit" },
            { FieldIdentifiers.AdditionalQuestion1, "Additional question 1 requires edit" },
            { FieldIdentifiers.AdditionalQuestion2, "Additional question 2 requires edit" },
        };
        
        private IDictionary<string, string> ManualQaMessagesForTraineeship => new Dictionary<string, string> 
        {
            { FieldIdentifiers.Title, "Vacancy title requires edit" },
            { FieldIdentifiers.ShortDescription, "Summary of traineeship requires edit" },
            { FieldIdentifiers.ClosingDate, "Application closing date requires edit" },
            { FieldIdentifiers.Wage, "Annual wage requires edit" },
            { FieldIdentifiers.WorkingWeek, "Working week requires edit" },
            { FieldIdentifiers.ExpectedDuration, "Expected duration requires edit" },
            { FieldIdentifiers.PossibleStartDate, "Start date requires edit" },
            { FieldIdentifiers.NumberOfPositions, "Number of positions requires edit" },
            { FieldIdentifiers.VacancyDescription, "What will the apprentice be doing requires edit" },
            { FieldIdentifiers.TrainingDescription, "Training provided requires edit" },
            { FieldIdentifiers.OutcomeDescription, "Future prospects requires edit" },
            { FieldIdentifiers.Skills, "Skills requires edit" },
            { FieldIdentifiers.ThingsToConsider, "Other things to consider requires edit" },
            { FieldIdentifiers.EmployerDescription, "Employer information requires edit" },
            { FieldIdentifiers.EmployerName, "Employer name requires edit" },
            { FieldIdentifiers.DisabilityConfident, "Disability confident employer requires edit" },
            { FieldIdentifiers.EmployerWebsiteUrl, "Employer website requires edit" },
            { FieldIdentifiers.EmployerContact, "Contact details requires edit" },
            { FieldIdentifiers.EmployerAddress, "Work experience address requires edit" },
            { FieldIdentifiers.Provider, "Training provider requires edit" },
            { FieldIdentifiers.ProviderContact, "Contact details requires edit" },
            { FieldIdentifiers.WorkExperience, "Work experience provided requires edit" },
            { FieldIdentifiers.TraineeRoute, "Traineeship sector requires edit" }
        };

        public IEnumerable<ReviewFieldIndicatorViewModel> MapFromFieldIndicators(ReviewFieldMappingLookupsForPage pageMappings, VacancyReview review)
        {
            
            var manualQaFieldIdentifierNames = review.ManualQaFieldIndicators
                ?.Where(r => r.IsChangeRequested)
                .Select(r => r.FieldIdentifier)
                .ToList() ?? new List<string>();

            var autoQaReferredOutcomeIds = review.AutomatedQaOutcomeIndicators
                ?.Where(i => i.IsReferred)
                .Select(i => i.RuleOutcomeId)
                .ToList() ?? new List<Guid>();

            var autoQaReferredOutcomes = review.AutomatedQaOutcome?.RuleOutcomes
                .SelectMany(d => d.Details)
                .Where(x => autoQaReferredOutcomeIds.Contains(x.Id))
                .ToList() ?? new List<RuleOutcome>();

            var uniqueFieldIdentifierNames =
                manualQaFieldIdentifierNames.Union(
                    autoQaReferredOutcomes.SelectMany(x => pageMappings.VacancyPropertyMappingsLookup.TryGetValue(x.Target, out var value) ? value : Enumerable.Empty<string>()));

            var indicatorsToDisplayLookup = pageMappings.FieldIdentifiersForPage
                .Where(r => uniqueFieldIdentifierNames.Contains(r.ReviewFieldIdentifier))
                .ToDictionary(x => x.ReviewFieldIdentifier, y => y);

            foreach(var indicator in indicatorsToDisplayLookup)
            {
                if (manualQaFieldIdentifierNames.Contains(indicator.Key))
                {
                    indicator.Value.ManualQaText = review.VacancySnapshot.VacancyType.GetValueOrDefault() == VacancyType.Apprenticeship 
                        ? ManualQaMessagesForApprenticeship[indicator.Key] 
                        : ManualQaMessagesForTraineeship[indicator.Key];
                }

                var autoQaOutcomes = autoQaReferredOutcomes.Where(x => pageMappings.VacancyPropertyMappingsLookup.TryGetValue(x.Target, out var value) && value.Contains(indicator.Key))
                    .Select(x => _ruleTemplateRunner.ToText(x.RuleId, x.Data, FieldDisplayNameResolver.Resolve(x.Target)));

                indicator.Value.AutoQaTexts.AddRange(autoQaOutcomes);
            }
            
            return indicatorsToDisplayLookup.Values;
        }
    }
}