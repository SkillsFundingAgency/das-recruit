using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Alerts;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer
{
    public class EmployerDashboard : QueryProjectionBase
    {
        public EmployerDashboard() : base(QueryViewType.EmployerDashboard.TypeName)
        {
        }

        public IEnumerable<VacancySummary> Vacancies { get; set; }
        public EmployerTransferredVacanciesAlertModel EmployerRevokedTransferredVacanciesAlert { get; set; } = new();
        public EmployerTransferredVacanciesAlertModel BlockedProviderTransferredVacanciesAlert { get; set; } = new();
        public BlockedProviderAlertModel BlockedProviderAlert { get; set; } = new();
        public WithdrawnVacanciesAlertModel WithDrawnByQaVacanciesAlert { get; set; } = new();
        public int? TotalVacancies { get; set; }
    }
}
