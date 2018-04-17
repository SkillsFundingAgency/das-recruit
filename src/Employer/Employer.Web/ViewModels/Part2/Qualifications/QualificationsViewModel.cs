using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications
{
    public class QualificationsViewModel : QualificationsEditModel
    {
        public string Title { get; internal set; }

        public List<string> QualificationTypes { get; set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(QualificationsEditModel.QualificationType),
            nameof(QualificationsEditModel.Subject),
            nameof(QualificationsEditModel.Grade),
            nameof(QualificationsEditModel.Weighting)
        };
    }
    
}
