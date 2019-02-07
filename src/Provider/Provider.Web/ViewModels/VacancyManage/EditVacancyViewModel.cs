using System;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyView;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;

namespace Esfa.Recruit.Provider.Web.ViewModels.VacancyManage
{
    public class EditVacancyViewModel : LiveVacancyViewModel
    {
        public string DisplayableClosingDate => ProposedClosingDate.HasValue ? ProposedClosingDate.Value.AsGdsDate() : ClosingDate;

        public string DisplayableStartDate => ProposedStartDate.HasValue ? ProposedStartDate.Value.AsGdsDate() : PossibleStartDate;

        public string ProposedClosingDateFormValue => ProposedClosingDate?.ToString("dd/MM/yyyy");
        public string ProposedStartDateFormValue => ProposedStartDate?.ToString("dd/MM/yyyy");

        public DateTime? ProposedClosingDate { get; internal set; }
        public DateTime? ProposedStartDate { get; internal set; }

        public bool ClosingDateChanged => ProposedClosingDate.HasValue;
        public bool StartDateChanged => ProposedStartDate.HasValue;

        public bool HasChanges => ClosingDateChanged || StartDateChanged;
        public bool IsUnchanged => !HasChanges;
    }
}