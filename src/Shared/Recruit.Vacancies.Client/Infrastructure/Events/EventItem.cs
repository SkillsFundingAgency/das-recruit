namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Events
{
    public class EventItem
    {
        public string EventType { get; set; }
        public string Data { get; set; }
        public string SourceCommandId { get; set; }
    }
}