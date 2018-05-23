namespace Esfa.Recruit.Vacancies.Client.Domain.Messaging
{
    public abstract class EventBase : IEvent
    {
        public string SourceCommandId { get; set; }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
