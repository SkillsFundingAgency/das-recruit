namespace Esfa.Recruit.Vacancies.Client.Infrastructure.SequenceStore
{
    internal class Sequence
    {
        public string Id { get; internal set; }
        public long LastValue { get; internal set; }
    }
}