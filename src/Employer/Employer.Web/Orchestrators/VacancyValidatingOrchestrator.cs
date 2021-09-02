using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public abstract class VacancyValidatingOrchestrator<TEditModel> : EntityValidatingOrchestrator<Vacancy, TEditModel>
    {
        protected VacancyValidatingOrchestrator(ILogger logger)
            : base(logger)
        {
        }

        protected void SetVacancyWithEmployerReviewFieldIndicators<T>(T currentValue,
            string fieldId,
            Vacancy vacancy,
            Func<Vacancy, T> setFunc
            )
        {
            PopulateEmployerReviewFieldIndicators(vacancy);

            var newValue = setFunc(vacancy);
            if (!JsonConvert.SerializeObject(currentValue).Equals(JsonConvert.SerializeObject(newValue)))
            {
                foreach (var fieldIdentifier in ReviewFieldMappingLookups.GetPreviewReviewFieldIndicators().VacancyPropertyMappingsLookup[fieldId])
                {
                    var changedIndicator = vacancy.EmployerReviewFieldIndicators.Where(p => p.FieldIdentifier == fieldIdentifier).FirstOrDefault();
                    if (changedIndicator != null)
                    {
                        changedIndicator.IsChangeRequested = true;
                    }
                }
            }
        }

        private void PopulateEmployerReviewFieldIndicators(Vacancy vacancy)
        {
            var employerReviewFieldIndicators = vacancy.EmployerReviewFieldIndicators ?? new List<EmployerReviewFieldIndicator>();

            // add field indicators which are missing
            var missingIndicators =
                ReviewFieldMappingLookups
                    .GetPreviewReviewFieldIndicators().FieldIdentifiersForPage
                    .Select(p => new EmployerReviewFieldIndicator { FieldIdentifier = p.ReviewFieldIdentifier, IsChangeRequested = false })
                    .Where(p => !employerReviewFieldIndicators.Any(v => v.FieldIdentifier == p.FieldIdentifier));

            if (missingIndicators.Any())
            {
                employerReviewFieldIndicators
                    .AddRange(missingIndicators);

                vacancy.EmployerReviewFieldIndicators = employerReviewFieldIndicators
                    .OrderBy(p => p.FieldIdentifier).ToList();
            }
        }
    }
}
