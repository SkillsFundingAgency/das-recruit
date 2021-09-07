using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public abstract class VacancyValidatingOrchestrator<TEditModel> : EntityValidatingOrchestrator<Vacancy, TEditModel>
    {
        protected VacancyValidatingOrchestrator(ILogger logger)
            : base(logger)
        {
        }

        protected void SetVacancyWithProviderReviewFieldIndicators<T>(T currentValue,
            string fieldId,
            Vacancy vacancy,
            Func<Vacancy, T> setFunc
            )
        {
            var newValue = setFunc(vacancy);
            
            if (vacancy.Status == VacancyStatus.Rejected)
            {
                PopulateProviderReviewFieldIndicators(vacancy);

                if (!JsonConvert.SerializeObject(currentValue).Equals(JsonConvert.SerializeObject(newValue)))
                {
                    foreach (var fieldIdentifier in ReviewFieldMappingLookups.GetPreviewReviewFieldIndicators().VacancyPropertyMappingsLookup[fieldId])
                    {
                        var changedIndicator = vacancy.ProviderReviewFieldIndicators.Where(p => p.FieldIdentifier == fieldIdentifier).FirstOrDefault();
                        if (changedIndicator != null)
                        {
                            changedIndicator.IsChangeRequested = true;
                        }
                    }
                }
            }
        }

        private void PopulateProviderReviewFieldIndicators(Vacancy vacancy)
        {
            var providerReviewFieldIndicators = vacancy.ProviderReviewFieldIndicators ?? new List<ProviderReviewFieldIndicator>();

            // add field indicators which are missing
            var missingIndicators =
                ReviewFieldMappingLookups
                    .GetPreviewReviewFieldIndicators().FieldIdentifiersForPage
                    .Select(p => new ProviderReviewFieldIndicator { FieldIdentifier = p.ReviewFieldIdentifier, IsChangeRequested = false })
                    .Where(p => !providerReviewFieldIndicators.Any(v => v.FieldIdentifier == p.FieldIdentifier));

            if (missingIndicators.Any())
            {
                providerReviewFieldIndicators
                    .AddRange(missingIndicators);

                vacancy.ProviderReviewFieldIndicators = providerReviewFieldIndicators
                    .OrderBy(p => p.FieldIdentifier).ToList();
            }
        }
    }
}
