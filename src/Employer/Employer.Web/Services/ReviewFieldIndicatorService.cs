using System.Linq;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Newtonsoft.Json;

namespace Esfa.Recruit.Employer.Web.Services;

public interface IReviewFieldIndicatorService
{
    void SetVacancyWithEmployerReviewFieldIndicators<T>(T currentValue, string fieldId, Vacancy vacancy, T newValue);
}

public class ReviewFieldIndicatorService: IReviewFieldIndicatorService
{
    public void SetVacancyWithEmployerReviewFieldIndicators<T>(T currentValue, string fieldId, Vacancy vacancy, T newValue)
    {
        PopulateEmployerReviewFieldIndicators(vacancy);

        if (JsonConvert.SerializeObject(currentValue).Equals(JsonConvert.SerializeObject(newValue)))
        {
            return;
        }
        
        foreach (string fieldIdentifier in ReviewFieldMappingLookups.GetPreviewReviewFieldIndicators().VacancyPropertyMappingsLookup[fieldId])
        {
            var changedIndicator = vacancy.EmployerReviewFieldIndicators.FirstOrDefault(p => p.FieldIdentifier == fieldIdentifier);
            if (changedIndicator != null)
            {
                changedIndicator.IsChangeRequested = true;
            }
        }
    }

    private static void PopulateEmployerReviewFieldIndicators(Vacancy vacancy)
    {
        var employerReviewFieldIndicators = vacancy.EmployerReviewFieldIndicators ?? [];

        // add field indicators which are missing
        var missingIndicators = ReviewFieldMappingLookups
            .GetPreviewReviewFieldIndicators().FieldIdentifiersForPage
            .Select(p => new EmployerReviewFieldIndicator { FieldIdentifier = p.ReviewFieldIdentifier, IsChangeRequested = false })
            .Where(p => employerReviewFieldIndicators.All(v => v.FieldIdentifier != p.FieldIdentifier)).ToList();

        if (missingIndicators.Count == 0)
        {
            return;
        }

        employerReviewFieldIndicators.AddRange(missingIndicators);
        vacancy.EmployerReviewFieldIndicators = employerReviewFieldIndicators.OrderBy(p => p.FieldIdentifier).ToList();
    }
}