using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public IList<VacancySummaryViewModel> Vacancies { get; internal set; }
        public string WarningMessage { get; internal set; }
        public string InfoMessage { get; internal set; }
        public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);
        public bool HasInfo => !string.IsNullOrEmpty(InfoMessage);
        public PagerViewModel Pager { get; internal set; }
        public string ResultsHeading { get; internal set; }
        public FilteringOptions Filter { get; set; }
        public bool HasVacancies { get; internal set; }
        public TransferredVacanciesAlertViewModel TransferredVacanciesAlert { get; internal set; }

        public bool ShowResultsTable => Vacancies.Any();
        public bool IsFiltered => Filter != FilteringOptions.All;
        public bool ShowTransferredVacanciesAlert => TransferredVacanciesAlert != null;
    }
}
