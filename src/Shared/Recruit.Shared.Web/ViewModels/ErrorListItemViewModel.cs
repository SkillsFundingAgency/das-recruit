namespace Esfa.Recruit.Shared.Web.ViewModels
{
    public class ErrorListItemViewModel
    {
        public ErrorListItemViewModel(string elementId, string message)
        {
            ElementId = elementId;
            Message = message;
        }

        public string ElementId { get; }
        public string Message {get; }
    }
}
