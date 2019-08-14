namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo
{
    public class LegalEntity
    {
        public long LegalEntityId { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public bool HasLegalEntityAgreement { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
    }
}
