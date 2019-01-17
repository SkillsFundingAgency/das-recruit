using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Shared.Web.Mappers
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