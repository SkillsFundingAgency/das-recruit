using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels.Qualifications;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.Qualifications
{
    public class QualificationsEditModel : QualificationEditModel
    {
        public List<QualificationEditModel> Qualifications { get; set; }

        public string AddQualificationAction { get; set; }
        public string RemoveQualification { get; set; }

        public bool IsAddingQualification => !string.IsNullOrWhiteSpace(AddQualificationAction);

        public bool IsRemovingQualification => !string.IsNullOrEmpty(RemoveQualification);
    }
}
