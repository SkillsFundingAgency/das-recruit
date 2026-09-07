using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class EmployerProfile
    {
        public string EmployerAccountId { get; set; }
        public string AboutOrganisation { get; set; }
        public string TradingName { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public IList<Address> OtherLocations { get; set; } = [];
    }
}