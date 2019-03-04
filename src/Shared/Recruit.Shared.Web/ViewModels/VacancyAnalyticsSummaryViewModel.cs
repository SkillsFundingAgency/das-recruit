using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;

namespace Esfa.Recruit.Shared.Web.ViewModels
{
    public class VacancyAnalyticsSummaryViewModel
    {
        public long? VacancyReference { get; internal set; }
        public DateTime LiveDate { get; internal set; }
        public DateTime LastUpdatedDate { get; internal set; }
        public string PeriodCaption => $"{LiveDate.AsGdsDate()} to {LastUpdatedDate.AsGdsDate()}";

        public VacancyStatus Status { get; internal set; }
        public ApplicationMethod? ApplicationMethod { get; internal set; }

        public int NoOfTimesAppearedInSearch { get; internal set; }
        public int NoOfTimesAppearedInSearchOverLastSevenDays { get; internal set; }
        public bool HasNoOfTimesAppearedInSearchIncreasedOverLastSevenDays => NoOfTimesAppearedInSearchOverLastSevenDays > 0;

        public int NoOfApplicationsStarted { get; internal set; }
        public int NoOfApplicationsStartedOverLastSevenDays { get; internal set; }
        public bool HasNoOfApplicationsStartedIncreasedOverLastSevenDays => NoOfApplicationsStartedOverLastSevenDays > 0;

        public int NoOfApplicationsSubmitted { get; internal set; }
        public int NoOfApplicationsSubmittedOverLastSevenDays { get; internal set; }
        public bool HasNoOfApplicationsSubmittedIncreasedOverLastSevenDays => NoOfApplicationsSubmittedOverLastSevenDays > 0;

        public int NoOfTimesViewed { get; internal set; }
        public int NoOfTimesViewedOverLastSevenDays { get; internal set; }
        public bool HasNoOfTimesViewedIncreasedOverLastSevenDays => NoOfTimesViewedOverLastSevenDays > 0;
    }
}