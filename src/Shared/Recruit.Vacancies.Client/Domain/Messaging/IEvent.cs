namespace Esfa.Recruit.Vacancies.Client.Domain.Messaging
{
    public interface IEvent
    {
        string SourceCommandId { get; }
    }
}