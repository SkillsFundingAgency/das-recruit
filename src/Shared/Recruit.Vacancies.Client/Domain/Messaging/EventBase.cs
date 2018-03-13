namespace Esfa.Recruit.Vacancies.Client.Domain.Messaging
{
    public abstract class EventBase : IEvent
    {
        protected EventBase(string commandId)
        {
            SourceCommandId = commandId;
        }

        protected EventBase(CommandBase command)
        {
            SourceCommandId = command.CommandId.ToString();
        }

        public string SourceCommandId { get; }
    }
}
