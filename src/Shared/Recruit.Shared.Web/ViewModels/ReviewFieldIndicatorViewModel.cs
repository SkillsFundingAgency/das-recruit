using System.Collections.Generic;

namespace Esfa.Recruit.Shared.Web.ViewModels;

public class ReviewFieldIndicatorViewModel(string reviewFieldIdentifier, string anchor)
{
    public string ReviewFieldIdentifier { get; } = reviewFieldIdentifier;
    public string Anchor { get; } = anchor;
    public string ManualQaText { get; set; }
    public List<string> AutoQaTexts { get; } = new();
}