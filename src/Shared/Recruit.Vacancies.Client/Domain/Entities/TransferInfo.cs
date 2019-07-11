using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class TransferInfo
    {
        public long Ukprn { get; set; }
        public string ProviderName { get; set; }
        public string LegalEntityName { get; set; }
        public VacancyUser TransferredBy { get; set; }
        public DateTime TransferredDate { get; set; }
    }
}