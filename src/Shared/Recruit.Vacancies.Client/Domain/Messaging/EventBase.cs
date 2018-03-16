namespace Esfa.Recruit.Vacancies.Client.Domain.Messaging
{
    public abstract class EventBase : IEvent
    {
        public string SourceCommandId { get; }
    }
}
