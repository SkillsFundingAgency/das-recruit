using System;
using System.Collections.Generic;

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
        public string TradingName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public VacancyUser LastUpdatedBy { get; set; }
        public static string GetId(string employerAccountId, long legalEntityId)
        {
            return string.Format(IdFormat, employerAccountId, legalEntityId);
        }
        public static string GetId(string employerAccountId, string accountLegalEntityPublicHashedId)
        {
            return string.Format(IdFormat, employerAccountId, accountLegalEntityPublicHashedId);
        }
        public IList<Address> OtherLocations { get; set; } = new List<Address>();
        public string AccountLegalEntityPublicHashedId { get; set; }
    }
}
