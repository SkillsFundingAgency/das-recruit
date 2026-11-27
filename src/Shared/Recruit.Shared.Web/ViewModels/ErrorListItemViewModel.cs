using System.Collections.Generic;

namespace Esfa.Recruit.Shared.Web.ViewModels;

public class ErrorListItemViewModel(string elementId, string message)
{
    public string ElementId { get; } = elementId;
    public string Message {get; } = message;

    // The FocusIdMap is used to map certain element IDs to their corresponding focusable element IDs. This is useful for accessibility purposes, ensuring that when an error message is displayed, the focus can be set to the appropriate input field or control associated with that error.
    private static readonly Dictionary<string, string> FocusIdMap = new()
    {
        ["ClosingDate"] = "ClosingDay",
        ["StartDate"] = "StartDay",
        ["WageType"] = "wage-type-fixed",
        ["SelectedLocation"] = "selected-location",
        ["SelectedLocations"] = "selected-location",
        ["SelectedAvailability"] = "OneLocation",
        ["Skills"] = "skills-selection",
        ["SelectedEmployerIdentityOption"] = "legal-entity-name",
        ["ApplicationMethod"] = "application-method-faa",
    };

    public string FocusElementId =>
        FocusIdMap.GetValueOrDefault(ElementId ?? string.Empty, ElementId);
}