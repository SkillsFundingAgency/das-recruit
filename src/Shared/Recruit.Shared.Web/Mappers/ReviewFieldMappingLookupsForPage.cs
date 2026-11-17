using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Shared.Web.Mappers;

public class ReviewFieldMappingLookupsForPage(
    IReadOnlyList<ReviewFieldIndicatorViewModel> viewModels,
    IDictionary<string, IEnumerable<string>> vacancyMappings)
{
    public IReadOnlyList<ReviewFieldIndicatorViewModel> FieldIdentifiersForPage { get; } = viewModels;

    public IDictionary<string, IEnumerable<string>> VacancyPropertyMappingsLookup { get; } = vacancyMappings;
}