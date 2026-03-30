using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.ViewModels;

public class VacanciesListSearchFilterViewModel
{
    public required Dictionary<string, string> RouteDictionary { get; set; }
    public required string ResultsHeading { get; set; }
    public string SearchTerm { get; set; }
    public bool SuggestionsEnabled { get; set; }
    public string SuggestionsRoute { get; set; }
    public Dictionary<string, string> SuggestionsRouteDictionary { get; set; }
    public required UserType UserType { get; set; }
}