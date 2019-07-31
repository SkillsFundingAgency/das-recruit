using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string IdamsUserId { get; set; } 
        public UserType UserType { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastSignedInDate { get; set; }
        public IList<string> AccountsDeclaredAsLevyPayers { get; set; } = new List<string>();
        public IList<string> EmployerAccountIds { get; set; } = new List<string>();
        public long? Ukprn { get; set; }
        public DateTime? TransferredVacanciesAlertDismissedOn { get; set; }
        public DateTime? BlockedProviderAlertDismissedOn { get; set; }
        public DateTime? BlockedProviderTransferredVacanciesAlertDismissedOn { get; set; }
        public DateTime? WithdrawnByQaVacanciesAlertDismissedOn { get; set; }
    }
}
