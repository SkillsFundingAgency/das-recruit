using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Newtonsoft.Json;

namespace Esfa.Recruit.Provider.Web.Services;

public interface IReviewFieldIndicatorService
{
    void SetVacancyWithProviderReviewFieldIndicators<T>(T currentValue, string fieldId, Vacancy vacancy, T newValue);
}

public class ReviewFieldIndicatorService : IReviewFieldIndicatorService
{
    public void SetVacancyWithProviderReviewFieldIndicators<T>(T currentValue, string fieldId, Vacancy vacancy, T newValue)
    {
        if (vacancy.Status != VacancyStatus.Rejected)
        {
            return;
        }
        
        PopulateProviderReviewFieldIndicators(vacancy);
        if (JsonConvert.SerializeObject(currentValue).Equals(JsonConvert.SerializeObject(newValue)))
        {
            return;
        }
        
        foreach (string fieldIdentifier in ReviewFieldMappingLookups.GetPreviewReviewFieldIndicators().VacancyPropertyMappingsLookup[fieldId])
        {
            var changedIndicator = vacancy.ProviderReviewFieldIndicators.FirstOrDefault(p => p.FieldIdentifier == fieldIdentifier);
            if (changedIndicator is not null)
            {
                changedIndicator.IsChangeRequested = true;
            }
        }
    }

    private static void PopulateProviderReviewFieldIndicators(Vacancy vacancy)
    {
        var providerReviewFieldIndicators = vacancy.ProviderReviewFieldIndicators ?? new List<ProviderReviewFieldIndicator>();

        // add field indicators which are missing
        var missingIndicators = ReviewFieldMappingLookups
            .GetPreviewReviewFieldIndicators().FieldIdentifiersForPage
            .Select(p => new ProviderReviewFieldIndicator { FieldIdentifier = p.ReviewFieldIdentifier, IsChangeRequested = false })
            .Where(p => providerReviewFieldIndicators.All(v => v.FieldIdentifier != p.FieldIdentifier))
            .ToList();

        if (missingIndicators.Count == 0)
        {
            return;
        }
        providerReviewFieldIndicators.AddRange(missingIndicators);
        vacancy.ProviderReviewFieldIndicators = providerReviewFieldIndicators.OrderBy(p => p.FieldIdentifier).ToList();
    }
}