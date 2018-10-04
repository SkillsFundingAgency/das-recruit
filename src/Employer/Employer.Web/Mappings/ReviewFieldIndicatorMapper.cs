using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Shared.Web.RuleTemplates;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FieldIdentifiers = Esfa.Recruit.Vacancies.Client.Domain.Entities.VacancyReview.FieldIdentifiers;

namespace Esfa.Recruit.Employer.Web.Mappings
{
    public sealed class ReviewFieldIndicatorMapper
    {
        private readonly IRuleMessageTemplateRunner _ruleTemplateRunner;

        public ReviewFieldIndicatorMapper(IRuleMessageTemplateRunner ruleTemplateRunner)
        {
            _ruleTemplateRunner = ruleTemplateRunner;
        }

        private IDictionary<string, string> ManualQaMessages => new Dictionary<string, string> 
        {
            { FieldIdentifiers.Title, "Title requires edit" },
            { FieldIdentifiers.ShortDescription, "Brief overview of the role requires edit" },
            { FieldIdentifiers.ClosingDate, "Closing date requires edit" },
            { FieldIdentifiers.WorkingWeek, "Working week requires edit" },
            { FieldIdentifiers.Wage, "Yearly wage requires edit" },
            { FieldIdentifiers.ExpectedDuration, "Expected duration requires edit" },
            { FieldIdentifiers.PossibleStartDate, "Possible start date requires edit" },
            { FieldIdentifiers.TrainingLevel, "Apprenticeship level requires edit" },
            { FieldIdentifiers.NumberOfPositions, "Positions requires edit" },
            { FieldIdentifiers.VacancyDescription, "Typical working day requires edit" },
            { FieldIdentifiers.TrainingDescription, "Training requires edit" },
            { FieldIdentifiers.OutcomeDescription, "Future prospects requires edit" },
            { FieldIdentifiers.Skills, "Skills requires edit" },
            { FieldIdentifiers.Qualifications, "Qualifications requires edit" },
            { FieldIdentifiers.ThingsToConsider, "Things to consider requires edit" },
            { FieldIdentifiers.EmployerDescription, "Employer description requires edit" },
            { FieldIdentifiers.DisabilityConfident, "Disability confident requires edit" },
            { FieldIdentifiers.EmployerWebsiteUrl, "Employer website requires edit" },
            { FieldIdentifiers.EmployerContact, "Contact details requires edit" },
            { FieldIdentifiers.EmployerAddress, "Employer address requires edit" },
            { FieldIdentifiers.Provider, "Training provider requires edit" },
            { FieldIdentifiers.Training, "Training requires edit" },
            { FieldIdentifiers.ApplicationMethod, "Application method requires edit" },
            { FieldIdentifiers.ApplicationUrl, "Apply now web address requires edit" },
            { FieldIdentifiers.ApplicationInstructions, "Application process requires edit" }
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

            var autoQaReferredOutcomes = review.AutomatedQaOutcome.RuleOutcomes
                .SelectMany(d => d.Details)
                .Where(x => autoQaReferredOutcomeIds.Contains(x.Id))
                .ToList();

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
                    indicator.Value.ManualQaText = ManualQaMessages[indicator.Key];
                }

                var autoQaOutcomes = autoQaReferredOutcomes.Where(x => pageMappings.VacancyPropertyMappingsLookup.TryGetValue(x.Target, out var value) && value.Contains(indicator.Key))
                    .Select(x => _ruleTemplateRunner.ToText(x.RuleId, x.Data, FieldDisplayNameResolver.Resolve(x.Target)));

                indicator.Value.AutoQaTexts.AddRange(autoQaOutcomes);
            }
            
            return indicatorsToDisplayLookup.Values;
        }
    }
}