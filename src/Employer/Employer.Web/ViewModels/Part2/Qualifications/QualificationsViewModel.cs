using Esfa.Recruit.Employer.Web.Views;
using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications
{
    public class QualificationsViewModel : QualificationsEditModel
    {
        public string Title { get; internal set; }

        public List<string> QualificationTypes { get; set; }

        public bool HasQualifications => Qualifications.Any();

        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(QualificationType),
            nameof(Subject),
            nameof(Grade),
            nameof(Weighting)
        };
    }
}
