namespace Communication.Types
{
    public struct Entity
    {
        public string EntityType { get; }
        public object EntityId { get; }

        public Entity(string entityType, object entityId)
        {
            EntityType = entityType;
            EntityId = entityId;
        }
    }
}
