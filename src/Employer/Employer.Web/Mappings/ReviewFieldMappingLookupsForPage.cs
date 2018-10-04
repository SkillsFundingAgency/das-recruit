using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;

namespace Esfa.Recruit.Employer.Web.Mappings
{
    public class ReviewFieldMappingLookupsForPage
    {
        public ReviewFieldMappingLookupsForPage(IReadOnlyList<ReviewFieldIndicatorViewModel> viewModels, IDictionary<string, IEnumerable<string>> vacancyMappings)
        {
            FieldIdentifiersForPage = viewModels;
            VacancyPropertyMappingsLookup = vacancyMappings;
        }

        public IReadOnlyList<ReviewFieldIndicatorViewModel> FieldIdentifiersForPage { get; }

        public IDictionary<string, IEnumerable<string>> VacancyPropertyMappingsLookup { get; }
    }
}