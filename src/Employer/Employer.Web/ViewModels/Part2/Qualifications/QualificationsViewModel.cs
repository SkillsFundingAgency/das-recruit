using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels.Qualifications;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications
{
    public class QualificationsViewModel : VacancyRouteModel
    {
        public string Title { get; set; }
        public List<QualificationEditModel> Qualifications { get; set; }
        public string InfoMessage { get; set; }

        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public bool HasInfo => string.IsNullOrWhiteSpace(InfoMessage) == false;
        public bool IsTaskListCompleted { get ; set ; }

        public bool? AddQualificationRequirement { get; set; }
    }
}
