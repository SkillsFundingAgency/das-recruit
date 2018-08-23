using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class DashboardViewModel
    {
        public IList<VacancySummary> Vacancies { get; set; }
        public string WarningMessage { get; internal set; }

        public bool ShowNoVacanciesMessage => !HasVacancies;
        public bool HasVacancies => Vacancies.Any();
        public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);

        public int NoOfVacancies => Vacancies.Count;
        public int NoOfDraftVacancies => Vacancies.Count(v => v.Status == VacancyStatus.Draft);
        public int NoOfSubmittedVacancies => Vacancies.Count(v => v.Status == VacancyStatus.Submitted);
        public int NoOfEditsRequiredVacancies => Vacancies.Count(v => v.Status == VacancyStatus.Referred);
        public int NoOfLiveVacancies => Vacancies.Count(v => v.Status == VacancyStatus.Live);
        public int NoOfClosedVacancies => Vacancies.Count(v => v.Status == VacancyStatus.Closed);

        public IEnumerable<VacancySummary> DisplayVacancies =>  Filter.HasValue
                                                                ? Vacancies.Where(v => v.Status == Filter.Value)
                                                                : Vacancies;

        public VacancyStatus? Filter { get; internal set; }

        public bool HasNoFilteredVacanciesToShow => Filter.HasValue && DisplayVacancies.Any() == false;
        public string FilterDisplayText => Filter.HasValue ? Filter.Value.GetDisplayName().ToLower() : string.Empty;
    }
}
