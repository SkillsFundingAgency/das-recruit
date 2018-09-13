using Esfa.Recruit.Employer.Web.Views;
using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications
{
    public class QualificationsViewModel : QualificationsEditModel
    {
        public string Title { get; internal set; }

        public List<string> QualificationTypes { get; set; }
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
