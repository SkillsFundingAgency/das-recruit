using System.Collections.Generic;

namespace Esfa.Recruit.Shared.Web.ViewModels;

public class VacanciesListSearchFilterViewModel
{
    public string ResultsHeading { get; set; }
    public string SearchTerm { get; set; }
    public bool SuggestionsEnabled { get; set; }
    public string SuggestionsRoute { get; set; }
    public Dictionary<string, string> SuggestionsRouteDictionary { get; set; }
    public long Ukprn { get; set; }
}