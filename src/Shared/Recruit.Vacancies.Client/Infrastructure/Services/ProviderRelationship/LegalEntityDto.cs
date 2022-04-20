namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship
{
    internal class LegalEntityDto
    {
        public long AccountId { get; set; }
        public string AccountHashedId { get; set; }
        public string AccountPublicHashedId { get; set; }
        public string AccountName { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public long AccountProviderId { get; set; }
    }
}