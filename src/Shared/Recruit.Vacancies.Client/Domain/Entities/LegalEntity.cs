namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class LegalEntity
    {
        public long LegalEntityId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public Address Address { get; set; }
    }
}
