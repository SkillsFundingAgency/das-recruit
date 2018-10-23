using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class EmployerProfile
    {
        private const string IdFormat = "{0}_{1}";

        public string Id => GetId(EmployerAccountId, LegalEntityId);

        public string EmployerAccountId { get; set; }
        public long LegalEntityId { get; set; }
        public string AboutOrganisation { get; set; }
        public string OrganistationWebsiteUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public VacancyUser LastUpdatedBy { get; set; }

        public static string GetId(string employerAccountId, long legalEntityId)
        {
            return string.Format(IdFormat, employerAccountId, legalEntityId); 
        }
    }
}
