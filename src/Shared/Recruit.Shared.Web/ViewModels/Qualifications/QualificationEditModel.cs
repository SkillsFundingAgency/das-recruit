using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.ViewModels.Qualifications
{
    public class QualificationEditModel
    {
        public string QualificationType { get; set; }
        public string Subject { get; set; }
        public string Grade { get; set; }
        public int? Level { get; set; }
        public string OtherQualificationName { get; set; }
        public QualificationWeighting? Weighting { get; set; }
    }
}
