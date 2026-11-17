namespace Esfa.Recruit.Shared.Web.ViewModels;

public class ErrorListItemViewModel(string elementId, string message)
{
    public string ElementId { get; } = elementId;
    public string Message {get; } = message;
}