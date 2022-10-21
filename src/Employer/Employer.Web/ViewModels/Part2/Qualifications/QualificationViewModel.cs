using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications
{
    public class QualificationViewModel : VacancyRouteModel
    {
        public int Index { get; set; }
        public string Title { get; internal set; }

        public IList<string> QualificationTypes { get; set; }

        public string QualificationType { get; set; }
        public string Subject { get; set; }
        public string Grade { get; set; }
        public QualificationWeighting? Weighting { get; set; }
        public string CancelRoute { get; set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(QualificationType),
            nameof(Subject),
            nameof(Grade),
            nameof(Weighting)
        };

        public bool IsTaskListCompleted { get ; set ; }
        public string PostRoute { get; set; }
    }
}
