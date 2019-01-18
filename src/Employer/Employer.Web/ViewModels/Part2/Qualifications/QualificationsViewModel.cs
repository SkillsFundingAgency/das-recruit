using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications
{
    public class QualificationsViewModel : QualificationsEditModel
    {
        public string Title { get; internal set; }

        public IList<string> QualificationTypes { get; set; }

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
