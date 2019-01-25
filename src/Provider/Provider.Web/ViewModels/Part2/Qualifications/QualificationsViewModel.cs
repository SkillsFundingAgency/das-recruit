using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels.Qualifications;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.Qualifications
{
    public class QualificationsViewModel
    {
        public List<QualificationEditModel> Qualifications { get; set; }

        public string Title { get; internal set; }

        public IList<string> QualificationTypes { get; set; }

        public bool HasQualifications => Qualifications.Any();

        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public string QualificationType { get; set; }
        public string Subject { get; set; }
        public string Grade { get; set; }
        public QualificationWeighting? Weighting { get; set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(QualificationType),
            nameof(Subject),
            nameof(Grade),
            nameof(Weighting)
        };
    }
}
