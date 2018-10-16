using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class EmployerProfile
    {
        public Guid Id { get; set; }
        public string EmployerAccountId { get; set; }
        public long LegalEntityId { get; set; }
        public string AboutOrganisation { get; set; }
        public string OrganistationWebsiteUrl { get; set; }
    }
}
