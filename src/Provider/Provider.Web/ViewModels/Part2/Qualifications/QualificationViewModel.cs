using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.Qualifications
{
    public class QualificationViewModel : VacancyRouteModel
    {
        public string Title { get; internal set; }

        public IList<string> QualificationTypes { get; set; }
        public IList<Qualification> Qualifications { get; set; } = new List<Qualification>();

        public string QualificationType { get; set; }
        public string OtherQualificationName { get; set; }
        public int? Level { get; set; }
        public string Subject { get; set; }
        public string Grade { get; set; }
        public QualificationWeighting? Weighting { get; set; }
        public int? EditIndex { get; set; }
        public string CancelRoute { get; set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(QualificationType),
            nameof(OtherQualificationName),
            nameof(Level),
            nameof(Subject),
            nameof(Grade),
            nameof(Weighting)
        };

        public string BackRoute { get; set; }
        public bool IsFaaV2Enabled { get; set; }

        public class Qualification
        {
            public string Name { get; set; }
            public string Data { get; set; }
        }
    }
}